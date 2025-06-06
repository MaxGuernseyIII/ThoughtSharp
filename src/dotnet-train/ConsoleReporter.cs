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

using System.CodeDom.Compiler;
using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

class ConsoleReporter : Reporter
{
  readonly CancellationTokenSource Cancellation = new();
  readonly object LockObject = new();
  readonly TrainingDataScheme Scheme;
  readonly Stack<ScenariosModelNode> Path = [];
  TrainingDataScheme? LastSchemeToPrint;

  public ConsoleReporter(TrainingDataScheme Scheme)
  {
    this.Scheme = Scheme;
    Start();
  }

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    Console.WriteLine($"Enter: {Node.Name}");
    lock(LockObject)
      Path.Push(Node);
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    lock (LockObject)
      Path.Pop();
    Console.WriteLine($"Exit: {Node.Name}");
    Console.WriteLine();
  }

  void Start()
  {
    Task.Run(async () =>
    {
      while (!Cancellation.Token.IsCancellationRequested)
      {
        try
        {
          await Task.Delay(TimeSpan.FromSeconds(1));

          Print();
        }
        catch (Exception Ex)
        {
          Console.WriteLine(Ex);
        }
      } 
    });
  }

  void Print()
  {
    lock (LockObject)
    {
      var StringWriter = new StringWriter();
      var SchemeToPrint = Path.Reverse().Aggregate(Scheme, (Current, Node) => Current.GetChildScheme(Node));
      if (!ReferenceEquals(SchemeToPrint, LastSchemeToPrint))
        Console.Clear();
      else
        Console.SetCursorPosition(0, 0);

      LastSchemeToPrint = SchemeToPrint;

      PrintScheme(SchemeToPrint, new(StringWriter));
      Console.WriteLine(StringWriter.ToString());
    }
  }

  void PrintScheme(TrainingDataScheme SchemeToPrint, IndentedTextWriter Writer)
  {
    if (SchemeToPrint.TrackedNodes.Any())
    {
      var LabelWidth = SchemeToPrint.TrackedNodes.Select(N => N.Name.Length + 3).Max();
      var Width = Math.Min(Console.WindowWidth - (10 + LabelWidth), 60);

      Writer.WriteLine($"Phase: {SchemeToPrint.Node?.Name}");
      Writer.Indent++;
      foreach (var Node in SchemeToPrint.TrackedNodes)
      {
        var State = SchemeToPrint.GetConvergenceTrackerFor(Node);
        var Convergence = State.MeasureConvergence();
        var ConvergenceBar = new string('█', (int) (Width * Convergence)).PadRight(Width, '░');
        Writer.WriteLine($"{Node.Name.PadRight(LabelWidth)} [{ConvergenceBar}]");
      }
    }

    foreach (var SubSchemeNode in SchemeToPrint.SubSchemeNodes)
    {
      var SubScheme = SchemeToPrint.GetChildScheme(SubSchemeNode);
      PrintScheme(SubScheme, Writer);
    }

    Writer.WriteLine($"{SchemeToPrint.Attempts.Value} attempts, {SchemeToPrint.TimesSinceSaved.Value} attempts since last save               ");
    Writer.Indent--;
  }

  public void Stop()
  {
    Cancellation.Cancel();
  }
}