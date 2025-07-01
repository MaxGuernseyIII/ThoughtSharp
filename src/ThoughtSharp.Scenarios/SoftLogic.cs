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

public static class SoftLogic
{
  public static Summarizer Or(float Laxness)
  {
    Assert.Critical(Laxness > 1f, "soft OR cannot have laxness <= 1");

    return Summarizers.Means.PowerMean(Laxness);
  }

  public static Summarizer And(float Strictness)
  {
    Assert.Critical(Strictness > 1f, "soft AND cannot have strictness <= 1");

    return Summarizers.Means.PowerMean(1 / Strictness);
  }
}

public static class ConvergenceMetrics
{
  public class SoftAndBase(float Strictness) : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.And(Strictness);
    }
  }

  public class SoftOrBase(float Laxness) : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.Or(Laxness);
    }
  }

  public class SuccessRateBase(float SuccessThreshold) : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return Summarizers.Convergence.PassRate(SuccessThreshold);
    }
  }

  public class SoftAnd2() : SoftAndBase(2);

  public class SoftAnd3() : SoftAndBase(3);

  public class SoftAnd4() : SoftAndBase(4);

  public class SoftOr2() : SoftOrBase(2);

  public class SoftOr3() : SoftOrBase(3);

  public class SoftOr4() : SoftOrBase(4);

  public class SuccessRate100() : SuccessRateBase(1f);

  public class SuccessRate099() : SuccessRateBase(0.99f);

  public class SuccessRate095() : SuccessRateBase(0.95f);
}