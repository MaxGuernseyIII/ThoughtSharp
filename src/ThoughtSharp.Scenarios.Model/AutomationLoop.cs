namespace ThoughtSharp.Scenarios.Model;

public class AutomationLoop(AutomationJob Pass, Gate ContinueGate) : AutomationJob
{
  public async Task Run()
  {
    while (ContinueGate.IsOpen) 
      await Pass.Run();
  }
}