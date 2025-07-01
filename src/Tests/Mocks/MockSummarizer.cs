using System.Collections.Immutable;
using ThoughtSharp.Scenarios;

namespace Tests.Mocks;

class MockSummarizer : Summarizer
{
  readonly List<(ImmutableArray<float> Argument, float Response)> Conditions = [];

  public void SetUpResponse(ImmutableArray<float> Argument, float Response)
  {
    Conditions.Add((Argument,Response));
  }

  public float Summarize(ImmutableArray<float> Values)
  {
    if (!Conditions.Any(C => C.Argument.SequenceEqual(Values)))
      Conditions.Add((Values, Any.Float));

    var (_, Response) = Conditions.Single(C => C.Argument.SequenceEqual(Values));

    return Response;
  }
}