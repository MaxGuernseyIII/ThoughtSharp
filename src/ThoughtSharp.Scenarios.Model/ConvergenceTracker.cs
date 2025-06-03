namespace ThoughtSharp.Scenarios.Model;

public class ConvergenceTracker(int Length)
{
  readonly Queue<bool> Results = new();

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