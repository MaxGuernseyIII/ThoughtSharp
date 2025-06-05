namespace ThoughtSharp.Scenarios.Model;

public class TrainingDataScheme
{
  public Counter TimesSinceSaved { get; } = new();
  public Counter Attempts { get; } = new();
}