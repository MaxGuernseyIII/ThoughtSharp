using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

public class MockSaver : Saver
{
  public int SaveCount { get; set; }

  public void Save()
  {
    SaveCount++;
  }
}