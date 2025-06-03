using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class AutomationPass(ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps, Gate SaveGate, Saver Saver, Reporter Reporter)
{
  public async Task Run()
  {
    foreach (var (_, Runnable) in Steps) 
      await Runnable.Run();
  }
}