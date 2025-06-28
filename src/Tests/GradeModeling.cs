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
    var ScoresAndReasons = AnyScoresAndReasons();
    
    var Grade = new Grade(ScoresAndReasons);

    Grade.Should().Be(new Grade(ScoresAndReasons));
  }

  [TestMethod]
  public void DefaultReason()
  {
    var Scores = Any.FloatArray();
    
    var Grade = new Grade([..Scores]);

    Grade.Should().Be(new Grade([..Scores.Select(S => (S, ""))]));
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingScore()
  {
    var ScoresAndReasons = AnyScoresAndReasons();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneDifferentScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfDifferingReason()
  {
    var ScoresAndReasons = AnyScoresAndReasons();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneDifferentReason(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfMissingScoreReasonPair()
  {
    var ScoresAndReasons = AnyScoresAndReasons();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneLessScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void InequalityBecauseOfExtraScoreReasonPair()
  {
    var ScoresAndReasons = AnyScoresAndReasons();
    var Grade = new Grade(ScoresAndReasons);
    var Altered = GivenOneExtraScore(ScoresAndReasons);

    var Other = new Grade(Altered);

    Grade.Should().NotBe(Other);
  }

  [TestMethod]
  public void ExposesScoresWithAttachedReasons()
  {
    var ScoresAndReasons = AnyScoresAndReasons();
    
    var Grade = new Grade(ScoresAndReasons);

    Grade.ScoresAndReasons.Should().BeEquivalentTo(ScoresAndReasons, O => O.WithStrictOrdering());
  }

  ImmutableArray<(float Score, string Reason)> GivenOneDifferentReason(ImmutableArray<(float Score, string Reason)> ScoresAndReasons)
  {
    var Index = Any.IndexOf(ScoresAndReasons);
    var Original = ScoresAndReasons[Index];
   
    return ScoresAndReasons.SetItem(Index, (Original.Score, Any.NormalString));
  }

  static ImmutableArray<(float Score, string Reason)> GivenOneDifferentScore(ImmutableArray<(float Score, string Reason)> ScoresAndReasons)
  {
    var Index = Any.IndexOf(ScoresAndReasons);
    var Original = ScoresAndReasons[Index];

    return ScoresAndReasons.SetItem(Index, (Any.FloatOutsideOf(Original.Score, 0.001f), Original.Reason));
  }

  static ImmutableArray<(float Score, string Reason)> GivenOneLessScore(ImmutableArray<(float Score, string Reason)> ScoresAndReasons)
  {
    var Index = Any.IndexOf(ScoresAndReasons);
    
    return ScoresAndReasons.RemoveAt(Index);
  }

  static ImmutableArray<(float Score, string Reason)> GivenOneExtraScore(ImmutableArray<(float Score, string Reason)> ScoresAndReasons)
  {
    var Index = Any.Int(0, ScoresAndReasons.Length);
    
    return ScoresAndReasons.Insert(Index, (Any.Float, Any.NormalString));
  }

  static ImmutableArray<(float Score, string Reason)> AnyScoresAndReasons()
  {
    return [..Any.FloatArray().Select(Score => (Score, Reason: Any.NormalString))];
  }
}

//[TestClass]
//public class GradeBuilding
//{
//}