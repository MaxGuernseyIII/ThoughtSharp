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

namespace ThoughtSharp.Runtime;

public abstract partial class Thought : IDisposable
{
  readonly IReadOnlyList<IDisposable> Disposables;
  readonly ThoughtResult Result;
  readonly IReadOnlyDictionary<Thought, float> Weights;

  // prevents foreign inheritors
  internal Thought(Reasoning LineOfReasoning, ThoughtResult Result)
  {
    this.Result = Result;
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
    return new(ThoughtResult.FromOutput(Product, Feedback), new());
  }

  public static Thought<TProduct, NullFeedback> Capture<TProduct>(TProduct Product)
  {
    return Capture(Product, NullFeedback.Instance);
  }

  public static Thought<TProduct, TFeedback> Think<TProduct, TFeedback>(
    Func<Reasoning, ThoughtResult<TProduct, TFeedback>> Produce)
  {
    var Reasoning = new Reasoning();
    var ThoughtResult = Produce(Reasoning);
    var Result = new Thought<TProduct, TFeedback>(ThoughtResult, Reasoning);
    Reasoning.Parent = Result;
    return Result;
  }

  public static Thought<TFeedback> Do<TFeedback>(Func<Reasoning, TFeedback> ToDo)
  {
    return Think(R => ThoughtResult
      .WithFeedbackSource(FeedbackSource.FromFunc(() => ToDo(R)))
      .FromLogic(object? (_) => null)
    );
  }

  public static async Task<Thought<TFeedback>> DoAsync<TFeedback>(Func<Reasoning, Task<TFeedback>> ToDo)
  {
    return await ThinkAsync(R => ThoughtResult
      .WithFeedbackSource(FeedbackSource.Deferred<TFeedback>())
      .FromLogicAsync(async D =>
      {
        D.Value = await ToDo(R);

        return (object?) null;
      })
    );
  }

  public static Thought Done()
  {
    return WithoutFeedback.Do(_ => { });
  }

  public static async Task<Thought<TProduct, TFeedback>> ThinkAsync<TProduct, TFeedback>(
    Func<Reasoning, Task<ThoughtResult<TProduct, TFeedback>>> Produce)
  {
    var Reasoning = new Reasoning();
    var ThoughtResult = await Produce(Reasoning);
    var Result = new Thought<TProduct, TFeedback>(ThoughtResult, Reasoning);
    Reasoning.Parent = Result;
    return Result;
  }

  public void RaiseAnyExceptions()
  {
    Result.RaiseAnyExceptions();
  }

  public static class WithoutFeedback
  {
    public static Task<Thought<TProduct, NullFeedback>> ThinkAsync<TProduct>(Func<Reasoning, Task<TProduct>> Produce)
    {
      return Thought.ThinkAsync(R => ThoughtResult
        .WithFeedbackSource(FeedbackSource.Null)
        .FromLogicAsync(_ => Produce(R)));
    }

    public static Thought<TProduct, NullFeedback> Think<TProduct>(Func<Reasoning, TProduct> Produce)
    {
      return Thought.Think(R => ThoughtResult
        .WithFeedbackSource(FeedbackSource.Null)
        .FromLogic(_ => Produce(R)));
    }

    public static async Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
    {
      return await Thought.DoAsync(async R =>
      {
        await ToDo(R);

        return NullFeedback.Instance;
      });
    }

    public static Thought Do(Action<Reasoning> ToDo)
    {
      return Thought.Do(R =>
      {
        ToDo(R);

        return NullFeedback.Instance;
      });
    }
  }
}

public abstract class Thought<TFeedback>(
  Thought.Reasoning LineOfReasoning,
  ThoughtResult<TFeedback> Result)
  : Thought(LineOfReasoning, Result)
{
  public TFeedback Feedback => Result.Feedback;
}

public sealed class Thought<TProduct, TFeedback> : Thought<TFeedback>
{
  readonly ThoughtResult<TProduct, TFeedback> Result;

  internal Thought(
    ThoughtResult<TProduct, TFeedback> Result,
    Reasoning LineOfReasoning)
    : base(LineOfReasoning, Result)
  {
    this.Result = Result;
  }

  public TProduct ConsumeDetached()
  {
    return new Reasoning().Consume(this);
  }

  internal TProduct GetProduct()
  {
    return Result.Product;
  }
}

public class DeferredFeedback<TFeedback>
{
  public TFeedback Value { get; set; } = default!;
}

public static class FeedbackSource
{
  public static FeedbackSource<NullFeedbackConfigurator, NullFeedback> Null { get; } =
    new(NullFeedbackConfigurator.Instance, () => NullFeedback.Instance);

  public static FeedbackSource<TFeedback, TFeedback> FromInstance<TFeedback>(TFeedback Instance)
  {
    return new(Instance, () => Instance);
  }

  public static FeedbackSource<DeferredFeedback<TFeedback>, TFeedback> Deferred<TFeedback>()
  {
    var Deferred = new DeferredFeedback<TFeedback>();

    return new(Deferred, () => Deferred.Value);
  }

  public static FeedbackSource<NullFeedbackConfigurator, TFeedback> FromFunc<TFeedback>(Func<TFeedback> CreateMethod)
  {
    return new(NullFeedbackConfigurator.Instance, CreateMethod);
  }
}

public class NullFeedbackConfigurator
{
  NullFeedbackConfigurator()
  {
  }

  public static NullFeedbackConfigurator Instance { get; } = new();
}

public record FeedbackSource<TConfigurator, TFeedback>(TConfigurator Configurator, Func<TFeedback> CreateFeedback);