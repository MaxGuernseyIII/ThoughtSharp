using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockRunnable : Runnable
{
  public RunResult Result = new()
  {
    Status = BehaviorRunStatus.NotRun
  };

  public int RunCount { get; set; }

  public Task<RunResult> Run()
  {
    RunCount++;

    return Task.FromResult(Result);
  }
}