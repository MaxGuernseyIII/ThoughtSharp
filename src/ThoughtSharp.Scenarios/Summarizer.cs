using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public interface Summarizer
{
  float Summarize(ImmutableArray<float> Values);
}