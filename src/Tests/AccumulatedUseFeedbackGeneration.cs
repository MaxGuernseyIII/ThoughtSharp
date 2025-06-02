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

using System.Collections.Immutable;
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public class AccumulatedUseFeedbackGeneration
{
  Queue<UseFeedbackSink<MockSurface>> Steps = null!;
  MockMind Mind = null!;
  MockMind ChainedMind = null!;
  List<MockMind> UsedMinds = null!;

  [TestInitialize]
  public void SetUp()
  {
    ChainedMind = new();
    Mind = new()
    {
      GetNextChainedReasoningMind = () =>
      {
        Mind.GetNextChainedReasoningMind = () => new();
        return ChainedMind;
      }
    };
    Steps = new();
    UsedMinds = [];
  }

  [TestMethod]
  public void UsesChainedMind()
  {
    GivenStep();

    WhenUse();

    UsedMinds.Should().AllSatisfy(M => M.Should().BeSameAs(ChainedMind));
  }

  [TestMethod]
  public void SingleOperation()
  {
    var Step = GivenStep();

    var T = WhenUse();

    ThenStepsShouldBe(T, Step);
  }

  [TestMethod]
  public void MultipleOperations()
  {
    var Step1 = GivenStep();
    var Step2 = GivenStep();

    var T = WhenUse();

    ThenStepsShouldBe(T, Step1, Step2);
  }
  
  [TestMethod]
  public void MaxNumberOfOperations()
  {
    var MaximumNumber = Any.Int(3, 6);
    var Expected = GivenSteps(MaximumNumber);

    var T = WhenUse(new() { MaxTrials = MaximumNumber });

    ThenStepsShouldBe(T, [.. Expected]);
  }

  [TestMethod]
  public void ExceedMaxNumberOfOperations()
  {
    var MaximumNumber = Any.Int(3, 6);
    var Expected = GivenSteps(MaximumNumber);
    GivenStep();

    var T = WhenUse(new() { MaxTrials = MaximumNumber });

    ThenStepsShouldBe(T, [.. Expected]);
  }

  List<UseFeedbackSink<MockSurface>> GivenSteps(int Count)
  {
    return Enumerable.Range(0, Count).Select(_ => GivenStep()).ToList();
  }

  FeedbackSink<IEnumerable<Action<MockSurface>>> WhenUse(MindExtensions.UseConfiguration? UseConfiguration = null)
  {
    return Mind.Use(M =>
    {
      UsedMinds.Add(M);
      var ThisStep = Steps.Dequeue();
      return CognitiveResult.From(Steps.Any(), ThisStep);
    }, UseConfiguration);
  }

  static void ThenStepsShouldBe(FeedbackSink<IEnumerable<Action<MockSurface>>> T,
    params ImmutableArray<UseFeedbackSink<MockSurface>> Expected)
  {
    T.Should().Be(new AccumulatedUseFeedback<MockSurface>([..Expected]));
  }

  UseFeedbackSink<MockSurface> GivenStep()
  {
    var Step = new UseFeedbackSink<MockSurface>(null!,
      delegate { Assert.Fail("This object is only used for its identity."); });
    Steps.Enqueue(Step);
    return Step;
  }

  // ReSharper disable once ClassNeverInstantiated.Local
  class MockSurface;
}