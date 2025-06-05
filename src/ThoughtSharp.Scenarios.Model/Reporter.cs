namespace ThoughtSharp.Scenarios.Model;

public interface Reporter : IDisposable
{
  void ReportRunResult(ScenariosModelNode Node, RunResult Result);
  void ReportEnter(ScenariosModelNode Node);
  void ReportExit(ScenariosModelNode Node);
}