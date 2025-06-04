namespace ThoughtSharp.Scenarios.Model;

public class AutomationLoop : AutomationJob
{
  readonly AutomationJob Pass;
  readonly Gate ContinueGate;
  readonly Incrementable Counter;

  internal AutomationLoop(AutomationJob Pass, Gate ContinueGate, Incrementable Counter)
  {
    this.Pass = Pass;
    this.ContinueGate = ContinueGate;
    this.Counter = Counter;
  }

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