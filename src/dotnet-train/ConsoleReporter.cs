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

using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

class ConsoleReporter : Reporter
{
  readonly CancellationTokenSource Cancellation = new();
  readonly TrainingDataScheme Scheme;

  public ConsoleReporter(TrainingDataScheme Scheme)
  {
    this.Scheme = Scheme;
    Start();
  }

  public void Start()
  {
    Task.Run(async () =>
    {
      while (!Cancellation.Token.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(0.5));

        Print();
      }
    });
  }

  void Print()
  {
    foreach (var Node in Scheme.TrackedNodes)
    {
      var State = Scheme.GetConvergenceTrackerFor(Node);
      var Convergence = State.MeasureConvergence();

      Console.WriteLine($"{Node.Name}: {Convergence:P}");
    }
  }

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    Console.WriteLine($"Enter: {Node.Name}");
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    Console.WriteLine($"Exit: {Node.Name}");
  }

  public void Dispose()
  {
    Print();
    Cancellation.Cancel();
  }
}