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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class Counting
{
  [TestMethod]
  public void InitialValueIsZero()
  {
    var Counter = new Counter();

    Counter.Value.Should().Be(0);
  }

  [TestMethod]
  public void ResetToOriginalValue()
  {
    var Counter = GivenCounterInSomeState();

    Counter.Reset();

    Counter.Value.Should().Be(new Counter().Value);
  }

  [TestMethod]
  public void IncrementUpdatesValue()
  {
    var Counter = GivenCounterInSomeState();
    var Previous = Counter.Value;

    Counter.Increment();

    Counter.Value.Should().Be(Previous + 1);
  }

  [TestMethod]
  public void SetCounterValue()
  {
    var Value = Any.Int(1000, 2000);

    var Counter = new Counter(Value);

    Counter.Value.Should().Be(Value);
  }

  [TestMethod]
  public void CompoundIncrementer()
  {
    var Counter1 = new MockIncrementable() { Count = Any.Int(100, 200) };
    var Counter2 = new MockIncrementable() { Count = Any.Int(100, 200) };
    var Compound = new CompoundIncrementable(Counter1, Counter2);
    var Original1 = Counter1.Count;
    var Original2 = Counter2.Count;

    Compound.Increment();

    Counter1.Count.Should().Be(Original1 + 1);
    Counter2.Count.Should().Be(Original2 + 1);
  }

  static Counter GivenCounterInSomeState()
  {
    return new(Any.Int(0, 20));
  }
}