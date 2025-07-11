﻿// MIT License
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
using ThoughtSharp.Scenarios;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class AutomationPasses
{
  MockReporter Reporter = null!;
  MockGate SaveGate = null!;
  MockSaver Saver = null!;
  TrainingDataScheme Scheme = null!;
  TrainingMetadata Metadata = null!;
  ImmutableArray<(ScenariosModelNode Node, MockRunnable Runnable)> Steps = [];

  [TestInitialize]
  public void SetUp()
  {
    Steps = [..Any.MockRunnables().Select(R => (new MockNode(), R))];
    SaveGate = new(Any.Bool);
    Saver = new();
    Reporter = new();
    Metadata = Any.TrainingMetadata();
    Scheme = new(new MockNode(), Metadata);
  }

  [TestMethod]
  public async Task CallsEachRunnableExactlyOnce()
  {
    var Pass = GivenAutomationPass();

    await Pass.Run();

    ThenEachStepWasRun();
  }

  [TestMethod]
  public async Task ReportsResultsOfRunnables()
  {
    var Pass = GivenAutomationPass();
    var (Node, Runnable) = Any.Of(Steps);
    var Result = GivenRunnableWillReturnResult(Runnable);

    await Pass.Run();

    ThenResultWasReported(Node, Result);
  }

  [TestMethod]
  public async Task SavesIfAppropriate()
  {
    var Pass = GivenAutomationPass();
    GivenSaveGateStateIs(true);

    await Pass.Run();

    ThenWasSaved();
  }

  [TestMethod]
  public async Task DoesNotSaveWhenItShouldNot()
  {
    var Pass = GivenAutomationPass();
    GivenSaveGateStateIs(false);

    await Pass.Run();

    ThenWasNotSaved();
  }

  [TestMethod]
  public async Task ReportsFailureToConvergenceTracker()
  {
    var (Node, Runnable) = Any.Of(Steps);
    var Pass = GivenAutomationPass();
    GivenConvergenceOf1For(Node);
    GivenRunnableWillReturnResult(Runnable, new()
    {
      Status = BehaviorRunStatus.Failure,
      Transcript = new([Any.GradeOfAtMost(0.99f)]),
      Exception = null
    });

    await Pass.Run();

    ThenConvergenceIsLessThan1For(Node);
  }

  [TestMethod]
  public async Task ReportsSuccessToConvergenceTracker()
  {
    var (Node, Runnable) = Any.Of(Steps);
    var Pass = GivenAutomationPass();
    GivenConvergenceOf0For(Node);
    GivenRunnableWillReturnResult(Runnable, new()
    {
      Status = BehaviorRunStatus.Success,
      Transcript = new([Any.GradeOfAtLeast(0.01f)]),
      Exception = null
    });

    await Pass.Run();

    ThenConvergenceIsGreaterThan0For(Node);
  }

  [TestMethod]
  public async Task CumulativeResultIsPassWhenNothingFails()
  {
    var Pass = GivenAutomationPass();
    foreach (var Step in Steps)
      Step.Runnable.Result = new()
      {
        Status = BehaviorRunStatus.Success,
        Transcript= new([])
      };

    var Result = await Pass.Run();

    Result.Should().Be(new RunResult() { Status = BehaviorRunStatus.Success, Transcript = new([])});
  }

  [TestMethod]
  public async Task CumulativeResultIsFailureWhenAnythingFails()
  {
    var Pass = GivenAutomationPass();
    foreach (var Step in Steps)
      Step.Runnable.Result = new() { Status = BehaviorRunStatus.Success, Transcript = new([])};
    Any.Of(Steps).Runnable.Result = new() {Status = BehaviorRunStatus.Failure, Transcript = new([])};

    var Result = await Pass.Run();

    Result.Should().Be(new RunResult() { Status = BehaviorRunStatus.Failure, Transcript = new([])});
  }

  [TestMethod]
  public async Task CumulativeTranscriptIsJoinedResults()
  {
    var Pass = GivenAutomationPass();
    var Transcripts = new List<Transcript>();
    foreach (var Step in Steps)
    {
      var Transcript = new Transcript([..Any.ListOf(() => Any.Grade, 1, 3)]);
      Transcripts.Add(Transcript);
      Step.Runnable.Result = new() { Status = BehaviorRunStatus.Success, Transcript = Transcript};
    }

    var Result = await Pass.Run();

    Result.Should().Be(new RunResult() { Status = BehaviorRunStatus.Success, Transcript = Transcript.Join(Transcripts)});
  }

  void ThenConvergenceIsLessThan1For(ScenariosModelNode Node)
  {
    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    Tracker.MeasureConvergence().Should().BeLessThan(1);
  }

  void GivenConvergenceOf1For(ScenariosModelNode Node)
  {
    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    foreach (var _ in Enumerable.Range(0, Metadata.SampleSize))
      Tracker.RecordResult(1f);
  }

  void ThenConvergenceIsGreaterThan0For(ScenariosModelNode Node)
  {
    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    Tracker.MeasureConvergence().Should().BeGreaterThan(0);
  }

  void GivenConvergenceOf0For(ScenariosModelNode Node)
  {
    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    foreach (var _ in Enumerable.Range(0, Metadata.SampleSize))
      Tracker.RecordResult(0f);
  }

  void GivenSaveGateStateIs(bool State)
  {
    SaveGate.Answers.Clear();
    SaveGate.Answers.Enqueue(State);
  }

  void ThenWasSaved()
  {
    Saver.SaveCount.Should().Be(1);
  }

  void ThenWasNotSaved()
  {
    Saver.SaveCount.Should().Be(0);
  }

  static RunResult GivenRunnableWillReturnResult(MockRunnable Runnable)
  {
    var Result = new RunResult
    {
      Status = Any.EnumValue<BehaviorRunStatus>(),
      Transcript = new([Any.Grade]),
      Exception = new()
    };
    GivenRunnableWillReturnResult(Runnable, Result);
    return Result;
  }

  static void GivenRunnableWillReturnResult(MockRunnable Runnable, RunResult Result)
  {
    Runnable.Result = Result;
  }

  void ThenResultWasReported(ScenariosModelNode Node, RunResult Result)
  {
    (Reporter.Results.GetValueOrDefault(Node)?[0]).Should().BeSameAs(Result);
  }

  void ThenEachStepWasRun()
  {
    Steps.Should().AllSatisfy(S => S.Runnable.RunCount.Should().Be(1));
  }

  Runnable GivenAutomationPass()
  {
    return new AutomationPass([..Steps], SaveGate, Saver, Scheme, Reporter);
  }
}