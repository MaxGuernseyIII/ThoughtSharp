namespace ThoughtSharp.Scenarios.Model;

public sealed record AutomationLoop(AutomationJob Pass, Gate ContinueGate, Incrementable Counter)
  : AutomationJob
{
  public async Task Run()
  {
    while (ContinueGate.IsOpen) 
      await OnePass();
  }

  async Task OnePass()
  {
    await Pass.Run();
    Counter.Increment();
  }
}