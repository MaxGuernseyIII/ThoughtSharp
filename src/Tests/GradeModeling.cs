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
using FluentAssertions;
using ThoughtSharp.Scenarios;

namespace Tests;

[TestClass]
public class GradeModeling
{
  [TestMethod]
  public void Equality()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    
    var Grade = new Grade(ScoresAndReasons);

    Grade.Should().Be(new Grade(ScoresAndReasons));
  }

  [TestMethod]
  public void NoAnnotationsByDefaul()
  {
    var Scores = Any.FloatArray();
    
    var Grade = new Grade([..Scores]);

    Grade.Should().Be(new Grade([..Scores.Select(S => new AnnotatedScore { Score = S, Annotations = [] })]));
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingScore()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneDifferentScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingReason()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneDifferentReason(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfMissingScoreReasonPair()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneLessScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfExtraScoreReasonPair()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneExtraScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void ExposesScoresWithAttachedReasons()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    
    var Grade = new Grade(ScoresAndReasons);

    Grade.AnnotatedScores.Should().BeEquivalentTo(ScoresAndReasons, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void JoinGrades()
  {
    var Grades = Any.ListOf(() => Any.Grade, 1, 3);

    var Joined = Grade.Join(Grades);

    Joined.Should().Be(new Grade([..Grades.SelectMany(G => G.AnnotatedScores)]));
  }

  ImmutableArray<AnnotatedScore> GivenOneDifferentReason(ImmutableArray<AnnotatedScore> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneReplaced(V => V with { Annotations = [..Any.ListOf(() => Any.NormalString, 1, 3)] });
  }

  static ImmutableArray<AnnotatedScore> GivenOneDifferentScore(ImmutableArray<AnnotatedScore> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneReplaced(V => V with { Score = Any.Float});
  }

  static ImmutableArray<AnnotatedScore> GivenOneLessScore(ImmutableArray<AnnotatedScore> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneLess();
  }

  static ImmutableArray<AnnotatedScore> GivenOneExtraScore(ImmutableArray<AnnotatedScore> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneMore(() => Any.AnnotatedScore);
  }

  static ImmutableArray<AnnotatedScore> AnyAnnotatedScores()
  {
    return [..Any.ListOf(() => Any.AnnotatedScore, 1, 3)];
  }
}
