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
using ThoughtSharp.Runtime;
using static ThoughtSharp.Runtime.AccumulatedUseFeedback;

namespace Tests;

[TestClass]
public class AccumulatedUseFeedbackTraining
{
  FeedbackSource<AccumulatedUseFeedbackConfigurator<MockToUse>, AccumulatedUseFeedback<MockToUse>> Source;

  public class MockToUse
  {
    public int Operation1CallCount;

    public void Operation1()
    {
      Operation1CallCount++;
    }

    public int Operation2CallCount;
    public int Operation2SomeArgument;

    public void Operation2(int SomeArgument)
    {
      Operation2CallCount++;
      Operation2SomeArgument = SomeArgument;
    }
  }

  [TestInitialize]
  public void SetUp()
  {
    Source = GetSource<MockToUse>();
  }

  [TestMethod]
  public void HandlesTheDoNothingCase()
  {
    var Step = GivenStep();

    WhenApplyFeedbackSteps();

    ThenFeedbackWasNoCalls(Step);
  }

  [TestMethod]
  public void HandlesOneCallWithoutParameters()
  {
    var Step = GivenStep();

    WhenApplyFeedbackSteps(M => M.Operation1());

    ThenStepWasOperation1Call(Step);
    ThenStepDoesNotRequireMoreCalls(Step);
  }

  [TestMethod]
  public void HandlesOneCallWithParameters()
  {
    var Step = GivenStep();
    var Parameter = Any.Int(-1000, 1000);

    WhenApplyFeedbackSteps(M => M.Operation2(Parameter));

    ThenStepWasOperation2Call(Step, Parameter);
    ThenStepDoesNotRequireMoreCalls(Step);
  }

  [TestMethod]
  public void HandlesMultipleCalls()
  {
    var Step1 = GivenStep();
    var Step2 = GivenStep();
    var Parameter = Any.Int(-1000, 1000);

    WhenApplyFeedbackSteps(
      M => M.Operation1(),
      M => M.Operation2(Parameter));

    ThenStepWasOperation1Call(Step1);
    ThenStepDoesRequireMoreCalls(Step1);
    ThenStepWasOperation2Call(Step2, Parameter);
    ThenStepDoesNotRequireMoreCalls(Step2);
  }

  (BoxedBool RequiresMore, MockToUse ActionSurfaceMock) GivenStep()
  {
    var RequiresMore = new BoxedBool { Value = Any.Bool };
    var ActionSurfaceMock = new MockToUse();
    var OperationFeedback = new UseFeedback<MockToUse>(ActionSurfaceMock, B => RequiresMore.Value = B);
    Source.Configurator.AddStep(OperationFeedback);
    return (RequiresMore, ActionSurfaceMock);
  }

  void WhenApplyFeedbackSteps(params IEnumerable<Action<MockToUse>> IEnumerable)
  {
    Source.CreateFeedback().UseShouldHaveBeen(IEnumerable);
  }

  static void ThenFeedbackWasNoCalls((BoxedBool RequiresMore, MockToUse ActionSurfaceMock) Step)
  {
    Step.ActionSurfaceMock.Operation1CallCount.Should().Be(0);
    Step.ActionSurfaceMock.Operation2CallCount.Should().Be(0);
    Step.RequiresMore.Value.Should().Be(false);
  }

  static void ThenStepWasOperation1Call((BoxedBool RequiresMore, MockToUse ActionSurfaceMock) Step)
  {
    Step.ActionSurfaceMock.Operation1CallCount.Should().Be(1);
    Step.ActionSurfaceMock.Operation2CallCount.Should().Be(0);
  }

  static void ThenStepWasOperation2Call((BoxedBool RequiresMore, MockToUse ActionSurfaceMock) Step, int Parameter)
  {
    Step.ActionSurfaceMock.Operation1CallCount.Should().Be(0);
    Step.ActionSurfaceMock.Operation2CallCount.Should().Be(1);
    Step.ActionSurfaceMock.Operation2SomeArgument.Should().Be(Parameter);
  }

  static void ThenStepDoesNotRequireMoreCalls((BoxedBool RequiresMore, MockToUse ActionSurfaceMock) Step)
  {
    Step.RequiresMore.Value.Should().Be(false);
  }

  static void ThenStepDoesRequireMoreCalls((BoxedBool RequiresMore, MockToUse ActionSurfaceMock) Step)
  {
    Step.RequiresMore.Value.Should().Be(true);
  }
}