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
using ThoughtSharp.Scenarios;

namespace Tests;

[TestClass]
public class GradedAssertions
{
  [TestMethod]
  public void TotalFailureGrade()
  {
    ConvergenceAssertions.TotalFailure.Should().Be(0f);
  }

  [TestMethod]
  public void TotalSuccessGrade()
  {
    ConvergenceAssertions.TotalSuccess.Should().Be(1f);
  }

  [TestMethod]
  public void TargetValueWithinRange()
  {
    var TotalSuccessRadius = Any.FloatWithin(.1f, .2f);
    var TotalFailureRadius = Any.FloatGreaterThan(TotalSuccessRadius);
    var Target = Any.Float;
    var Actual = Any.FloatWithin(Target - TotalSuccessRadius, Target + TotalSuccessRadius);

    var Grade = Actual.ShouldConvergeOn().BeingApproximately(Target, TotalSuccessRadius, TotalFailureRadius);

    Grade.Should().Be(new Grade([ConvergenceAssertions.TotalSuccess]));
  }

  [TestMethod]
  public void TargetValueBelowRange()
  {
    var TotalSuccessRadius = Any.FloatWithin(.1f, .2f);
    var TotalFailureRadius = Any.FloatGreaterThan(TotalSuccessRadius);
    var Target = Any.Float;
    var Actual = Any.FloatLessThanOrEqualTo(Target - TotalFailureRadius);

    var Grade = Actual.ShouldConvergeOn().BeingApproximately(Target, TotalSuccessRadius, TotalFailureRadius);

    Grade.Should().Be(new Grade([ConvergenceAssertions.TotalFailure]));
  }
}