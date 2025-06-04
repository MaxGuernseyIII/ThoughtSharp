using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

public class MockAutomationJob : AutomationJob
{
  public int RunCount { get; set; }

  public Task Run()
  {
    RunCount++;
    return Task.CompletedTask;
  }
}