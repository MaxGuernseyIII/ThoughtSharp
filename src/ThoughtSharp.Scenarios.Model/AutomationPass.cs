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

using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public sealed class AutomationPass(
  ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps,
  Gate SaveGate,
  Saver Saver,
  TrainingDataScheme Scheme,
  Reporter Reporter)
  : Runnable
{
  readonly Gate SaveGate = SaveGate;
  readonly Saver Saver = Saver;
  readonly ImmutableArray<(ScenariosModelNode Node, Runnable Runnable)> Steps = Steps;
  readonly TrainingDataScheme Scheme = Scheme;

  public async Task<RunResult> Run()
  {
    var AnyFailed = false;

    foreach (var (Node, Runnable) in Steps)
    {
      var Result = await Runnable.Run();
      Scheme.Reporter.ReportRunResult(Node, Result);
      var WasSuccessful = Result.Status == BehaviorRunStatus.Success;
      Scheme.GetConvergenceTrackerFor(Node).RecordResult(WasSuccessful);
      AnyFailed = AnyFailed || Result.Status == BehaviorRunStatus.Failure;
    }

    if (SaveGate.IsOpen)
      Saver.Save();

    return new()
    {
      Status = AnyFailed ? BehaviorRunStatus.Failure : BehaviorRunStatus.Success
    };
  }

  bool Equals(AutomationPass Other)
  {
    return
      Steps.SequenceEqual(Other.Steps) &&
      SaveGate.Equals(Other.SaveGate) &&
      Saver.Equals(Other.Saver) &&
      Equals(Scheme, Other.Scheme);
  }

  public override bool Equals(object? Obj)
  {
    return ReferenceEquals(this, Obj) || (Obj is AutomationPass Other && Equals(Other));
  }

  public override int GetHashCode()
  {
    return Steps.OfType<object>().Concat([SaveGate, Saver, Scheme]).Aggregate(0, HashCode.Combine);
  }
}