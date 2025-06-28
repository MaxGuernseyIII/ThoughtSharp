using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public sealed record Grade(ImmutableArray<(float Score, string Reason)> ScoresAndReasons)
{
  public Grade(ImmutableArray<float> Scores)
    : this([..Scores.Select(S => (S, ""))])
  {
  }

  public ImmutableArray<(float Score, string Reason)> ScoresAndReasons { get; } = ScoresAndReasons;

  public bool Equals(Grade? Other)
  {
    return Other != null && (ReferenceEquals(this, Other) || ScoresAndReasons.SequenceEqual(Other.ScoresAndReasons));
  }

  public override int GetHashCode()
  {
    return ScoresAndReasons.Aggregate(0, HashCode.Combine);
  }
}