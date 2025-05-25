// MIT License
// 
// Copyright (c) 2024-2024 Hexagon Software LLC
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

public abstract partial class Thought
{
  readonly IReadOnlyDictionary<Thought, float> Weights;

  // prevents foreign inheritors
  internal Thought(Reasoning LineOfReasoning, TrainingPolicy? TrainingPolicy)
  {
    this.TrainingPolicy = TrainingPolicy;
    Children = LineOfReasoning.Children;
    Weights = LineOfReasoning.ChildrenWeights;
  }

  internal TrainingPolicy? TrainingPolicy { get; set; }

  internal Reasoning? Container { get; set; }

  public Thought? Parent => Container?.Parent;
  public IReadOnlyList<Thought> Children { get; }

  public static Thought Done(TrainingPolicy? Training = null)
  {
    return new Thought<object?>(null, new(), Training);
  }

  public static Thought<T> Capture<T>(T Product, TrainingPolicy? Training = null)
  {
    return new(Product, new(), Training);
  }

  public static Thought<T> Think<T>(Func<Reasoning, T> Produce, TrainingPolicy? Policy = null)
  {
    var Reasoning = new Reasoning();
    var Product = Produce(Reasoning);
    var Result = new Thought<T>(Product, Reasoning, Policy);
    Reasoning.Parent = Result;
    return Result;
  }

  public static Thought Do(Action<Reasoning> ToDo)
  {
    return Think(object? (R) =>
    {
      ToDo(R);
      return null;
    });
  }

  public static async Task<Thought<T>> ThinkAsync<T>(Func<Reasoning, Task<T>> Produce, TrainingPolicy? Policy = null)
  {
    var Reasoning = new Reasoning();
    var Product = await Produce(Reasoning);
    var Result = new Thought<T>(Product, Reasoning, Policy);
    Reasoning.Parent = Result;
    return Result;
  }

  public static async Task<Thought> DoAsync(Func<Reasoning, Task> ToDo)
  {
    var Result = await ThinkAsync(async Task<object?> (R) =>
    {
      await ToDo(R);

      return null;
    });

    return Result;
  }

  // TODO: this needs to be rebuilt to...
  //  1. [x] get a graph of thoughts bucketed into time-ordered lists by Mind
  //  2. apply reward to both state and output for all be last thought for a Mind
  //  3. apply reward to output only for the last thought for a Mind
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
}

public sealed class Thought<T> : Thought
{
  internal readonly T Product;

  internal Thought(T Product, Reasoning LineOfReasoning, TrainingPolicy? TrainingPolicy)
    : base(LineOfReasoning, TrainingPolicy)
  {
    this.Product = Product;
  }

  public T ConsumeDetached()
  {
    return new Reasoning().Consume(this);
  }
}