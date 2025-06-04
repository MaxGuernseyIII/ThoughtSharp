using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class AutomationPass(ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps, Gate SaveGate, Saver Saver, Reporter Reporter) : AutomationJob
{
  public async Task Run()
  {
    foreach (var (Node, Runnable) in Steps)
    {
      var Result = await Runnable.Run();
      Reporter.ReportRunResult(Node, Result);
    }

    if (SaveGate.IsOpen)
      Saver.Save();
  }
}

