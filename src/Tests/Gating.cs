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
public class Gating
{
  [TestMethod]
  public void BasedOnTrackerAndThresholdFailsWhenConvergenceTooLow()
  {
    var Length = Any.Int(1, 10);
    var Tracker = new ConvergenceTracker(Length);
    Tracker.ApplyHistory(Any.ConvergenceRecord(Length));
    var Gate = ThoughtSharp.Scenarios.Model.Gate.ForConvergenceTrackerAndThreshold(Tracker,
      Tracker.MeasureConvergence() + ConvergenceConstants.Precision);

    var GateIsOpen = Gate.IsOpen;

    GateIsOpen.Should().Be(false);
  }

  [TestMethod]
  public void BasedOnTrackerAndThresholdSucceedsWhenConvergenceIsHighEnough()
  {
    var Length = Any.Int(1, 10);
    var Tracker = new ConvergenceTracker(Length);
    Tracker.ApplyHistory(Any.ConvergenceRecord(Length));
    var Gate = ThoughtSharp.Scenarios.Model.Gate.ForConvergenceTrackerAndThreshold(Tracker,
      Tracker.MeasureConvergence());

    var GateIsOpen = Gate.IsOpen;

    GateIsOpen.Should().Be(true);
  }

  [TestMethod]
  [DataRow(false, false, false)]
  [DataRow(false, true, false)]
  [DataRow(true, false, false)]
  [DataRow(true, true, true)]
  public void AndGate(bool LeftValue, bool RightVale, bool Expected)
  {
    var LeftGate = new MockGate(LeftValue);
    var RightGate = new MockGate(RightVale);
    var AndGate = Gate.ForAnd(LeftGate, RightGate);

    var GateIsOpen = AndGate.IsOpen;

    GateIsOpen.Should().Be(Expected);
  }

  [TestMethod]
  [DataRow(false, false, false)]
  [DataRow(false, true, true)]
  [DataRow(true, false, true)]
  [DataRow(true, true, true)]
  public void OrGate(bool LeftValue, bool RightVale, bool Expected)
  {
    var LeftGate = new MockGate(LeftValue);
    var RightGate = new MockGate(RightVale);
    var AndGate = Gate.ForOr(LeftGate, RightGate);

    var GateIsOpen = AndGate.IsOpen;

    GateIsOpen.Should().Be(Expected);
  }

  [TestMethod]
  public void CounterAndThresholdGateWhenCountIsBelowThreshold()
  {
    var Counter = new Counter();
    var Threshold = Any.Int(10, 20);
    var G = Gate.ForCounterAndThreshold(Counter, Threshold);
    Counter.Value = Any.Int(0, Threshold - 1);

    var IsOpen = G.IsOpen;

    IsOpen.Should().Be(false);
  }

  [TestMethod]
  public void CounterAndThresholdGateWhenCountIsAtOrAboveThreshold()
  {
    var Counter = new Counter();
    var Threshold = Any.Int(10, 20);
    var G = Gate.ForCounterAndThreshold(Counter, Threshold);
    Counter.Value = Any.Int(Threshold, Threshold + 1);

    var IsOpen = G.IsOpen;

    IsOpen.Should().Be(true);
  }
}