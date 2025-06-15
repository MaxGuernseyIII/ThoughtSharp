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
using System.Collections.Immutable;
using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

class ConsoleReporter : Reporter
{
  readonly CancellationTokenSource Cancellation = new();

  readonly ScenarioNameVisitor GetScenarioName = new();
  readonly ConcurrentDictionary<ScenariosModelNode, RunResult> MostRecentFailures = [];
  readonly ConcurrentStack<ScenariosModelNode> Path = [];
  readonly Task PrintTask;
  readonly TrainingDataScheme Scheme;
  readonly TextWriter Writer = Console.Out;
  int LastLineCount;
  TrainingDataScheme? LastSchemeToPrint;

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

  void PrintScheme(TrainingDataScheme SchemeToPrint)
  {
    void WriteLine(string ToWrite)
    {
      Writer.WriteLine(ToWrite);
    }

    if (SchemeToPrint.TrackedNodes.Any())
    {
      var NodeNames = SchemeToPrint.TrackedNodes.Select(N => (Node: N, Name: N.Query(GetScenarioName)))
        .OrderBy(Pair => Pair.Name).ToImmutableArray();
      var LabelWidth = NodeNames.Select(Pair => Pair.Name.Length + 3).Max();
      var EmptyLabel = new string(' ', LabelWidth);
      var RemainingWidth = Console.WindowWidth - (10 + LabelWidth);
      var BarWidth = Math.Min(RemainingWidth, 60);

      WriteLine($"Phase: {SchemeToPrint.Node.Name}");
      foreach (var (Node, Name) in NodeNames)
      {
        var State = SchemeToPrint.GetConvergenceTrackerFor(Node);
        var Convergence = State.MeasureConvergence();
        var ConvergenceBar = new string('█', (int) (BarWidth * Convergence)).PadRight(BarWidth, '░');
        var ConvergenceThreshold = (int) (SchemeToPrint.Metadata.SuccessFraction * BarWidth);
        var IsConvergent = Convergence >= SchemeToPrint.Metadata.SuccessFraction;

        var BelowThresholdPortion = ConvergenceBar[..ConvergenceThreshold];
        var AboveThresholdPortion = ConvergenceBar[ConvergenceThreshold..];

        ClearLine();

        Writer.Write($"{Name.PadRight(LabelWidth)} [");
        Console.ForegroundColor = IsConvergent ? ConsoleColor.Green : ConsoleColor.DarkRed;
        Writer.Write(BelowThresholdPortion);
        Console.ForegroundColor = IsConvergent ? ConsoleColor.DarkGreen : ConsoleColor.Red;
        Writer.Write(AboveThresholdPortion);
        Console.ForegroundColor = ConsoleColor.White;

        WriteLine($"] ({Convergence:P})");
        if (!IsConvergent && MostRecentFailures.TryGetValue(Node, out var RecentResult))
        {
          var Content = "";
          if (RecentResult.Exception is not null)
            Content = RecentResult.Exception.Message + Environment.NewLine;
          Content += RecentResult.Output;

          var Lines = Content.Split([Environment.NewLine],
              StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Line => Line.Length < RemainingWidth ? Line : Line[..(RemainingWidth - 4)] + "...")
            .Take(3);

          //Lines = [..Lines, ..Enumerable.Repeat("", 3 - Lines.Count())];

          foreach (var Line in Lines)
          {
            ClearLine();
            WriteLine($"{EmptyLabel}  {Line[..Math.Min(RemainingWidth, Line.Length)]}");
          }
        }
        //else
        //{
        //  foreach (var _ in Enumerable.Range(0, 3))
        //  {
        //    ClearLine();
        //    WriteLine("");
        //  }
        //}
      }
    }

    foreach (var SubSchemeNode in SchemeToPrint.SubSchemeNodes)
    {
      var SubScheme = SchemeToPrint.GetChildScheme(SubSchemeNode);
      PrintScheme(SubScheme);
    }

    ClearLine();
    WriteLine(
      $"{SchemeToPrint.Attempts.Value} attempts, {SchemeToPrint.TimesSinceSaved.Value} attempts since last save");

    var LineCount = Console.GetCursorPosition().Top;

    foreach (var _ in Enumerable.Range(0, Math.Max(0, LastLineCount - LineCount)))
    {
      ClearLine();
      Writer.WriteLine();
    }

    LastLineCount = LineCount;
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
}