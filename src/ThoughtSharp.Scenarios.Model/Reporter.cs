namespace ThoughtSharp.Scenarios.Model;

public interface Reporter
{
  void ReportRunResult(ScenariosModelNode Node, RunResult Result);
}