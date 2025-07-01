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
    MetricTest<Metrics.SoftAnd2>(SoftLogic.And(2));
  }

  [TestMethod]
  public void SoftAnd3()
  {
    MetricTest<Metrics.SoftAnd3>(SoftLogic.And(3));
  }

  [TestMethod]
  public void SoftAnd4()
  {
    MetricTest<Metrics.SoftAnd4>(SoftLogic.And(4));
  }

  [TestMethod]
  public void SoftOr2()
  {
    MetricTest<Metrics.SoftOr2>(SoftLogic.Or(2));
  }

  [TestMethod]
  public void SoftOr3()
  {
    MetricTest<Metrics.SoftOr3>(SoftLogic.Or(3));
  }

  [TestMethod]
  public void SoftOr4()
  {
    MetricTest<Metrics.SoftOr4>(SoftLogic.Or(4));
  }
}