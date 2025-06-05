using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

public class MockAutomationJob : AutomationJob
{
  public Func<Task> RunBehavior = () => Task.CompletedTask;

  public int RunCount { get; set; }

  public Task Run()
  {
    RunCount++;
    return RunBehavior();
  }
}