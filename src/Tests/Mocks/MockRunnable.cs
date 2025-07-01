using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockRunnable : Runnable
{
  public Func<Task<RunResult>> RunBehavior = () => Task.FromResult(new RunResult()
  {
    Status = BehaviorRunStatus.NotRun,
    Transcript = new([])
  });

  public RunResult Result
  {
    set
    {
      RunBehavior = () => Task.FromResult(value);
    }
  }

  public int RunCount { get; set; }

  public Task<RunResult> Run()
  {
    RunCount++;

    return RunBehavior();
  }
}