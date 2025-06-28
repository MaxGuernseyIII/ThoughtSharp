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

namespace ThoughtSharp.Scenarios;

public static partial class Summarizers
{
  public static class Means
  {
    public static Summarizer Harmonic { get; } = new HarmonicMeanSummarizer();
    public static Summarizer Geometric { get; } = new GeometricMeanSummarizer();
    public static Summarizer Arithmetic { get; } = new MeanSummarizer();
  }

  public static class Quantiles
  {
    public static Summarizer Q1 { get; } = Percentile(25);
    public static Summarizer Median { get; } = Percentile(50);
    public static Summarizer Q3 { get; } = Percentile(75);
  }

  public static Summarizer Quantile(float Quantile)
  {
    return new QuantileSummarizer(Quantile);
  }

  public static Summarizer PercentWithSuccessOverThreshold(float Threshold)
  {
    return new SuccessOverThresholdSummarizer(Threshold);
  }

  public static Summarizer MeanMinusStandardDeviationTimesWeight(float StandardDeviationWeight)
  {
    return SpreadPenalizedCenter(Means.Arithmetic, PowerMean(2), StandardDeviationWeight);
  }

  public static Summarizer SpreadPenalizedCenter(Summarizer CenterMetric, Summarizer SpreadMetric, float SpreadWeight)
  {
    return new SpreadPenalizedCenterSummarizer(CenterMetric, SpreadMetric, SpreadWeight);
  }

  public static Summarizer Percentile(int Percent)
  {
    return Quantile(0.01f * Percent);
  }
}