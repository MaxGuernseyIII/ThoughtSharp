using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public class PowerMeanSummarizer(float Power) : Summarizer
{
  public float Summarize(ImmutableArray<float> Values)
  {
    return 
      MathF.Pow(
        Values.Select(V => MathF.Pow(V, Power)).Sum() / Values.Length, 1 / Power);
  }
}