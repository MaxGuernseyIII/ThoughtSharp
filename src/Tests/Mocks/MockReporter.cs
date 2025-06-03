using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockReporter : Reporter
{
  public Dictionary<ScenariosModelNode, List<RunResult>> Results = [];

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
    if (!Results.TryGetValue(Node, out var List))
      Results[Node] = List = new();

    List.Add(Result);
  }
}