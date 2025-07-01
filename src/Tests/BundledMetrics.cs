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
public class BundledMetrics
{
  void MetricTest<T>(Summarizer Expected)
    where T : Metric, new()
  {
    var Metric = new T();

    var Actual = Metric.CreateSummarizer();

    Actual.Should().Be(Expected);
  }

  [TestMethod]
  public void SoftAnd2()
  {
    MetricTest<ConvergenceMetrics.SoftAnd2>(SoftLogic.And(2));
  }

  [TestMethod]
  public void SoftAnd3()
  {
    MetricTest<ConvergenceMetrics.SoftAnd3>(SoftLogic.And(3));
  }

  [TestMethod]
  public void SoftAnd4()
  {
    MetricTest<ConvergenceMetrics.SoftAnd4>(SoftLogic.And(4));
  }

  [TestMethod]
  public void SoftOr2()
  {
    MetricTest<ConvergenceMetrics.SoftOr2>(SoftLogic.Or(2));
  }

  [TestMethod]
  public void SoftOr3()
  {
    MetricTest<ConvergenceMetrics.SoftOr3>(SoftLogic.Or(3));
  }

  [TestMethod]
  public void SoftOr4()
  {
    MetricTest<ConvergenceMetrics.SoftOr4>(SoftLogic.Or(4));
  }

  [TestMethod]
  public void SuccessRate100()
  {
    MetricTest<ConvergenceMetrics.SuccessRate100>(Summarizers.Convergence.PassRate(1f));
  }

  [TestMethod]
  public void SuccessRate099()
  {
    MetricTest<ConvergenceMetrics.SuccessRate099>(Summarizers.Convergence.PassRate(.99f));
  }

  [TestMethod]
  public void SuccessRate095()
  {
    MetricTest<ConvergenceMetrics.SuccessRate095>(Summarizers.Convergence.PassRate(.95f));
  }

  [TestMethod]
  public void Percentile01()
  {
    MetricTest<ConvergenceMetrics.Percentile01>(Summarizers.Quantiles.FromPercent(01));
  }

  [TestMethod]
  public void Percentile02()
  {
    MetricTest<ConvergenceMetrics.Percentile02>(Summarizers.Quantiles.FromPercent(02));
  }

  [TestMethod]
  public void Percentile05()
  {
    MetricTest<ConvergenceMetrics.Percentile05>(Summarizers.Quantiles.FromPercent(05));
  }

  [TestMethod]
  public void Percentile10()
  {
    MetricTest<ConvergenceMetrics.Percentile10>(Summarizers.Quantiles.FromPercent(10));
  }

  [TestMethod]
  public void Percentile90()
  {
    MetricTest<ConvergenceMetrics.Percentile90>(Summarizers.Quantiles.FromPercent(90));
  }

  [TestMethod]
  public void Percentile95()
  {
    MetricTest<ConvergenceMetrics.Percentile95>(Summarizers.Quantiles.FromPercent(95));
  }

  [TestMethod]
  public void Percentile98()
  {
    MetricTest<ConvergenceMetrics.Percentile98>(Summarizers.Quantiles.FromPercent(98));
  }

  [TestMethod]
  public void Percentile99()
  {
    MetricTest<ConvergenceMetrics.Percentile99>(Summarizers.Quantiles.FromPercent(99));
  }
}