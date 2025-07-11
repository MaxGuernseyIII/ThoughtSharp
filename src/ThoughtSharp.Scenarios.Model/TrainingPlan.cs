﻿// MIT License
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

public record TrainingPlan(
  ScenariosModelNode PlanNode, 
  ImmutableArray<Runnable> SubJobs, 
  TrainingDataScheme Scheme,
  Reporter Reporter) : Runnable
{
  public async Task<RunResult> Run()
  {
    try
    {
      Reporter.ReportEnter(PlanNode);

      foreach (var SubJob in SubJobs)
        if ((await SubJob.Run()).Status == BehaviorRunStatus.Failure)
          return new()
          {
            Status = BehaviorRunStatus.Failure,
            Transcript = new([]),
          };

      return new()
      {
        Status = BehaviorRunStatus.Success,
        Transcript = new([])
      };
    }
    finally
    {
      Reporter.ReportExit(PlanNode);
    }
  }

  public virtual bool Equals(TrainingPlan? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return PlanNode.Equals(Other.PlanNode) && SubJobs.SequenceEqual(Other.SubJobs) && Equals(Scheme, Other.Scheme);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(PlanNode, SubJobs);
  }
}