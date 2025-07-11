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
public class ConvergenceTracking
{
  double Convergence;
  int Length;
  MockSummarizer Summarizer = null!;
  ConvergenceTracker Tracker = null!;

  [TestInitialize]
  public void SetUp()
  {
    Length = Any.Int(10, 100);
    Summarizer = new();
    Tracker = new(Length, Summarizer);
  }

  //[TestMethod]
  //public void StartsOffAtZero()
  //{
  //  WhenMeasureConvergence();

  //  ThenConvergenceIs(0);
  //}

  //[TestMethod]
  //public void ComputesBasedOnTotalSuccessesWhenNotFull()
  //{
  //  var Amount = Any.Int(1, Length - 1);
  //  GivenTrackRecord((Amount, true));

  //  WhenMeasureConvergence();

  //  ThenConvergenceIs(1d * Amount / Length);
  //}

  //[TestMethod]
  //public void DoesNotIncludeFailuresAsConvergence()
  //{
  //  var Amount = Any.Int(1, Length - 1);
  //  GivenTrackRecord((Amount, true), (1, false));

  //  WhenMeasureConvergence();

  //  ThenConvergenceIs(1d * Amount / Length);
  //}

  [TestMethod]
  public void UsesMetricToComputeHistory()
  {
    var History = Any.ConvergenceRecord(Length);
    var ExpectedSummary = Summarizer.Summarize([..History]);
    GivenTrackRecord(History);

    WhenMeasureConvergence();

    ThenConvergenceIs(ExpectedSummary);
  }

  [TestMethod]
  public void MetricIsComputedBasedOnFullLength()
  {
    var History = Any.ConvergenceRecord(Any.Int(0, Length - 1));
    var EffectiveHistory = History.Concat(Enumerable.Repeat(0f, Length - History.Length));
    var ExpectedSummary = Summarizer.Summarize([..EffectiveHistory]);
    GivenTrackRecord(History);

    WhenMeasureConvergence();

    ThenConvergenceIs(ExpectedSummary);
  }

  [TestMethod]
  public void DecomposesGradesIntoConstituentScores()
  {
    var History = Any.ConvergenceRecord(Any.Int(0, Length * 2));
    var Grade = new Transcript([..History.Select(S => new Grade() { Score = S, Annotations = [] })]);
    GivenTrackRecord(Grade);

    WhenMeasureConvergence();

    ThenConvergenceIsSameAsForHistory(History);
  }

  [TestMethod]
  public void OldResultsFallOff()
  {
    var OldHistory = Any.ConvergenceRecord(1);
    var RecentHistory = Any.ConvergenceRecord(Length);
    GivenTrackRecord(
    [
      ..OldHistory,
      ..RecentHistory
    ]);

    WhenMeasureConvergence();

    ThenConvergenceIsSameAsForHistory(RecentHistory);
  }

  [TestMethod]
  public void Equivalence()
  {
    var Length = Any.Int(1, 20);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(0, 10));
    var Tracker1 = GivenTrackerWith(Length, Metric, History);
    var Tracker2 = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreEquivalent(Tracker1, Tracker2);
  }

  [TestMethod]
  public void NonEquivalenceByLength()
  {
    var Length = Any.Int(1, 20);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(0, Length));
    var Actual = GivenTrackerWith(Any.IntOtherThan(Length), Metric, History);
    var Expected = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreNotEquivalent(Actual, Expected);
  }

  [TestMethod]
  public void NonEquivalenceMissingHistory()
  {
    var Length = Any.Int(20, 30);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(1, Length));
    var Actual = GivenTrackerWith(Length, Metric, History.WithOneLess());
    var Expected = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreNotEquivalent(Actual, Expected);
  }

  [TestMethod]
  public void NonEquivalenceExtraHistory()
  {
    var Length = Any.Int(20, 30);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(0, Length));
    var Actual = GivenTrackerWith(Length, Metric, History.WithOneMore(() => Any.Float));
    var Expected = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreNotEquivalent(Actual, Expected);
  }

  [TestMethod]
  public void NonEquivalenceSwappedHistory()
  {
    var Length = Any.Int(20, 100);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(1, Length));
    var Actual = GivenTrackerWith(Length, Metric, History.WithOneReplaced(_ => Any.Float));
    var Expected = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreNotEquivalent(Actual, Expected);
  }

  [TestMethod]
  public void NonEquivalenceMetric()
  {
    var Length = Any.Int(1, 20);
    var Metric = GivenMetric();
    var History = Any.ConvergenceRecord(Any.Int(0, Length));
    var Actual = GivenTrackerWith(Length, GivenMetric(), History);
    var Expected = GivenTrackerWith(Length, Metric, History);

    ThenTrackersAreNotEquivalent(Actual, Expected);
  }

  static void ThenTrackersAreEquivalent(ConvergenceTracker Actual, ConvergenceTracker Expected)
  {
    Actual.Should().Be(Expected);
    Actual.GetHashCode().Should().Be(Expected.GetHashCode());
  }

  static void ThenTrackersAreNotEquivalent(ConvergenceTracker Actual, ConvergenceTracker Expected)
  {
    Actual.Should().NotBe(Expected);
  }

  static ConvergenceTracker GivenTrackerWith(
    int Length,
    Summarizer Metric,
    params ImmutableArray<float> History)
  {
    var Tracker = new ConvergenceTracker(Length, Metric);
    Tracker.ApplyHistory(History);

    return Tracker;
  }

  Summarizer GivenMetric()
  {
    return new MockSummarizer();
  }

  void ThenConvergenceIsSameAsForHistory(ImmutableArray<float> ReferenceHistory)
  {
    var Tracker = new ConvergenceTracker(Length, Summarizer);

    Tracker.ApplyHistory(ReferenceHistory);

    ThenConvergenceIs(Tracker.MeasureConvergence());
  }

  void GivenTrackRecord(params ImmutableArray<float> Trials)
  {
    Tracker.ApplyHistory(Trials);
  }

  void GivenTrackRecord(Transcript Transcript)
  {
    Tracker.RecordResult(Transcript);
  }

  void ThenConvergenceIs(double Expected)
  {
    Convergence.Should().BeApproximately(Expected, ConvergenceConstants.Precision);
  }

  void WhenMeasureConvergence()
  {
    Convergence = Tracker.MeasureConvergence();
  }
}