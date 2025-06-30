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
public class TranscriptModeling
{
  [TestMethod]
  public void Equality()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    
    var Grade = new Transcript(ScoresAndReasons);

    Grade.Should().Be(new Transcript(ScoresAndReasons));
  }

  [TestMethod]
  public void NoAnnotationsByDefault()
  {
    var Scores = Any.FloatArray();
    
    var Grade = new Transcript([..Scores]);

    Grade.Should().Be(new Transcript([..Scores.Select(S => new Grade { Score = S, Annotations = [] })]));
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingScore()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Transcript(ScoresAndReasons);
    var Altered = GivenOneDifferentScore(ScoresAndReasons);

    var Other = new Transcript(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingReason()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Transcript(ScoresAndReasons);
    var Altered = GivenOneDifferentReason(ScoresAndReasons);

    var Other = new Transcript(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfMissingScoreReasonPair()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Transcript(ScoresAndReasons);
    var Altered = GivenOneLessScore(ScoresAndReasons);

    var Other = new Transcript(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfExtraScoreReasonPair()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    var Grade = new Transcript(ScoresAndReasons);
    var Altered = GivenOneExtraScore(ScoresAndReasons);

    var Other = new Transcript(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void ExposesScoresWithAttachedReasons()
  {
    var ScoresAndReasons = AnyAnnotatedScores();
    
    var Grade = new Transcript(ScoresAndReasons);

    Grade.Grades.Should().BeEquivalentTo(ScoresAndReasons, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void JoinTranscripts()
  {
    var Grades = Any.ListOf(() => Any.Transcript, 1, 3);

    var Joined = Transcript.Join(Grades);

    Joined.Should().Be(new Transcript([..Grades.SelectMany(G => G.Grades)]));
  }

  ImmutableArray<Grade> GivenOneDifferentReason(ImmutableArray<Grade> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneReplaced(V => V with { Annotations = [..Any.ListOf(() => Any.NormalString, 1, 3)] });
  }

  static ImmutableArray<Grade> GivenOneDifferentScore(ImmutableArray<Grade> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneReplaced(V => V with { Score = Any.Float});
  }

  static ImmutableArray<Grade> GivenOneLessScore(ImmutableArray<Grade> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneLess();
  }

  static ImmutableArray<Grade> GivenOneExtraScore(ImmutableArray<Grade> ScoresAndReasons)
  {
    return ScoresAndReasons.WithOneMore(() => Any.Grade);
  }

  static ImmutableArray<Grade> AnyAnnotatedScores()
  {
    return [..Any.ListOf(() => Any.Grade, 1, 3)];
  }
}