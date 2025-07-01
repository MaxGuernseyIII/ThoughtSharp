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

public static class Metrics
{
  public class SoftAnd2 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.And(2);
    }
  }

  public class SoftAnd3 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.And(3);
    }
  }

  public class SoftAnd4 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.And(4);
    }
  }

  public class SoftOr2 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.Or(2);
    }
  }

  public class SoftOr3 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.Or(3);
    }
  }

  public class SoftOr4 : Metric
  {
    public Summarizer CreateSummarizer()
    {
      return SoftLogic.Or(4);
    }
  }
}