namespace ThoughtSharp.Scenarios.Model;

public interface Reporter
{
  void Update();
  void ReportRunResult(ScenariosModelNode Node, RunResult Result);
}