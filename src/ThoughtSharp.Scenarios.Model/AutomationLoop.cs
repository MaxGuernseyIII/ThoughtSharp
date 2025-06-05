namespace ThoughtSharp.Scenarios.Model;

public sealed record AutomationLoop(Runnable Pass, Gate ContinueGate, Gate Success, Incrementable Counter)
  : Runnable
{
  public async Task<RunResult> Run()
  {
    while (ContinueGate.IsOpen) 
      await OnePass();

    return new() {Status = BehaviorRunStatus.NotRun};
  }

  async Task OnePass()
  {
    await Pass.Run();
    Counter.Increment();
  }
}