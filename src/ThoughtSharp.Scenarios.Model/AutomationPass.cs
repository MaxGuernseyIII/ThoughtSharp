using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class AutomationPass(ImmutableArray<Runnable> Steps, Gate SaveGate, Saver Saver, Reporter Reporter)
{
  public async Task Run()
  {
    foreach (var Runnable in Steps) 
      await Runnable.Run();
  }
}