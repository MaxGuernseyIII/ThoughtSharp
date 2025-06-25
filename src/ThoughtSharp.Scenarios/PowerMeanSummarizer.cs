using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public sealed class PowerMeanSummarizer(float Power) : Summarizer
{
  readonly float Power = Power;

  public static Summarizer Create(float Power)
  {
    return new PowerMeanSummarizer(Power);
  }

  public float Summarize(ImmutableArray<float> Values)
  {
    return 
      MathF.Pow(
        Values.Select(V => MathF.Pow(V, Power)).Sum() / Values.Length, 1 / Power);
  }

  bool Equals(PowerMeanSummarizer Other)
  {
    return Power.Equals(Other.Power);
  }

  public override bool Equals(object? Obj)
  {
    return ReferenceEquals(this, Obj) || Obj is PowerMeanSummarizer Other && Equals(Other);
  }

  public override int GetHashCode()
  {
    return Power.GetHashCode();
  }
}