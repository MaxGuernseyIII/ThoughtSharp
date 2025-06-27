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

  [TestClass]
  public class ShouldBeApproximately
  {
    float TotalSuccessRadius;
    float TotalFailureRadius;
    float Target;
    Grade Grade = null!;
    float Actual;

    [TestInitialize]
    public void SetUp()
    {
      TotalSuccessRadius = Any.FloatWithin(.2f, .1f);
      TotalFailureRadius = Any.FloatGreaterThan(TotalSuccessRadius);
      Target = Any.Float;
    }

    [TestMethod]
    public void TargetValueWithinSuccessRange()
    {
      GivenActualIs(Any.FloatWithin(Target, TotalSuccessRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess);
    }

    [DataRow(-1f)]
    [DataRow(1f)]
    [TestMethod]
    public void TargetAtEdgeOfSuccessRange(float TotalSuccessRadiusMultiplier)
    {
      GivenActualIs(Target + TotalSuccessRadius * TotalSuccessRadiusMultiplier);

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess);
    }

    [TestMethod]
    public void TargetValueBelowRange()
    {
      GivenActualIs(Any.FloatLessThanOrEqualTo(Target - TotalFailureRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure);
    }

    [TestMethod]
    public void TargetValueAboveRange()
    {
      GivenActualIs(Any.FloatGreaterThanOrEqualTo(Target + TotalFailureRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure);
    }

    void GivenActualIs(float Value)
    {
      Actual = Value;
    }

    void WhenAssertApproximateValue()
    {
      Grade = Actual.ShouldConvergeOn().Approximately(Target, TotalSuccessRadius, TotalFailureRadius);
    }

    void ThenGradeShouldBe(double Expected)
    {
      Grade.Should().Be(new Grade([Expected]));
    }
  }
}