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
public class DynamicWeightedRunning
{
  const int ConvergenceTrackerLength = 10;
  int RunCount;
  RunResult RunResult = null!;
  MockRunnable Underlying = null!;
  ConvergenceTracker Tracker = null!;
  Counter TrialCounter = null!;
  MockSummarizer Summarizer = null!;
  RunResult Result = null!;

  [TestInitialize]
  public void SetUp()
  {
    RunCount = 0;
    RunResult = new() { Status = BehaviorRunStatus.Success, Transcript = new([new() { Score = 1, Annotations = [] }])};
    Underlying = new()
    {
      RunBehavior = () =>
      {
        RunCount++;
        return Task.FromResult(RunResult);
      }
    };
    Summarizer = new();
    Tracker = new(ConvergenceTrackerLength, Summarizer);
    TrialCounter = new();
    Result = null!;
  }

  [TestMethod]
  public async Task WithFullWeightIsPassThrough()
  {
    var Runner = GivenWeightedRunnable(1, 1, .5);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task SupportsConvergenceThresholdOfOneHundredPercent()
  {
    GivenConvergence(0f);
    var Runner = GivenWeightedRunnable(1, 1, 1);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task WithPartialWeightAndNoConvergenceDoesNotRunRightAway()
  {
    GivenConvergence(0f);
    var Runner = GivenWeightedRunnable(.25, .5, .5);

    await WhenRun(Runner);

    ThenResultIsNotRun();
    ThenRunCountIs(0);
  }

  [TestMethod]
  public async Task WithPartialWeightAndNoConvergenceTakesMultipleTries()
  {
    GivenConvergence(0f);
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    await GivenPreviousRuns(Runner, 1);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task WithPartialWeightAndFullConvergenceDoesNotRunRightAway()
  {
    GivenConvergence(1f);
    var Runner = GivenWeightedRunnable(.25, .5, .5);

    await WhenRun(Runner);

    ThenResultIsNotRun();
    ThenRunCountIs(0);
  }

  [TestMethod]
  public async Task WithPartialWeightAndFullConvergenceTakesMultipleTries()
  {
    GivenConvergence(1f);
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    await GivenPreviousRuns(Runner, 3);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task WithPartialWeightsAndPartialConvergenceTakesMultipleTries()
  {
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    GivenConvergence(.8f);
    await GivenPreviousRuns(Runner, 2);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task KeepsCounterUpToDate()
  {
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    GivenConvergence(Any.Float);
    await GivenPreviousRuns(Runner, Any.Int(0, 10));

    await WhenRun(Runner);

    ThenCounterHasSameValueAsNumberOfRuns();
  }

  void GivenConvergence(float Convergence)
  {
    IReadOnlyList<float> History = Any.FloatArray(Any.Int(1, 5));
    Tracker.ApplyHistory(History);
    Summarizer.SetUpResponse([..History, ..Enumerable.Repeat(0f, ConvergenceTrackerLength - History.Count)], Convergence);
  }

  static async Task GivenPreviousRuns(DynamicWeightedRunnable Runner, int Count)
  {
    foreach (var _ in Enumerable.Range(0, Count))
      await Runner.Run();
  }

  DynamicWeightedRunnable GivenWeightedRunnable(double MinimumWeight, double MaximumWeight, double ConvergenceThreshold)
  {
    return new(Underlying, MinimumWeight, MaximumWeight, Tracker, ConvergenceThreshold, TrialCounter);
  }

  async Task WhenRun(DynamicWeightedRunnable Runner)
  {
    Result = await Runner.Run();
  }

  void ThenResultIsWhatWasReturnedByUnderlying()
  {
    Result.Should().BeSameAs(RunResult);
  }

  void ThenResultIsNotRun()
  {
    Result.Should().Be(new RunResult { Status = BehaviorRunStatus.NotRun, Transcript = new([])});
  }

  void ThenRunCountIs(int Expected)
  {
    RunCount.Should().Be(Expected);
  }

  void ThenCounterHasSameValueAsNumberOfRuns()
  {
    TrialCounter.Value.Should().Be(RunCount);
  }
}