using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public sealed record Grade(ImmutableArray<double> Scores)
{
  public ImmutableArray<double> Scores { get; } = Scores;

  public bool Equals(Grade? Other)
  {
    return Other != null && (ReferenceEquals(this, Other) || Scores.SequenceEqual(Other.Scores));
  }

  public override int GetHashCode()
  {
    return Scores.Aggregate(0, HashCode.Combine);
  }
}