// MIT License
// 
// Copyright (c) 2025-2025 Hexagon Software LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace ThoughtSharp.Runtime;

public abstract partial class Thought : IDisposable
{
  readonly IReadOnlyList<IDisposable> Disposables;
  readonly ExceptionDispatchInfo? ExceptionInfo;
  readonly IReadOnlyDictionary<Thought, float> Weights;

  // prevents foreign inheritors
  internal Thought(Reasoning LineOfReasoning, ExceptionDispatchInfo? ExceptionInfo)
  {
    this.ExceptionInfo = ExceptionInfo;
    Children = LineOfReasoning.Children;
    Weights = LineOfReasoning.ChildrenWeights;
    Disposables = LineOfReasoning.DisposableResources;
  }

  internal Reasoning? Container { get; private set; }

  public Thought? Parent => Container?.Parent;
  public IReadOnlyList<Thought> Children { get; }

  public void Dispose()
  {
    foreach (var Child in Children)
      Child.Dispose();

    foreach (var Disposable in Disposables)
      Disposable.Dispose();
  }
  
  public static Thought<TProduct, TFeedback> Capture<TProduct, TFeedback>(TProduct Product, TFeedback Feedback, TrainingPolicy? Training = null)
  {
    return new(Product, Feedback, null, new());
  }

  public static Thought<TProduct, NullFeedback> Capture<TProduct>(TProduct Product) =>
    Capture(Product, NullFeedback.Instance);

  public struct WithEarlyFeedbackFactory<TFeedback>
  {
    public Thought<TProduct, TFeedback> Think<TProduct>(
      Func<Thought<TFeedback>.Reasoning, (TProduct Product, TFeedback Feedback)> Produce,
      TrainingPolicy? Policy = null)
    {
      var Reasoning = new Thought<TFeedback>.Reasoning();
      TProduct? Product;
      TFeedback? FeedbackPolicy;

      ExceptionDispatchInfo? ExceptionInfo;
      try
      {
        (Product, FeedbackPolicy) = Produce(Reasoning);
        ExceptionInfo = null;
      }
      catch (Exception Exception)
      {
        (Product, FeedbackPolicy) = (default, default);
        ExceptionInfo = ExceptionDispatchInfo.Capture(Exception);
      }

      FeedbackPolicy ??= Reasoning.Feedback;

        var Result = new Thought<TProduct, TFeedback>(Product, FeedbackPolicy, ExceptionInfo, Reasoning);
      Reasoning.Parent = Result;
      return Result;
    }
  }

  public static WithEarlyFeedbackFactory<TFeedback> WithEarlyFeedback<TFeedback>() => new();

  public static Thought<TProduct, TFeedback> Think<TProduct, TFeedback>(
    Func<Thought.Reasoning, (TProduct Product, TFeedback Feedback)> Produce,
    TrainingPolicy? Policy = null)
  {
    var Reasoning = new Thought<TFeedback>.Reasoning();
    TProduct? Product;
    TFeedback? FeedbackPolicy;

    ExceptionDispatchInfo? ExceptionInfo;
    try
    {
      (Product, FeedbackPolicy) = Produce(Reasoning);
      ExceptionInfo = null;
    }
    catch (Exception Exception)
    {
      (Product, FeedbackPolicy) = (default, default);
      ExceptionInfo = ExceptionDispatchInfo.Capture(Exception);
    }

    var Result = new Thought<TProduct, TFeedback>(Product, FeedbackPolicy, ExceptionInfo, Reasoning);
    Reasoning.Parent = Result;
    return Result;
  }

  public static Thought Do(Action<Reasoning> ToDo)
  {
    return Think<object?, object>(R =>
    {
      ToDo(R);
      return (null, null!);
    });
  }

  public static Thought<TFeedback> Do<TFeedback>(Func<Reasoning, TFeedback> ToDo)
  {
    return Think<object?, TFeedback>(R =>
    {
      var Feedback = ToDo(R);
      return (null, Feedback);
    });
  }

  public static Thought Done()
  {
    return Do(R => { });
  }

  public static async Task<Thought<TProduct, TFeedback>> ThinkAsync<TProduct, TFeedback>(
    Func<Reasoning, Task<(TProduct, TFeedback)>> Produce, 
    TrainingPolicy? Policy = null)
  {
    var Reasoning = new Thought<TFeedback>.Reasoning();
    TProduct? Product;
    TFeedback? Feedback;

    ExceptionDispatchInfo? ExceptionInfo;
    try
    {
      (Product, Feedback) = await Produce(Reasoning);
      ExceptionInfo = null;
    }
    catch (Exception Exception)
    {
      Product = default;
      Feedback = default;
      ExceptionInfo = ExceptionDispatchInfo.Capture(Exception);
    }

    var Result = new Thought<TProduct, TFeedback>(Product, Feedback, ExceptionInfo, Reasoning);
    Reasoning.Parent = Result;
    return Result;
  }

  public static async Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
  {
    var Result = await ThinkAsync(async Task<(object?, object?)> (R) =>
    {
      await ToDo(R);

      return (null, null);
    });

    return Result;
  }

  public void RaiseAnyExceptions()
  {
    if (ExceptionInfo is not null)
      ExceptionInfo.Throw();
  }
}

public abstract partial class Thought<TFeedback>(
  Thought.Reasoning LineOfReasoning,
  ExceptionDispatchInfo? ExceptionInfo,
  TFeedback? Feedback)
  : Thought(LineOfReasoning, ExceptionInfo)
{
  public TFeedback Feedback { get; } = Feedback!;
}

public sealed class Thought<TProduct, TFeedback> : Thought<TFeedback>
{
  readonly TProduct? Product;

  internal Thought(
    TProduct? Product, 
    TFeedback? Feedback,
    ExceptionDispatchInfo? ExceptionInfo, 
    Reasoning LineOfReasoning)
    : base(LineOfReasoning, ExceptionInfo, Feedback)
  {
    this.Product = Product;
  }

  public TProduct ConsumeDetached()
  {
    return new Reasoning().Consume(this);
  }

  internal TProduct GetProduct()
  {
    return Product!;
  }
}