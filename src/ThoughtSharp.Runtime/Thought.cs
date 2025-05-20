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

public abstract class Thought
{
  readonly Action<float>? Reward;

  // prevents foreign inheritors
  internal Thought(Action<float>? Reward)
  {
    this.Reward = Reward;
  }

  internal Reasoning? Container { get; set; }

  public Thought? Parent => Container?.Parent;
  public IReadOnlyList<Thought> Children { get; protected set; } = [];

  public static Thought<T> Capture<T>(T Product, Action<float>? Reward = null)
  {
    return new(Product, [], Reward);
  }

  public static Thought<T> Think<T>(Func<Reasoning, T> Produce)
  {
    var Reasoning = new Reasoning();
    var Product = Produce(Reasoning);
    var Result = new Thought<T>(Product, Reasoning.Children, null);
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

  public static async Task<Thought<T>> ThinkAsync<T>(Func<Reasoning, Task<T>> Produce)
  {
    var Reasoning = new Reasoning();
    var Product = await Produce(Reasoning);
    var Result = new Thought<T>(Product, Reasoning.Children, null);
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

  public void ApplyReward(float RewardToApply)
  {
    Reward!(RewardToApply);
  }
}

public sealed class Thought<T> : Thought
{
  internal readonly T Product;

  internal Thought(T Product, IReadOnlyList<Thought> Children, Action<float>? Reward) 
    : base(Reward)
  {
    this.Product = Product;
    this.Children = Children;
  }

  public T ConsumeDetached()
  {
    return new Reasoning().Consume(this);
  }
}