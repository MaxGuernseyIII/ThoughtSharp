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
          await Task.Delay(TimeSpan.FromSeconds(5));

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
      PrintScheme(SchemeToPrint, new(StringWriter));
      Console.WriteLine(StringWriter.ToString());
    }
  }

  void PrintScheme(TrainingDataScheme SchemeToPrint, IndentedTextWriter Writer)
  {
    Writer.WriteLine($"Phase: {SchemeToPrint.Node?.Name}");
    Writer.Indent++;
    Writer.WriteLine($"{SchemeToPrint.Attempts.Value} attempts");
    Writer.WriteLine($"{SchemeToPrint.TimesSinceSaved.Value} attempts since last save");
    foreach (var Node in SchemeToPrint.TrackedNodes)
    {
      var State = SchemeToPrint.GetConvergenceTrackerFor(Node);
      var Convergence = State.MeasureConvergence();
      Writer.WriteLine($"{Node.Name} ({Node.GetType()}): {Convergence:P}");
    }

    foreach (var SubSchemeNode in SchemeToPrint.SubSchemeNodes)
    {
      var SubScheme = SchemeToPrint.GetChildScheme(SubSchemeNode);
      PrintScheme(SubScheme, Writer);
    }
    Writer.Indent--;
  }

  public void Stop()
  {
    Cancellation.Cancel();
  }
}