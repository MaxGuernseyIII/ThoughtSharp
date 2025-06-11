// MIT License
// 
// Copyright (c) 2025-2025 Hexagon Software LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Concurrent;
using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

class ConsoleReporter : Reporter
{
  class ScenarioNameVisitor : ScenariosModelNodeVisitor<string>
  {
    public string Visit(ScenariosModel Model)
    {
      return Model.Name;
    }

    public string Visit(DirectoryNode Directory)
    {
      return Directory.Name;
    }

    public string Visit(CurriculumNode Curriculum)
    {
      return Curriculum.Name;
    }

    public string Visit(CapabilityNode Capability)
    {
      return Capability.Name;
    }

    public string Visit(MindPlaceNode MindPlace)
    {
      return MindPlace.Name;
    }

    public string Visit(BehaviorNode Behavior)
    {
      return $"{Behavior.HostType.Name}.{Behavior.Name}";
    }

    public string Visit(CurriculumPhaseNode CurriculumPhase)
    {
      return CurriculumPhase.Name;
    }
  }

  readonly CancellationTokenSource Cancellation = new();
  readonly ConcurrentDictionary<ScenariosModelNode, RunResult> MostRecentFailures = [];
  readonly ConcurrentStack<ScenariosModelNode> Path = [];
  readonly TrainingDataScheme Scheme;
  readonly TextWriter Writer = Console.Out;
  TrainingDataScheme? LastSchemeToPrint;
  readonly Task PrintTask;

  public ConsoleReporter(TrainingDataScheme Scheme)
  {
    this.Scheme = Scheme;
    PrintTask = Start();
  }

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
    if (Result.Status == BehaviorRunStatus.Failure) 
      MostRecentFailures[Node] = Result;
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    Writer.WriteLine($"Enter: {Node.Name}");
    Path.Push(Node);
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    Path.TryPop(out _);

    Writer.WriteLine($"Exit: {Node.Name}");
    Writer.WriteLine();
  }

  Task Start()
  {
    return Task.Run(async () =>
    {
      while (!Cancellation.Token.IsCancellationRequested)
        try
        {
          await Task.Delay(TimeSpan.FromSeconds(.2));

          Print();
        }
        catch (Exception Ex)
        {
          Writer.WriteLine(Ex);
        }
    });
  }

  void Print()
  {
    var SchemeToPrint = Path.Reverse().Aggregate(Scheme, (Current, Node) => Current.GetChildScheme(Node));
    if (!ReferenceEquals(SchemeToPrint, LastSchemeToPrint))
      Console.Clear();
    else
      Console.SetCursorPosition(0, 0);
    LastSchemeToPrint = SchemeToPrint;

    PrintScheme(SchemeToPrint);
  }

  readonly ScenarioNameVisitor GetScenarioName = new();
  int LastLineCount = 0;

  void PrintScheme(TrainingDataScheme SchemeToPrint)
  {
    var LineCount = 0;
    if (SchemeToPrint.TrackedNodes.Any())
    {
      var LabelWidth = SchemeToPrint.TrackedNodes.Select(N => N.Query(GetScenarioName).Length + 3).Max();
      var EmptyLabel = new string(' ', LabelWidth);
      var RemainingWidth = Console.WindowWidth - (10 + LabelWidth);
      var BarWidth = Math.Min(RemainingWidth, 60);

      Writer.WriteLine($"Phase: {SchemeToPrint.Node.Name}");
      LineCount++;
      foreach (var Node in SchemeToPrint.TrackedNodes)
      {
        var State = SchemeToPrint.GetConvergenceTrackerFor(Node);
        var Convergence = State.MeasureConvergence();
        var ConvergenceBar = new string('█', (int) (BarWidth * Convergence)).PadRight(BarWidth, '░');
        var ConvergenceThreshold = (int) (SchemeToPrint.Metadata.SuccessFraction * BarWidth);
        var IsConvergent = Convergence >= SchemeToPrint.Metadata.SuccessFraction;

        var BelowThresholdPortion = ConvergenceBar[..ConvergenceThreshold];
        var AboveThresholdPortion = ConvergenceBar[ConvergenceThreshold..];

        ClearLine();

        Writer.Write($"{Node.Query(GetScenarioName).PadRight(LabelWidth)} [");
        Console.ForegroundColor = IsConvergent ? ConsoleColor.Green : ConsoleColor.DarkRed;
        Writer.Write(BelowThresholdPortion);
        Console.ForegroundColor = IsConvergent ? ConsoleColor.DarkGreen : ConsoleColor.Red;
        Writer.Write(AboveThresholdPortion);
        Console.ForegroundColor = ConsoleColor.White;

        Writer.WriteLine($"] ({Convergence:P})");
        LineCount++;
        if (!IsConvergent && MostRecentFailures.TryGetValue(Node, out var RecentResult))
        {
          var Content = "";
          if (RecentResult.Exception is not null)
            Content = RecentResult.Exception.Message + Environment.NewLine;
          Content += RecentResult.Output;

          var Lines = Content.Split([Environment.NewLine],
              StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Line => Line.Length < Console.WindowWidth ? Line : Line[..(Console.WindowWidth - 4)] + "...")
            .Take(3);

          foreach (var Line in Lines)
          {
            ClearLine();
            Writer.WriteLine($"{EmptyLabel}{Line[..Math.Min(RemainingWidth, Line.Length)]}");
            LineCount++;
          }
        }

        foreach (var _ in Enumerable.Range(0, Math.Max(0, LastLineCount - LineCount)))
        {
          ClearLine();
          Writer.WriteLine();
        }

        LastLineCount = LineCount;
      }
    }

    foreach (var SubSchemeNode in SchemeToPrint.SubSchemeNodes)
    {
      var SubScheme = SchemeToPrint.GetChildScheme(SubSchemeNode);
      PrintScheme(SubScheme);
    }

    ClearLine();
    Writer.WriteLine(
      $"{SchemeToPrint.Attempts.Value} attempts, {SchemeToPrint.TimesSinceSaved.Value} attempts since last save");
  }

  void ClearLine()
  {
    var Blank = new string(' ', Console.BufferWidth);
    var Top = Console.CursorTop;
    Console.SetCursorPosition(0, Top);
    Writer.Write(Blank);
    Console.SetCursorPosition(0, Top);
  }

  public async Task Stop()
  {
    await Cancellation.CancelAsync();
    await PrintTask;
    Print();
  }
}