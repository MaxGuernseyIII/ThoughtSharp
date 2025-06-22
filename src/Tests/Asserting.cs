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
using static FluentAssertions.FluentActions;
using Assert = ThoughtSharp.Scenarios.Assert;

namespace Tests;

public class AssertingBase<T>
{
  protected T Payload = default!;
  protected MockFeedbackSink<T> FeedbackMock = null!;
  protected Action? Action;

  [TestInitialize]
  public void BaseSetUp()
  {
    FeedbackMock = new();
  }

  protected void ThenAssertionDidNotThrowAnException()
  {
    Action.Should().NotThrow();
  }

  protected void WhenAssertThatResultIs(CognitiveResult<T, T> R, T Payload)
  {
    Action = Invoking(() => Assert.That(R).Is(Payload));
  }

  protected CognitiveResult<T, T> GivenCognitiveResult()
  {
    return CognitiveResult.From(Payload, FeedbackMock);
  }

  protected void ThenModelWasTrainedWith(IEnumerable<T> Expected)
  {
    FeedbackMock.RecordedFeedback.Should().BeEquivalentTo(Expected, O => O.WithStrictOrdering());
  }
}

[TestClass]
public class AssertingSingleObjects : AssertingBase<int>
{
  [TestInitialize]
  public void SetUp()
  {
    Payload = Any.Int();
  }

  [TestMethod]
  public void AssertOnSingleObjects()
  {
    var R = GivenCognitiveResult();

    WhenAssertThatResultIs(R, Payload);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([Payload]);
  }
}