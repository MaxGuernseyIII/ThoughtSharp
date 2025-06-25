using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public sealed class PowerMeanSummarizer(float Power) : Summarizer
{
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
}