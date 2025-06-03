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

namespace Tests;

[TestClass]
public class ConvergenceTracking
{
  double Convergence;
  int Length;
  ConvergenceTracker Tracker = null!;

  [TestInitialize]
  public void SetUp()
  {
    Length = Any.Int(10, 100);
    Tracker = new(Length);
  }

  [TestMethod]
  public void StartsOffAtZero()
  {
    WhenMeasureConvergence();

    ThenConvergenceIs(0);
  }

  [TestMethod]
  public void ComputesBasedOnTotalSuccessesWhenNotFull()
  {
    var Amount = Any.Int(1, Length - 1);
    GivenTrackRecord((Amount, true));

    WhenMeasureConvergence();

    ThenConvergenceIs(1d * Amount / Length);
  }

  [TestMethod]
  public void DoesNotIncludeFailuresAsConvergence()
  {
    var Amount = Any.Int(1, Length - 1);
    GivenTrackRecord((Amount, true), (1, false));

    WhenMeasureConvergence();

    ThenConvergenceIs(1d * Amount / Length);
  }

  [TestMethod]
  public void OldResultsFallOff()
  {
    var OldHistory = AnyTrackRecord(1);
    var RecentHistory = AnyTrackRecord(Length);
    GivenTrackRecord(
    [
      ..OldHistory,
      ..RecentHistory
    ]);

    WhenMeasureConvergence();

    ThenConvergenceIsSameAsForHistory(RecentHistory);
  }

  void ThenConvergenceIsSameAsForHistory(ImmutableArray<(int Amount, bool Result)> RecentHistory)
  {
    var Tracker = new ConvergenceTracker(Length);
    
    ApplyToTracker(RecentHistory, Tracker);
    
    ThenConvergenceIs(Tracker.MeasureConvergence());
  }

  ImmutableArray<(int Amount, bool Result)> AnyTrackRecord(int Amount)
  {
    var Result = new List<(int, bool)>();

    while (Amount > 0)
    {
      var ChunkSize = Any.Int(1, Amount);

      Result.Add((ChunkSize, Any.Bool));

      Amount -= ChunkSize;
    }

    return [..Result];
  }

  void GivenTrackRecord(params ImmutableArray<(int Amount, bool Result)> Trials)
  {
    ApplyToTracker(Trials, Tracker);
  }

  static void ApplyToTracker(ImmutableArray<(int Amount, bool Result)> Trials, ConvergenceTracker ConvergenceTracker)
  {
    foreach (var Run in from Run in Trials from _ in Enumerable.Range(0, Run.Amount) select Run)
      ConvergenceTracker.RecordResult(Run.Result);
  }

  void ThenConvergenceIs(double Expected)
  {
    Convergence.Should().BeApproximately(Expected, 0.00001);
  }

  void WhenMeasureConvergence()
  {
    Convergence = Tracker.MeasureConvergence();
  }
}

public class ConvergenceTracker(int Length)
{
  readonly Queue<bool> Results = new();

  public double MeasureConvergence()
  {
    var Successes = Results.Count(B => B);
    return 1d * Successes / Length;
  }

  public void RecordResult(bool RunResult)
  {
    Results.Enqueue(RunResult);
    while (Results.Count > Length) 
      Results.Dequeue();
  }
}