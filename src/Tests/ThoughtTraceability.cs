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
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public sealed class ThoughtTraceability
{
  [TestMethod]
  public void ForProduct()
  {
    var Expected = new MockProduct();

    var T = Thought.Capture(Expected, NullFeedback.Instance);

    T.ConsumeDetached().Should().BeSameAs(Expected);
    T.Children.Should().Equal();
  }

  [TestMethod]
  public void UseSubthought()
  {
    var Expected = new MockProduct();
    var T = Thought.Capture(Expected);
    var Reasoning = new Thought.Reasoning();

    var Actual = Reasoning.Consume(T);

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void UsingSubthoughtBindsToSuperthought()
  {
    var Expected = new MockProduct();
    var T = Thought.Capture(Expected);
    var Reasoning = new Thought.Reasoning();
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
    var T = Thought.Think(_ => ThoughtResult.FromOutput(Expected, NullFeedback.Instance));

    var Actual = T.ConsumeDetached();

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void FeedbackCanBeSetEarlyAndIsStillAccessibleAfterException()
  {
    var Expected = new object();
    var T = Thought.Think(_ =>
    {
      return ThoughtResult.FromFeedback(Expected)
        .FromLogic(() =>
          {
            Assert.Fail("This exception should not be raised.");

            return new MockProduct();
          }
        );
    });

    T.Feedback.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public void ThoughtsCaptureAndReleaseExceptions()
  {
    var Expected = new Exception();
    var T = Thought.WithoutFeedback.Think<object?>(_ => throw Expected);

    T.Invoking(It => new Thought.Reasoning().Incorporate(It)).Should().Throw<Exception>().And.Should().Be(Expected);
  }

  [TestMethod]
  public async Task AsynchronousThoughtsCaptureAndReleaseExceptions()
  {
    var Expected = new Exception();
    var T = await Thought.WithoutFeedback.DoAsync(_ => Task.FromException(Expected));

    T.Invoking(It => new Thought.Reasoning().Incorporate(It)).Should().Throw<Exception>().And.Should().Be(Expected);
  }

  [TestMethod]
  public void SynchronousCleanup()
  {
    var Disposable = new MockDisposable();
    var T = Thought.WithoutFeedback.Do(R => { R.RegisterDisposable(Disposable); });

    T.Dispose();

    Disposable.Disposed.Should().Be(true);
  }

  [TestMethod]
  public void CleanupIsDistributive()
  {
    var Disposable = new MockDisposable();
    var T = Thought.WithoutFeedback.Do(R => { R.Incorporate(Thought.WithoutFeedback.Do(R2 => R2.RegisterDisposable(Disposable))); });

    T.Dispose();

    Disposable.Disposed.Should().Be(true);
  }

  [TestMethod]
  public async Task AsynchronousCleanup()
  {
    var Disposable = new MockDisposable();
    var T = await Thought.WithoutFeedback.DoAsync(R =>
    {
      R.RegisterDisposable(Disposable);
      return Task.CompletedTask;
    });

    T.Dispose();

    Disposable.Disposed.Should().Be(true);
  }

  [TestMethod]
  public void ThoughtChildrenIsReasoningChildrenAtEndOfProduction()
  {
    Thought.Reasoning CapturedReasoning = null!;
    var T = Thought.WithoutFeedback.Think(R =>
    {
      CapturedReasoning = R;
      R.Consume(Thought.Capture(new MockProduct()));
      R.Consume(Thought.Capture(new object()));
      R.Consume(Thought.Capture(new MockProduct()));

      return (new MockProduct(), NullFeedback.Instance);
    });

    var Actual = T.Children;

    Actual.Should().Equal(CapturedReasoning.Children);
  }

  [TestMethod]
  public void ThoughtChildrenAreAttachedToParentThought()
  {
    var Subthought = Thought.Capture(new MockProduct());
    var Superthought = Thought.WithoutFeedback.Do(R => { R.Consume(Subthought); });

    Subthought.Parent.Should().BeSameAs(Superthought);
  }

  [TestMethod]
  public void UsedThoughtParentIsCorrectAtEndOfReasoning()
  {
    var Subthought = Thought.Capture(new MockProduct());
    var T = Thought.WithoutFeedback.Think(R =>
    {
      R.Consume(Subthought);

      return (new MockProduct(), NullFeedback.Instance);
    });

    var Actual = Subthought.Parent;

    Actual.Should().BeSameAs(T);
  }

  [TestMethod]
  public void SubthoughtCannotBeUsedTwice()
  {
    var Subthought = Thought.Capture(new MockProduct());
    Thought.WithoutFeedback.Think(R =>
    {
      R.Consume(Subthought);
      return (new object(), NullFeedback.Instance);
    });
    var SubthoughtParent = Subthought.Parent;

    var FailedThought = Thought.WithoutFeedback.Think(R =>
    {
      R
        .Invoking(Reasoning => Reasoning.Consume(Subthought))
        .Should()
        .Throw<InvalidOperationException>();

      return (new object(), NullFeedback.Instance);
    });

    FailedThought.Children.Should().Equal();
    Subthought.Parent.Should().BeSameAs(SubthoughtParent);
  }

  [TestMethod]
  public async Task AsynchronousThinkingYieldsProduct()
  {
    var Expected = new MockProduct();
    var T = await Thought.WithoutFeedback.ThinkAsync(_ => Task.FromResult(Expected));

    var Actual = T.ConsumeDetached();

    Actual.Should().BeSameAs(Expected);
  }

  [TestMethod]
  public async Task AsynchronousThinkingCapturesParentThought()
  {
    var Subthought = Thought.Capture(new MockProduct());

    var T = await Thought.WithoutFeedback.DoAsync(R =>
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

    var T = await Thought.WithoutFeedback.DoAsync(R =>
    {
      R.Consume(Subthought);
      return Task.CompletedTask;
    });

    T.Children.Should().Contain(Subthought);
  }
}