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
using Tests.Mocks;
using ThoughtSharp.Scenarios;
using Assert = ThoughtSharp.Scenarios.Assert;

namespace Tests;

[TestClass]
public class AssertingFuzzyObjects : AssertingBase<MockData, MockData>
{
  MockSummarizer Summarizer = null!;
  MockData Target = null!;

  [TestInitialize]
  public void SetUp()
  {
    Summarizer = new();
    Payload = new() { Value1 = Any.Float, Value2 = Any.Int() };
    Target = new() {Value1 = Any.Float, Value2 = Any.Int()};
  }

  [TestMethod]
  public void GradesAreSummarized()
  {
    var Result = GivenCognitiveResult();

    var Grade1 = Any.Grade;
    var Grade2 = Any.Grade;

    var FinalGrade = Assert.That(Result)
      .ConvergesOn()
      .WithSummarizer(Summarizer)
      .Target(Target, 
        C => C
          .Expect(D => D.Value1, (_, _) => Grade1)
          .Expect(D => D.Value2, (_, _) => Grade2));

    FinalGrade.Should().Be(Grade.Merge(Summarizer, [Grade1, Grade2]));
  }

  [TestMethod]
  public void GradingExposesActualDataPointsToExpectations()
  {
    var Result = GivenCognitiveResult();

    Assert.That(Result)
      .ConvergesOn()
      .WithSummarizer(Summarizer)
      .Target(Target, 
        C => C
          .Expect(D => D.Value1, (Actual, _) =>
          {
            Actual.Should().Be(Payload.Value1);
            return new() {Score = 0, Annotations = []};
          }));
  }

  [TestMethod]
  public void GradingExposesExpectedDataPointsToExpectations()
  {
    var Result = GivenCognitiveResult();

    Assert.That(Result)
      .ConvergesOn()
      .WithSummarizer(Summarizer)
      .Target(Target, 
        C => C
          .Expect(D => D.Value1, (_, Expected) =>
          {
            Expected.Should().Be(Target.Value1);
            return new() {Score = 0, Annotations = []};
          }));
  }

  [TestMethod]
  public void TargetIsUsedForTraining()
  {
    var Result = GivenCognitiveResult();

    Assert.That(Result)
      .ConvergesOn()
      .WithSummarizer(Summarizer)
      .Target(Target, C => C);

    ThenModelWasTrainedWith([Target]);
  }

  [TestMethod]
  public void DefaultSummarizer()
  {
    var Result = GivenCognitiveResult();
    var WithExplicitSummarizer = Assert.That(Result)
      .ConvergesOn()
      .WithSummarizer(SoftLogic.And(2))
      .Target(Target, SimpleComparison);

    var WithDefaultSummarizer = Assert.That(Result)
      .ConvergesOn()
      .Target(Target, SimpleComparison);

    WithDefaultSummarizer.Should().Be(WithExplicitSummarizer);
  }

  static ObjectConvergenceComparison<MockData> SimpleComparison(ObjectConvergenceComparison<MockData> C)
  {
    return C
      .Expect(D => D.Value1,
        (Actual, Expected) => Actual.ShouldConvergeOn().Approximately(Expected, .01f, 1f))
      .Expect(D => D.Value2,
        (Actual, Expected) => ((float)Actual).ShouldConvergeOn().AtLeast(Expected, 10f));
  }
}