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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public sealed class ThoughtTraceability
{
  [TestMethod]
  public void ForProduct()
  {
    var Expected = new MockProduct();
    
    var T = Thought.ForProduct(Expected);

    T.Consume().Should().BeSameAs(Expected);
    T.Children.Should().Equal();
  }

  [TestMethod]
  public void UseSubthought()
  {
    var Expected = new MockProduct();
    var T = Thought.ForProduct(Expected);
    var Superthought = new Thought();
    var Reasoning = new Reasoning(Superthought);

    var Actual = Reasoning.Use(T);

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void UsingSubthoughtBindsToSuperthought()
  {
    var Expected = new MockProduct();
    var T = Thought.ForProduct(Expected);
    var Superthought = new Thought();
    var Reasoning = new Reasoning(Superthought);
    Reasoning.Use(Thought.ForProduct(new MockProduct()));
    Reasoning.Use(Thought.ForProduct(new MockProduct()));
    var OriginalChildren = Reasoning.Children.ToArray();

    Reasoning.Use(T);

    T.Parent.Should().BeSameAs(Superthought);
    Reasoning.Children.Should().Equal([.. OriginalChildren, T]);
  }

  [TestMethod]
  public void InternalReasoning()
  {
    var Expected = new MockProduct();
    var T = Thought.ForReasoning(R => Expected);

    var Actual = T.Consume();

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void ThoughtChildrenIsReasoningChildrenAtEndOfProduction()
  {
    Reasoning CapturedReasoning = null!;
    var T = Thought.ForReasoning(R =>
    {
      CapturedReasoning = R;
      R.Use(Thought.ForProduct(new MockProduct()));
      R.Use(Thought.ForProduct(new object()));
      R.Use(Thought.ForProduct(new MockProduct()));

      return new MockProduct();
    });

    var Actual = T.Children;

    Actual.Should().Equal(CapturedReasoning.Children);
  }

  [TestMethod]
  public void UsedThoughtParentIsCorrectAtEndOfReasoning()
  {
    var Subthought = Thought.ForProduct(new MockProduct());
    var T = Thought.ForReasoning(R =>
    {
      R.Use(Subthought);

      return new MockProduct();
    });

    var Actual = Subthought.Parent;

    Actual.Should().BeSameAs(T);
  }

  [TestMethod]
  public void SubthoughtCannotBeUsedTwice()
  {
    var Subthought = Thought.ForProduct(new MockProduct());
    Thought.ForReasoning(R =>
    {
      R.Use(Subthought);
      return new object();
    });
    var SubthoughtParent = Subthought.Parent;

    var FailedThought = Thought.ForReasoning(R =>
    {
      R
        .Invoking(Reasoning => Reasoning.Use(Subthought))
        .Should()
        .Throw<InvalidOperationException>();

      return new object();
    });

    FailedThought.Children.Should().Equal();
    Subthought.Parent.Should().BeSameAs(SubthoughtParent);
  }
}