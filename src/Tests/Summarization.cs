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
using System.Linq.Expressions;
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Scenarios;

namespace Tests;

[TestClass]
public class Summarization
{
  const float Epsilon = 0.0001f;

  [TestMethod]
  public void PowerMean()
  {
    var Values = Any.FloatArray();
    var Power = Any.FloatWithin(2, 1.75f);
    var Summarizer = Summarizers.Means.PowerMean(Power);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(MathF.Pow(Values.Select(V => MathF.Pow(V, Power)).Sum() / Values.Length, 1 / Power));
  }

  [TestMethod]
  public void PowerMeanEquivalence()
  {
    var Power = Any.FloatWithin(2, 1.75f);

    var Summarizer = Summarizers.Means.PowerMean(Power);

    Summarizer.Should().Be(Summarizers.Means.PowerMean(Power));
  }

  [TestMethod]
  public void PowerMeanNonEquivalence()
  {
    var Power = Any.FloatWithin(2, 1.75f);

    var Summarizer = Summarizers.Means.PowerMean(Power);

    Summarizer.Should().NotBe(Summarizers.Means.PowerMean(Any.FloatOutsideOf(Power, Epsilon)));
  }

  [TestMethod]
  public void ArithmeticMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.Means.Arithmetic;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Average());
  }

  [TestMethod]
  public void GeometricMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.Means.Geometric;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(MathF.Exp(Values.Select(V => MathF.Log(V)).Sum() / Values.Length));
  }

  [TestMethod]
  public void HarmonicMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.Means.Harmonic;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Length / Values.Select(V => 1f / V).Sum());
  }

  [TestMethod]
  public void PercentOverSuccessThreshold()
  {
    var Values = Any.FloatArray(100);
    var Threshold = Any.Float;
    var Summarizer = Summarizers.Convergence.PassRate(Threshold);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(Values.Count(V => V >= Threshold) * 1f / Values.Length);
  }

  [TestMethod]
  public void PercentOverSuccessThresholdEquivalence()
  {
    var Threshold = Any.Float;
    
    var Summarizer = Summarizers.Convergence.PassRate(Threshold);

    Summarizer.Should().Be(Summarizers.Convergence.PassRate(Threshold));
  }

  [TestMethod]
  public void PercentOverSuccessThresholdNonEquivalence()
  {
    var Threshold = Any.Float;
    
    var Summarizer = Summarizers.Convergence.PassRate(Threshold);

    Summarizer.Should().NotBe(Summarizers.Convergence.PassRate(Any.FloatOutsideOf(Threshold, Epsilon)));
  }

  [TestMethod]
  public void Quantile()
  {
    var Values = Any.FloatArray(100);
    var Quantile = Any.Float;
    var Summarizer = Summarizers.Quantiles.FromFraction(Quantile);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(Values.Order().ElementAt((int)(Quantile *(Values.Length - 1))));
  }

  [TestMethod]
  public void QuantileEquivalence()
  {
    var Quantile = Any.Float;

    var Summarizer = Summarizers.Quantiles.FromFraction(Quantile);

    Summarizer.Should().Be(Summarizers.Quantiles.FromFraction(Quantile));
  }

  [TestMethod]
  public void QuantileNonEquivalence()
  {
    var Quantile = Any.Float;

    var Summarizer = Summarizers.Quantiles.FromFraction(Quantile);

    Summarizer.Should().NotBe(Summarizers.Quantiles.FromFraction(Any.FloatOutsideOf(Quantile, Epsilon)));
  }

  [TestMethod]
  public void SpreadPenalizedCenter()
  {
    var CenterSummarizer = new MockSummarizer();
    var FeatureSummarizer = new MockSummarizer();
    var Input = Any.FloatArray();
    var Center = GivenSummaryValue(CenterSummarizer, Input);
    var Variation = Input.Select(V => V - Center).ToImmutableArray();
    var Feature = GivenSummaryValue(FeatureSummarizer, Variation);
    var K = Any.Float;
    var Summarizer = Summarizers.Composables.SpreadPenalizedCenter(CenterSummarizer, FeatureSummarizer, K);

    var Summary = Summarizer.Summarize([..Input]);

    Summary.Should().BeApproximately(Center - Feature * K, Epsilon);
  }

  [TestMethod]
  public void TransformToAbsoluteValue()
  {
    var Underlying = new MockSummarizer();
    var Input = Any.FloatArray();
    var Output = GivenSummaryValue(Underlying, [..Input.Select(MathF.Abs)]);
    var Summarizer = Summarizers.Composables.TransformInputsToAbsoluteValue(Underlying);

    var Summary = Summarizer.Summarize([..Input]);

    Summary.Should().Be(Output);
  }

  [TestMethod]
  public void MeanMinusStandardDeviationTimesWeight()
  {
    var Weight = Any.Float;

    var Summarizer = Summarizers.Means.Penalized(Weight);

    Summarizer.Should()
      .Be(Summarizers.Composables.SpreadPenalizedCenter(Summarizers.Means.Arithmetic, Summarizers.Means.PowerMean(2), Weight));
  }

  [TestMethod]
  public void MedianAbsoluteDeviation()
  {
    var Weight = Any.Float;

    var Summarizer = Summarizers.Convergence.MedianAbsoluteDeviation(Weight);

    Summarizer.Should()
      .Be(
        Summarizers.Composables.SpreadPenalizedCenter(
          Summarizers.Quantiles.Median,
          Summarizers.Composables.TransformInputsToAbsoluteValue(
            Summarizers.Quantiles.Median),
          Weight));
  }

  [TestMethod]
  public void PercentileAlias()
  {
    var I = Any.Int(0, 100);

    Summarizers.Quantiles.FromPercent(I).Should().Be(Summarizers.Quantiles.FromFraction(0.01f * I));
  }

  [TestMethod]
  public void PopularQuantiles()
  {
    Summarizers.Quantiles.Q1.Should().Be(Summarizers.Quantiles.FromPercent(25));
    Summarizers.Quantiles.Median.Should().Be(Summarizers.Quantiles.FromPercent(50));
    Summarizers.Quantiles.Q3.Should().Be(Summarizers.Quantiles.FromPercent(75));
  }

  static float GivenSummaryValue(MockSummarizer Underlying, IReadOnlyList<float> Input)
  {
    var Center = Any.Float;
    Underlying.SetUpResponse([..Input], Center);
    return Center;
  }

  // TODO:
  // [ ] Quantile gap
}