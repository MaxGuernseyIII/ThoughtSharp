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

using System.Runtime.ExceptionServices;

namespace ThoughtSharp.Runtime;

public abstract partial class Thought : IDisposable
{
  readonly IReadOnlyList<IDisposable> Disposables;
  readonly ExceptionDispatchInfo? ExceptionInfo;
  readonly IReadOnlyDictionary<Thought, float> Weights;

  // prevents foreign inheritors
  internal Thought(Reasoning LineOfReasoning, ExceptionDispatchInfo? ExceptionInfo,
    TrainingPolicy? TrainingPolicy)
  {
    this.TrainingPolicy = TrainingPolicy;
    this.ExceptionInfo = ExceptionInfo;
    Children = LineOfReasoning.Children;
    Weights = LineOfReasoning.ChildrenWeights;
    Disposables = LineOfReasoning.DisposableResources;
  }

  internal TrainingPolicy? TrainingPolicy { get; set; }

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

  public static Thought Done(TrainingPolicy? Training = null)
  {
    return new Thought<object?, object?>(null, null, null, new(), Training);
  }

  public static Thought<TProduct, TFeedback> Capture<TProduct, TFeedback>(TProduct Product, FeedbackPolicy<TFeedback>? Feedback = null, TrainingPolicy? Training = null)
  {
    return new(Product, Feedback, null, new(), Training);
  }

  public static Thought<TProduct, TFeedback> Think<TProduct, TFeedback>(
    Func<Reasoning, (TProduct Product, FeedbackPolicy<TFeedback> Feedback)> Produce,
    TrainingPolicy? Policy = null)
  {
    var Reasoning = new Reasoning();
    TProduct? Product;
    FeedbackPolicy<TFeedback> FeedbackPolicy;

    ExceptionDispatchInfo? ExceptionInfo;
    try
    {
      (Product, FeedbackPolicy) = Produce(Reasoning);
      ExceptionInfo = null;
    }
    catch (Exception Exception)
    {
      (Product, FeedbackPolicy) = (default, null);
      ExceptionInfo = ExceptionDispatchInfo.Capture(Exception);
    }

    var Result = new Thought<TProduct, TFeedback>(Product, FeedbackPolicy, ExceptionInfo, Reasoning, Policy);
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

  public static async Task<Thought<TProduct, TFeedback>> ThinkAsync<TProduct, TFeedback>(
    Func<Reasoning, Task<(TProduct, FeedbackPolicy<TFeedback>)>> Produce, 
    TrainingPolicy? Policy = null)
  {
    var Reasoning = new Reasoning();
    TProduct? Product;
    FeedbackPolicy<TFeedback>? Feedback;

    ExceptionDispatchInfo? ExceptionInfo;
    try
    {
      (Product, Feedback) = await Produce(Reasoning);
      ExceptionInfo = null;
    }
    catch (Exception Exception)
    {
      Product = default;
      Feedback = null;
      ExceptionInfo = ExceptionDispatchInfo.Capture(Exception);
    }

    var Result = new Thought<TProduct, TFeedback>(Product, Feedback, ExceptionInfo, Reasoning, Policy);
    Reasoning.Parent = Result;
    return Result;
  }

  public static async Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
  {
    var Result = await ThinkAsync(async Task<(object?, FeedbackPolicy<object?>)> (R) =>
    {
      await ToDo(R);

      return (null, null!);
    });

    return Result;
  }

  public void ApplyIncentive(float Reward)
  {
    var ThoughtGraph = Thought.ThoughtGraph.For(this);
    foreach (var Rewarded in ThoughtGraph.RewardedWithoutMind)
      Rewarded.IncentivizeOutput(Reward);

    foreach (var (_, Sequence) in ThoughtGraph.RewardedWithMind)
    {
      foreach (var Rewarded in Sequence[..^1])
        Rewarded.IncentivizeOutputAndState(Reward);

      Sequence[^1].IncentivizeOutput(Reward);
    }
  }

  public void RaiseAnyExceptions()
  {
    if (ExceptionInfo is not null)
      ExceptionInfo.Throw();
  }
}

public sealed class Thought<TProduct, TFeedback> : Thought
{
  readonly TProduct? Product;

  internal Thought(
    TProduct? Product, 
    FeedbackPolicy<TFeedback>? Feedback,
    ExceptionDispatchInfo? ExceptionInfo, 
    Reasoning LineOfReasoning,
    TrainingPolicy? TrainingPolicy)
    : base(LineOfReasoning, ExceptionInfo, TrainingPolicy)
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