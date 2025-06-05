namespace ThoughtSharp.Scenarios.Model;

public class ConvergenceTracker(int Length)
{
  readonly Queue<bool> Results = new();
  readonly int Length = Length;

  protected bool Equals(ConvergenceTracker Other)
  {
    return Results.SequenceEqual(Other.Results) && Length == Other.Length;
  }

  public override bool Equals(object? Obj)
  {
    if (Obj is null) return false;
    if (ReferenceEquals(this, Obj)) return true;
    if (Obj.GetType() != GetType()) return false;
    return Equals((ConvergenceTracker) Obj);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Results, Length);
  }

  public double MeasureConvergence()
  {
    var Successes = Results.Count(B => B);
    return 1d * Successes / Length;
  }

  public void RecordResult(bool RunResult)
  {
    Results.Enqueue(RunResult);
    while (Results.Count > Length) 
      Results.Dequeue();
  }
}