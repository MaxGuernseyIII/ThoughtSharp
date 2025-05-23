﻿// MIT License
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
    
    var T = Thought.Capture(Expected);

    T.ConsumeDetached().Should().BeSameAs(Expected);
    T.Children.Should().Equal();
  }

  [TestMethod]
  public void UseSubthought()
  {
    var Expected = new MockProduct();
    var T = Thought.Capture(Expected);
    var Reasoning = new Reasoning();

    var Actual = Reasoning.Consume(T);

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void UsingSubthoughtBindsToSuperthought()
  {
    var Expected = new MockProduct();
    var T = Thought.Capture(Expected);
    var Reasoning = new Reasoning();
    Reasoning.Consume(Thought.Capture(new MockProduct()));
    Reasoning.Consume(Thought.Capture(new MockProduct()));
    var OriginalChildren = Reasoning.Children.ToArray();

    Reasoning.Consume(T);

    Reasoning.Children.Should().Equal([.. OriginalChildren, T]);
  }

  [TestMethod]
  public void InternalReasoning()
  {
    var Expected = new MockProduct();
    var T = Thought.Think(_ => Expected);

    var Actual = T.ConsumeDetached();

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void ThoughtChildrenIsReasoningChildrenAtEndOfProduction()
  {
    Reasoning CapturedReasoning = null!;
    var T = Thought.Think(R =>
    {
      CapturedReasoning = R;
      R.Consume(Thought.Capture(new MockProduct()));
      R.Consume(Thought.Capture(new object()));
      R.Consume(Thought.Capture(new MockProduct()));

      return new MockProduct();
    });

    var Actual = T.Children;

    Actual.Should().Equal(CapturedReasoning.Children);
  }

  [TestMethod]
  public void ThoughtChildrenAreAttachedToParentThought()
  {
    var Subthought = Thought.Capture(new MockProduct());
    var Superthought = Thought.Do(R =>
    {
      R.Consume(Subthought);
    });

    Subthought.Parent.Should().BeSameAs(Superthought);
  }

  [TestMethod]
  public void UsedThoughtParentIsCorrectAtEndOfReasoning()
  {
    var Subthought = Thought.Capture(new MockProduct());
    var T = Thought.Think(R =>
    {
      R.Consume(Subthought);

      return new MockProduct();
    });

    var Actual = Subthought.Parent;

    Actual.Should().BeSameAs(T);
  }

  [TestMethod]
  public void SubthoughtCannotBeUsedTwice()
  {
    var Subthought = Thought.Capture(new MockProduct());
    Thought.Think(R =>
    {
      R.Consume(Subthought);
      return new object();
    });
    var SubthoughtParent = Subthought.Parent;

    var FailedThought = Thought.Think(R =>
    {
      R
        .Invoking(Reasoning => Reasoning.Consume(Subthought))
        .Should()
        .Throw<InvalidOperationException>();

      return new object();
    });

    FailedThought.Children.Should().Equal();
    Subthought.Parent.Should().BeSameAs(SubthoughtParent);
  }

  [TestMethod]
  public async Task AsynchronousThinkingYieldsProduct()
  {
    var Expected = new MockProduct();
    var T = await Thought.ThinkAsync(_ => Task.FromResult(Expected));

    var Actual = T.ConsumeDetached();

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public async Task AsynchronousThinkingCapturesParentThought()
  {
    var Subthought = Thought.Capture(new MockProduct());

    var T = await Thought.DoAsync(R =>
    {
      R.Consume(Subthought);
      return Task.CompletedTask;
    });

    Subthought.Parent.Should().BeSameAs(T);
  }

  [TestMethod]
  public async Task AsynchronousThinkingCapturesChildThought()
  {
    var Subthought = Thought.Capture(new MockProduct());

    var T = await Thought.DoAsync(R =>
    {
      R.Consume(Subthought);
      return Task.CompletedTask;
    });

    T.Children.Should().Contain(Subthought);
  }
}