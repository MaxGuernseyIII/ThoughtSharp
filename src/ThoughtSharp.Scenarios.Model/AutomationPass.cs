using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class AutomationPass : AutomationJob
{
  readonly ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps;
  readonly Gate SaveGate;
  readonly Saver Saver;
  readonly Reporter Reporter;

  internal AutomationPass(ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps, Gate SaveGate, Saver Saver, Reporter Reporter)
  {
    this.Steps = Steps;
    this.SaveGate = SaveGate;
    this.Saver = Saver;
    this.Reporter = Reporter;
  }

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

