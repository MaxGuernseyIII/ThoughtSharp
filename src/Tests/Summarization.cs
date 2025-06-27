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
using static FluentAssertions.FluentActions;

namespace Tests;

[TestClass]
public class Summarization
{
  [TestMethod]
  public void PowerMean()
  {
    var Values = Any.FloatArray();
    var Power = Any.FloatWithin(2, 1.75f);
    var Summarizer = PowerMeanSummarizer.Create(Power);

    var Summary = Summarizer.Summarize([..Values]);

    Summary.Should().Be(MathF.Pow(Values.Select(V => MathF.Pow(V, Power)).Sum() / Values.Length, 1 / Power));
  }

  [TestMethod]
  public void PowerMeanEquivalence()
  {
    var Power = Any.FloatWithin(2, 1.75f);

    var Summarizer = PowerMeanSummarizer.Create(Power);

    Summarizer.Should().Be(PowerMeanSummarizer.Create(Power));
  }

  [TestMethod]
  public void ArithmeticMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = MeanSummarizer.Instance;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Average());
  }

  [TestMethod]
  public void GeometricMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = GeometricMeanSummarizer.Instance;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(MathF.Exp(Values.Select(V => MathF.Log(V)).Sum() / Values.Length));
  }

  [TestMethod]
  public void HarmonicMean()
  {
    var Values = Any.FloatArray();
    var Summarizer = HarmonicMeanSummarizer.Instance;

    var Summary = Summarizer.Summarize([.. Values]);

    Summary.Should().Be(Values.Length  / Values.Select(V => 1f / V).Sum());
  }
}

[TestClass]
public class SoftLogicConstruction
{
  [TestMethod]
  public void SoftOr()
  {
    var Laxness = Any.FloatGreaterThan(1f);

    var SoftOr = SoftLogic.Or(Laxness);

    SoftOr.Should().Be(PowerMeanSummarizer.Create(Laxness));
  }

  [TestMethod]
  public void SoftOrCannotBeLessThanOne()
  {
    Invoking(() => SoftLogic.Or(Any.FloatLessThanOrEqualTo(1f))).Should().Throw<FatalErrorException>().WithMessage(
      "Critical condition not met: soft OR cannot have laxness <= 1");
  }

  [TestMethod]
  public void SoftAnd()
  {
    var Strictness = Any.FloatGreaterThan(1f);

    var SoftAnd = SoftLogic.And(Strictness);

    SoftAnd.Should().Be(PowerMeanSummarizer.Create(1 / Strictness));
  }

  [TestMethod]
  public void SoftAndCannotBeLessThanOne()
  {
    Invoking(() => SoftLogic.And(Any.FloatLessThanOrEqualTo(1f))).Should().Throw<FatalErrorException>().WithMessage(
      "Critical condition not met: soft AND cannot have strictness <= 1");
  }
}