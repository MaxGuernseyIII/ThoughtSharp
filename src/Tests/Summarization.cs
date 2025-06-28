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
public class Summarization
{
  const float Epsilon = 0.0001f;

  [TestMethod]
  public void PowerMean()
  {
    var Values = Any.FloatArray();
    var Power = Any.FloatWithin(2, 1.75f);
    var Summarizer = Summarizers.PowerMean(Power);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(MathF.Pow(Values.Select(V => MathF.Pow(V, Power)).Sum() / Values.Length, 1 / Power));
  }

  [TestMethod]
  public void PowerMeanEquivalence()
  {
    var Power = Any.FloatWithin(2, 1.75f);

    var Summarizer = Summarizers.PowerMean(Power);

    Summarizer.Should().Be(Summarizers.PowerMean(Power));
  }

  [TestMethod]
  public void PowerMeanDifference()
  {
    var Power = Any.FloatWithin(2, 1.75f);

    var Summarizer = Summarizers.PowerMean(Power);

    Summarizer.Should().NotBe(Summarizers.PowerMean(Any.FloatOutsideOf(Power, Epsilon)));
  }

  [TestMethod]
  public void ArithmeticMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.ArithmeticMean;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Average());
  }

  [TestMethod]
  public void GeometricMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.GeometricMean;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(MathF.Exp(Values.Select(V => MathF.Log(V)).Sum() / Values.Length));
  }

  [TestMethod]
  public void HarmonicMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = Summarizers.HarmonicMean;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Length / Values.Select(V => 1f / V).Sum());
  }

  [TestMethod]
  public void PercentOverSuccessThreshold()
  {
    var Values = Any.FloatArray(100);
    var Threshold = Any.Float;
    var Summarizer = Summarizers.PercentWithSuccessOverThreshold(Threshold);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(Values.Count(V => V >= Threshold) * 1f / Values.Length);
  }

  [TestMethod]
  public void PercentOverSuccessThresholdEquivalence()
  {
    var Threshold = Any.Float;
    
    var Summarizer = Summarizers.PercentWithSuccessOverThreshold(Threshold);

    Summarizer.Should().Be(Summarizers.PercentWithSuccessOverThreshold(Threshold));
  }

  [TestMethod]
  public void PercentOverSuccessThresholdNonEquivalence()
  {
    var Threshold = Any.Float;
    
    var Summarizer = Summarizers.PercentWithSuccessOverThreshold(Threshold);

    Summarizer.Should().NotBe(Summarizers.PercentWithSuccessOverThreshold(Any.FloatOutsideOf(Threshold, Epsilon)));
  }

  [TestMethod]
  public void Quantile()
  {
    var Values = Any.FloatArray(100);
    var Quantile = Any.Float;
    var Summarizer = Summarizers.Quantile(Quantile);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(Values.Order().ElementAt((int)(Quantile *(Values.Length - 1))));
  }

  [TestMethod]
  public void QuantileEquivalence()
  {
    var Quantile = Any.Float;

    var Summarizer = Summarizers.Quantile(Quantile);

    Summarizer.Should().Be(Summarizers.Quantile(Quantile));
  }

  [TestMethod]
  public void QuantileNonEquivalence()
  {
    var Quantile = Any.Float;

    var Summarizer = Summarizers.Quantile(Quantile);

    Summarizer.Should().NotBe(Summarizers.Quantile(Any.FloatOutsideOf(Quantile, Epsilon)));
  }

  //[TestMethod]
  //public void MeanMinusStandardDeviationTimesK()
  //{
  //  var Values = Any.FloatArray(100);
  //  var K = Any.Float;
  //  var Summarizer = Summarizers.MeanMinusStandardDeviationTimesK(K);

  //  var Summary = Summarizer.Summarize([..Values]);

  //  Summary.Should().Be()
  //}

  // TODO:
  // [ ] metric - metric component
  // [ ] transform deviations - use one summarizer to get the "center", subtract from originals, pass into another summarizer
  // [ ] metric components:
  //     [ ] mean of absolute deviation
  //     [x] root-mean-square (for standard deviation) - this is really just PowerMean(2)
  //     [ ] range (seems like the 0-100 quantile gap but maybe can be optimized)
  //     [ ] Quantile gap
  // [ ] semantic overlay (e.g., construct graph of summarizers for MAD or Standard Deviation)
  //     [ ] comparability of summarizers
  //     [ ] standard deviation
  //     [ ] Interquartile range
}