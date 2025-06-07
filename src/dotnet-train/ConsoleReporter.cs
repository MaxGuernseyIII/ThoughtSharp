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
  readonly CancellationTokenSource Cancellation = new();
  readonly ConcurrentDictionary<ScenariosModelNode, RunResult> MostRecentFailures = [];
  readonly ConcurrentStack<ScenariosModelNode> Path = [];
  readonly TrainingDataScheme Scheme;
  TrainingDataScheme? LastSchemeToPrint;

  public ConsoleReporter(TrainingDataScheme Scheme)
  {
    this.Scheme = Scheme;
    Start();
  }

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
    if (Result.Status == BehaviorRunStatus.Failure) 
      MostRecentFailures[Node] = Result;
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    Console.WriteLine($"Enter: {Node.Name}");
    Path.Push(Node);
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    Path.TryPop(out _);

    Console.WriteLine($"Exit: {Node.Name}");
    Console.WriteLine();
  }

  void Start()
  {
    Task.Run(async () =>
    {
      while (!Cancellation.Token.IsCancellationRequested)
        try
        {
          await Task.Delay(TimeSpan.FromSeconds(5));

          Print();
        }
        catch (Exception Ex)
        {
          Console.WriteLine(Ex);
        }
    });
  }

  void Print()
  {
    var SchemeToPrint = Path.Reverse().Aggregate(Scheme, (Current, Node) => Current.GetChildScheme(Node));
    //if (!ReferenceEquals(SchemeToPrint, LastSchemeToPrint))
    Console.Clear();
    //else
    //Console.SetCursorPosition(0, 0);

    //foreach (var (Node, Result) in Results.Where(R => R.Node is CurriculumPhaseNode)) 
    //  Console.WriteLine($"{Node.Name}: {Result.Status}");

    LastSchemeToPrint = SchemeToPrint;

    PrintScheme(SchemeToPrint);
  }

  void PrintScheme(TrainingDataScheme SchemeToPrint)
  {
    if (SchemeToPrint.TrackedNodes.Any())
    {
      var LabelWidth = SchemeToPrint.TrackedNodes.Select(N => N.Name.Length + 3).Max();
      var BarWidth = Math.Min(Console.WindowWidth - (10 + LabelWidth), 60);

      Console.WriteLine($"Phase: {SchemeToPrint.Node?.Name}");
      foreach (var Node in SchemeToPrint.TrackedNodes)
      {
        var State = SchemeToPrint.GetConvergenceTrackerFor(Node);
        var Convergence = State.MeasureConvergence();
        var ConvergenceBar = new string('█', (int) (BarWidth * Convergence)).PadRight(BarWidth, '░');
        var ConvergenceThreshold = (int) SchemeToPrint.Metadata.SuccessFraction * BarWidth;
        var IsConvergent = Convergence >= SchemeToPrint.Metadata.SuccessFraction;

        var BelowThresholdPortion = ConvergenceBar[..ConvergenceThreshold];
        var AboveThresholdPortion = ConvergenceBar[ConvergenceThreshold..];

        Console.Write($"{Node.Name.PadRight(LabelWidth)} [");
        Console.ForegroundColor = IsConvergent ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(BelowThresholdPortion);
        Console.ForegroundColor = IsConvergent ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
        Console.Write(AboveThresholdPortion);
        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine("]");
        if (MostRecentFailures.TryGetValue(Node, out var RecentResult))
        {
          var Content = "";
          if (RecentResult.Exception is not null)
            Content = RecentResult.Exception.Message + Environment.NewLine;
          Content += RecentResult.Output;

          var Lines = Content.Split([Environment.NewLine],
              StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Line => Line.Length < Console.WindowWidth ? Line : Line[..(Console.WindowWidth - 4)] + "...")
            .Take(3);
          Lines = Lines.Concat(Enumerable.Repeat("", 3 - Lines.Count()));

          foreach (var Line in Lines)
            Console.WriteLine(Line);
        }
      }
    }

    foreach (var SubSchemeNode in SchemeToPrint.SubSchemeNodes)
    {
      var SubScheme = SchemeToPrint.GetChildScheme(SubSchemeNode);
      PrintScheme(SubScheme);
    }

    Console.WriteLine(
      $"{SchemeToPrint.Attempts.Value} attempts, {SchemeToPrint.TimesSinceSaved.Value} attempts since last save               ");
  }

  public void Stop()
  {
    Cancellation.Cancel();
    Print();
  }
}