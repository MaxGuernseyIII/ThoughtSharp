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

  public class GradedAssertionsTestBase
  {
    protected float Target;
    protected float Actual;
    protected Transcript Transcript = null!;

    [TestInitialize]
    public void SetUpBase()
    {
      Target = Any.Float;
      Transcript = null!;
    }

    protected void GivenActualIs(float Value)
    {
      Actual = Value;
    }

    protected void ThenGradeShouldBe(float ExpectedScore, string ExpectedReason)
    {
      Transcript.Grades.Should().ContainSingle();
      var Pair = Transcript.Grades[0];
      Pair.Score.Should().BeApproximately(ExpectedScore, 0.0001f);
      Pair.Annotations.Should()
        .BeEquivalentTo(ExpectedReason);
    }
  }

  [TestClass]
  public class AtMost : GradedAssertionsTestBase
  {
    float TotalFailureRadius;

    [TestInitialize]
    public void SetUp()
    {
      TotalFailureRadius = Any.Float;
    }

    [TestMethod]
    public void TargetValueWithinSuccessRange()
    {
      GivenActualIs(Any.FloatLessThanOrEqualTo(Target));

      WhenAssertAtMostValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess, ExpectedReason);
    }

    [TestMethod]
    public void TargetValueAtBottomEdgeOfSuccessRange()
    {
      GivenActualIs(Any.FloatGreaterThanOrEqualTo(Target + TotalFailureRadius));

      WhenAssertAtMostValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure, ExpectedReason);
    }

    [DataRow(0f)]
    [DataRow(0.1f)]
    [DataRow(0.5f)]
    [DataRow(0.9f)]
    [DataRow(1f)]
    [TestMethod]
    public void TargetValueInGradedRange(float FailureQuotient)
    {
      GivenActualIs(Target + TotalFailureRadius * FailureQuotient);

      WhenAssertAtMostValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess - FailureQuotient, ExpectedReason);
    }

    void WhenAssertAtMostValue()
    {
      Transcript = Actual.ShouldConvergeOn().AtMost(Target, TotalFailureRadius);
    }

    string ExpectedReason => $"Expected <= {Target} (total failure at >= {Target + TotalFailureRadius}) and found {Actual}";
  }

  [TestClass]
  public class AtLeast : GradedAssertionsTestBase
  {
    float TotalFailureRadius;

    [TestInitialize]
    public void SetUp()
    {
      TotalFailureRadius = Any.Float;
    }

    [TestMethod]
    public void TargetValueWithinSuccessRange()
    {
      GivenActualIs(Any.FloatGreaterThanOrEqualTo(Target));

      WhenAssertAtLeastValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess, ExpectedReason);
    }

    [TestMethod]
    public void TargetValueAtBottomEdgeOfSuccessRange()
    {
      GivenActualIs(Any.FloatLessThanOrEqualTo(Target - TotalFailureRadius));

      WhenAssertAtLeastValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure, ExpectedReason);
    }

    [DataRow(0f)]
    [DataRow(0.1f)]
    [DataRow(0.5f)]
    [DataRow(0.9f)]
    [DataRow(1f)]
    [TestMethod]
    public void TargetValueInGradedRange(float FailureQuotient)
    {
      GivenActualIs(Target - TotalFailureRadius * FailureQuotient);

      WhenAssertAtLeastValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess - FailureQuotient, ExpectedReason);
    }

    void WhenAssertAtLeastValue()
    {
      Transcript = Actual.ShouldConvergeOn().AtLeast(Target, TotalFailureRadius);
    }

    string ExpectedReason => $"Expected >= {Target} (total failure at <= {Target - TotalFailureRadius}) and found {Actual}";
  }

  [TestClass]
  public class Approximately : GradedAssertionsTestBase
  {
    float TotalSuccessRadius;
    float TotalFailureRadius;

    [TestInitialize]
    public void SetUp()
    {
      TotalSuccessRadius = Any.FloatWithin(.2f, .1f);
      TotalFailureRadius = Any.FloatGreaterThan(TotalSuccessRadius);
    }

    [TestMethod]
    public void TargetValueWithinSuccessRange()
    {
      GivenActualIs(Any.FloatWithin(Target, TotalSuccessRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess, ExpectedReason);
    }

    [DataRow(-1f)]
    [DataRow(1f)]
    [TestMethod]
    public void TargetAtEdgeOfSuccessRange(float TotalSuccessRadiusMultiplier)
    {
      GivenActualIs(Target + TotalSuccessRadius * TotalSuccessRadiusMultiplier);

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalSuccess, ExpectedReason);
    }

    [DataRow(0f)]
    [DataRow(0.1f)]
    [DataRow(0.9f)]
    [DataRow(1f)]
    [TestMethod]
    public void ActualValueInLowerDropOffZone(float FailureQuotient)
    {
      GivenActualIs(Target - (TotalSuccessRadius + (TotalFailureRadius - TotalSuccessRadius) * FailureQuotient));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(1f - FailureQuotient, ExpectedReason);
    }

    [TestMethod]
    public void ActualValueBelowTotalFailureRange()
    {
      GivenActualIs(Any.FloatLessThanOrEqualTo(Target - TotalFailureRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure, ExpectedReason);
    }

    [DataRow(0f)]
    [DataRow(0.1f)]
    [DataRow(0.9f)]
    [DataRow(1f)]
    [TestMethod]
    public void ActualValueInUpperDropOffZone(float FailureQuotient)
    {
      GivenActualIs(Target + (TotalSuccessRadius + (TotalFailureRadius - TotalSuccessRadius) * FailureQuotient));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(1f - FailureQuotient, ExpectedReason);
    }

    [TestMethod]
    public void TargetValueAboveTotalFailureRange()
    {
      GivenActualIs(Any.FloatGreaterThanOrEqualTo(Target + TotalFailureRadius));

      WhenAssertApproximateValue();

      ThenGradeShouldBe(ConvergenceAssertions.TotalFailure, ExpectedReason);
    }

    void WhenAssertApproximateValue()
    {
      Transcript = Actual.ShouldConvergeOn().Approximately(Target, TotalSuccessRadius, TotalFailureRadius);
    }

    string ExpectedReason => $"Expected {Target}±{TotalSuccessRadius} (total failure at ±{TotalFailureRadius}) and got {Actual}";
  }
}