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
  
  public static Thought<TProduct, TFeedback> Capture<TProduct, TFeedback>(TProduct Product, TFeedback Feedback)
  {
    return new(Product, Feedback, null, new());
  }

  public static Thought<TProduct, NullFeedback> Capture<TProduct>(TProduct Product) =>
    Capture(Product, NullFeedback.Instance);

  public static class WithFeedback<TFeedback>
  {
    public static Thought<TProduct, TFeedback> Think<TProduct>(
      Func<Thought<TFeedback>.Reasoning, (TProduct Product, TFeedback Feedback)> Produce)
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

    public static async Task<Thought<TProduct, TFeedback>> ThinkAsync<TProduct>(
      Func<Thought<TFeedback>.Reasoning, Task<(TProduct, TFeedback)>> Produce)
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

    public static Thought<TFeedback> Do(Action<Thought<TFeedback>.Reasoning> ToDo)
    {
      return Think<object?>(R =>
      {
        ToDo(R);
        return (null, R.Feedback!);
      });
    }

    public static async Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
    {
      var Result = await ThinkAsync(async Task<(object?, TFeedback)> (R) =>
      {
        await ToDo(R);

        return (null, R.Feedback!);
      });

      return Result;
    }
  }

  public static Thought<TProduct, TFeedback> Think<TProduct, TFeedback>(
    Func<Reasoning, (TProduct Product, TFeedback Feedback)> Produce)
  {
    return WithFeedback<TFeedback>.Think(Produce);
  }

  public static Thought Do(Action<Reasoning> ToDo)
  {
    return WithFeedback<object?>.Do(ToDo);
  }

  public static Thought<TFeedback> Do<TFeedback>(Func<Reasoning, TFeedback> ToDo)
  {
    return WithFeedback<TFeedback>.Think<object?>(R =>
    {
      var Feedback = ToDo(R);
      return (null, Feedback);
    });
  }

  public static Thought Done()
  {
    return Do(R => { });
  }

  public static Task<Thought<TProduct, TFeedback>> ThinkAsync<TProduct, TFeedback>(
    Func<Reasoning, Task<(TProduct, TFeedback)>> Produce)
  {
    return WithFeedback<TFeedback>.ThinkAsync(Produce);
  }

  public static Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
  {
    return WithFeedback<NullFeedback>.DoAsync(ToDo);
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