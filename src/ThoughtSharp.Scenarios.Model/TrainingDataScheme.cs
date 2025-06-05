namespace ThoughtSharp.Scenarios.Model;

public class TrainingDataScheme(TrainingMetadata Metadata)
{
  readonly TrainingMetadata Metadata = Metadata;
  readonly Dictionary<ScenariosModelNode, ConvergenceTracker> Trackers = [];

  public Counter TimesSinceSaved { get; } = new();
  public Counter Attempts { get; } = new();

  public ConvergenceTracker GetConvergenceTrackerFor(ScenariosModelNode Node)
  {
    if (!Trackers.TryGetValue(Node, out var Result))
      Trackers[Node] = Result = new(Metadata.SampleSize);

    return Result;
  }

  protected bool Equals(TrainingDataScheme Other)
  {
    return Metadata.Equals(Other.Metadata);
  }

  public override bool Equals(object? Obj)
  {
    if (Obj is null) return false;
    if (ReferenceEquals(this, Obj)) return true;
    if (Obj.GetType() != GetType()) return false;
    return Equals((TrainingDataScheme) Obj);
  }

  public override int GetHashCode()
  {
    return Metadata.GetHashCode();
  }
}