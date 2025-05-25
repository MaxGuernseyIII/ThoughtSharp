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

public partial class Thought
{
  public class Reasoning
  {
    internal Thought? Parent { get; set; }

    readonly List<Thought> UsedSubthoughts = [];
    readonly Dictionary<Thought, float> Weights = [];
    readonly List<IDisposable> Disposables = [];

    internal IReadOnlyList<Thought> Children => UsedSubthoughts.ToArray();
    internal IReadOnlyDictionary<Thought, float> ChildrenWeights => new Dictionary<Thought, float>(Weights);
    internal IReadOnlyList<IDisposable> DisposableResources => new List<IDisposable>(Disposables);

    public T Consume<T>(Thought<T> Subthought)
    {
      Incorporate(Subthought);

      return Subthought.GetProduct();
    }

    public void Incorporate(Thought Subthought)
    {
      if (Subthought.Container is not null)
        throw new InvalidOperationException("A Thought can only be used in one line of reasoning");

      UsedSubthoughts.Add(Subthought);
      Subthought.Container = this;
      Weights[Subthought] = 1f;

      Subthought.RaiseAnyExceptions();
    }

    public void SetWeight(Thought Subthought, float Weight)
    {
      Weights[Subthought] = Weight;
    }

    public void RegisterDisposable(IDisposable Resource)
    {
      Disposables.Add(Resource);
    }
  }
}
