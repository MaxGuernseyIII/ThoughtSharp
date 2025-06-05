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
public class TrainingDataTracking
{
  TrainingMetadata Metadata = null!;
  TrainingDataScheme Scheme = null!;

  [TestInitialize]
  public void SetUp()
  {
    Metadata = new()
    {
      MaximumAttempts = Any.Int(1, 10),
      SampleSize = Any.Int(1, 10),
      SuccessFraction = Any.Float
    };
    Scheme = new(Metadata);
  }

  [TestMethod]
  public void HasAPersistentTimesSinceSavedCounter()
  {

    var Actual = Scheme.TimesSinceSaved;

    Actual.Should().NotBeNull();
    Actual.Should().BeSameAs(Scheme.TimesSinceSaved);
  }

  [TestMethod]
  public void HasAPersistentAttemptsCounter()
  {
    var Actual = Scheme.Attempts;

    Actual.Should().NotBeNull();
    Actual.Should().BeSameAs(Scheme.Attempts);
  }

  [TestMethod]
  public void AttemptsAndTimesSinceSavedCountersAreDistinct()
  {
    Scheme.Attempts.Should().NotBeSameAs(Scheme.TimesSinceSaved);
  }

  [TestMethod]
  public void ManagesPersistentConvergenceTrackerForOneNode()
  {
    var Node = new MockNode();

    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    Tracker.Should().NotBeNull();
    Tracker.Should().BeSameAs(Scheme.GetConvergenceTrackerFor(Node));
  }

  [TestMethod]
  public void PersistentConvergenceTrackersAreCreatedWithCorrectSize()
  {
    var Node = new MockNode();

    var Tracker = Scheme.GetConvergenceTrackerFor(Node);

    Tracker.Should().Be(new ConvergenceTracker(Metadata.SampleSize));
  }

  [TestMethod]
  public void ManagesConvergenceTrackersAreDistinctByNode()
  {
    var Tracker = Scheme.GetConvergenceTrackerFor(new MockNode());

    Tracker.Should().NotBeNull();
    Tracker.Should().NotBeSameAs(Scheme.GetConvergenceTrackerFor(new MockNode()));
  }
}