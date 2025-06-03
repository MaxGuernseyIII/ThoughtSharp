using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;

namespace Tests.Mocks;

public class MockMindPlace : MindPlace
{
  public static MockMindPlace ForBrain(
    Brain ToReturn,
    Func<Brain, object> MakeMind)
  {
    var Result = new MockMindPlace() { MakeMind = MakeMind };
    Result.BrainsToMake.Enqueue(ToReturn);

    return Result;
  }

  public Queue<Brain> BrainsToMake { get; } = [];
  public bool FailOnLoad { get; set; }
  public required Func<Brain, object> MakeMind { get; set; }

  public Brain MakeNewBrain()
  {
    return BrainsToMake.Dequeue();
  }

  public object MakeNewMind(Brain Brain)
  {
    return MakeMind(Brain);
  }

  public List<Brain> Loaded { get; } = [];

  public void LoadSavedBrain(Brain ToLoad)
  {
    if (FailOnLoad)
      throw new("Could not load the brain data.");

    Loaded.Add(ToLoad);
  }

  public List<Brain> Saved { get; } = [];

  public void SaveBrain(Brain ToSave)
  {
    Saved.Add(ToSave);
  }
}