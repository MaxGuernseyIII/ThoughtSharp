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

public class Thought
{
  internal Reasoning? Container { get; set; }

  public Thought? Parent => Container?.Parent;
  public IReadOnlyList<Thought> Children { get; protected set; } = [];

  public static Thought<T> Capture<T>(T Product)
  {
    return new(Product, []);
  }

  public static Thought<T> Think<T>(Func<Reasoning, T> Produce)
  {
    var Reasoning = new Reasoning();
    var Product = Produce(Reasoning);
    var Result = new Thought<T>(Product, Reasoning.Children);
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

  public static async Task<Thought<T>> ThinkAsync<T>(Func<Reasoning, Task<T>> Func)
  {
    throw new NotImplementedException();
  }
}

public class Thought<T> : Thought
{
  internal readonly T Product;

  internal Thought(Func<Reasoning, T> Produce)
  {
    var Reasoning = new Reasoning();
    Product = Produce(Reasoning);
    Children = Reasoning.Children;
  }

  internal Thought(T Product, IReadOnlyList<Thought> Children)
  {
    this.Product = Product;
    this.Children = Children;
  }

  public T Consume()
  {
    return new Reasoning().Use(this);
  }
}