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

namespace ThoughtSharp.Scenarios.Model;

public interface Gate
{
  bool IsOpen { get; }

  static Gate AlwaysOpen => new AlwaysGate(true);

  static Gate ForConvergenceTrackerAndThreshold(ConvergenceTracker Tracker, double Threshold)
  {
    return new ConvergenceTrackerAndThresholdGate(Tracker, Threshold);
  }

  static Gate ForAnd(Gate LeftGate, Gate RightGate)
  {
    return new AndGate(LeftGate, RightGate);
  }

  static Gate ForCounterAndMinimum(HasValue<int> Counter, int Threshold)
  {
    return new CounterAndMinimumGate(Counter, Threshold);
  }

  static Gate ForCounterAndMaximum(HasValue<int> Counter, int Threshold)
  {
    return new CounterAndMaximumGate(Counter, Threshold);
  }

  static Gate ForOr(Gate LeftGate, Gate RightGate)
  {
    return new OrGate(LeftGate, RightGate);
  }

  static Gate NotGate(Gate Underlying)
  {
    return new NotGate(Underlying);
  }
}