using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockReporter : MockDisposable, Reporter
{
  public Action<ScenariosModelNode> ReportEnterBehavior = delegate { };
  public Action<ScenariosModelNode> ReportExitBehavior = delegate { };

  public Dictionary<ScenariosModelNode, List<RunResult>> Results = [];

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
    if (!Results.TryGetValue(Node, out var List))
      Results[Node] = List = new();

    List.Add(Result);
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    ReportEnterBehavior(Node);
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    ReportExitBehavior(Node);
  }
}