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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class TrainingPlans
{
  MockNode PlanNode = null!;
  List<MockRunnable> ActuallyRunJobs = null!;

  [TestInitialize]
  public void SetUp()
  {
    PlanNode = new();
    ActuallyRunJobs = new();
  }

  [TestMethod]
  public async Task RunSuccessfulItemsInOrder()
  {
    var SubJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success);
    var Plan = GivenTrainingPlanForJobs(SubJobs);

    await WhenRunPlan(Plan);

    ThenRunJobsWere(SubJobs);
  }

  void ThenRunJobsWere(IReadOnlyList<MockRunnable> SubJobs)
  {
    ActuallyRunJobs.Should().BeEquivalentTo(SubJobs, O => O.WithStrictOrdering());
  }

  TrainingPlan GivenTrainingPlanForJobs(IReadOnlyList<MockRunnable> SubJobs)
  {
    return new(PlanNode, [..SubJobs]);
  }

  IReadOnlyList<MockRunnable> GivenAnySelfTrackingRunnables(BehaviorRunStatus RunStatus)
  {
    return Any.ListOf(() => GivenSelfTrackingRunnable(RunStatus), 1, 4);
  }

  MockRunnable GivenSelfTrackingRunnable(BehaviorRunStatus RunStatus)
  {
    var Job = new MockRunnable();
    Job.RunBehavior = () =>
    {
      ActuallyRunJobs.Add(Job);
      return Task.FromResult(new RunResult() {Status = RunStatus});
    };
    return Job;
  }

  static Task<RunResult> WhenRunPlan(TrainingPlan Plan)
  {
    return Plan.Run();
  }
}