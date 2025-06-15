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
  RunResult Result = null!;

  [TestInitialize]
  public void SetUp()
  {
    RunCount = 0;
    RunResult = new() { Status = BehaviorRunStatus.Success };
    Underlying = new()
    {
      RunBehavior = () =>
      {
        RunCount++;
        return Task.FromResult(RunResult);
      }
    };
    Tracker = new(ConvergenceTrackerLength);
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
  public async Task WithPartialWeightAndNoConvergenceDoesNotRunRightAway()
  {
    var Runner = GivenWeightedRunnable(.25, .5, .5);

    await WhenRun(Runner);

    ThenResultIsNotRun();
    ThenRunCountIs(0);
  }

  [TestMethod]
  public async Task WithPartialWeightAndNoConvergenceTakesMultipleTries()
  {
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    await GivenPreviousRuns(Runner, 1);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task WithPartialWeightAndFullConvergenceDoesNotRunRightAway()
  {
    GivenSuccesses(ConvergenceTrackerLength);
    var Runner = GivenWeightedRunnable(.25, .5, .5);

    await WhenRun(Runner);

    ThenResultIsNotRun();
    ThenRunCountIs(0);
  }

  [TestMethod]
  public async Task WithPartialWeightAndFullConvergenceTakesMultipleTries()
  {
    GivenSuccesses(ConvergenceTrackerLength);
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
    GivenSuccesses((int) (ConvergenceTrackerLength * .5 + ConvergenceTrackerLength * .5 * .6));
    await GivenPreviousRuns(Runner, 2);

    await WhenRun(Runner);

    ThenResultIsWhatWasReturnedByUnderlying();
    ThenRunCountIs(1);
  }

  [TestMethod]
  public async Task KeepsCounterUpToDate()
  {
    var Runner = GivenWeightedRunnable(.25, .5, .5);
    GivenSuccesses(Any.Int(0, ConvergenceTrackerLength));
    await GivenPreviousRuns(Runner, Any.Int(0, 10));

    await WhenRun(Runner);

    ThenCounterHasSameValueAsNumberOfRuns();
  }

  void GivenSuccesses(int I)
  {
    foreach (var _ in Enumerable.Range(0, I)) 
      Tracker.RecordResult(true);
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
    Result.Should().Be(new RunResult { Status = BehaviorRunStatus.NotRun});
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