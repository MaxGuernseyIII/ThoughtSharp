using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public sealed class AutomationPass(
  ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps,
  Gate SaveGate,
  Saver Saver,
  Reporter Reporter,
  TrainingDataScheme Scheme)
  : Runnable
{
  readonly ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps = Steps;
  readonly Gate SaveGate = SaveGate;
  readonly Saver Saver = Saver;
  readonly Reporter Reporter = Reporter;

  public async Task<RunResult> Run()
  {
    foreach (var (Node, Runnable) in Steps)
    {
      var Result = await Runnable.Run();
      Reporter.ReportRunResult(Node, Result);
      Scheme.GetConvergenceTrackerFor(Node).RecordResult(Result.Status == BehaviorRunStatus.Success);
    }

    if (SaveGate.IsOpen)
      Saver.Save();

    return new()
    {
      Status = BehaviorRunStatus.NotRun
    };
  }

  bool Equals(AutomationPass Other)
  {
    return 
      Steps.SequenceEqual(Other.Steps) && 
      SaveGate.Equals(Other.SaveGate) && 
      Saver.Equals(Other.Saver) && 
      Reporter.Equals(Other.Reporter);
  }

  public override bool Equals(object? Obj)
  {
    return ReferenceEquals(this, Obj) || Obj is AutomationPass Other && Equals(Other);
  }

  public override int GetHashCode()
  {
    return Steps.OfType<object>().Concat([SaveGate, Saver, Reporter]).Aggregate(0, HashCode.Combine);
  }
}

