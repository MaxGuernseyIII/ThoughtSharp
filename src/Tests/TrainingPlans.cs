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
using FluentAssertions.Execution;
using Tests.Mocks;
using ThoughtSharp.Scenarios.Model;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests;

[TestClass]
public class TrainingPlans
{
  List<MockRunnable> ActuallyRunJobs = null!;
  MockNode PlanNode = null!;
  MockReporter Reporter = null!;

  [TestInitialize]
  public void SetUp()
  {
    PlanNode = new();
    ActuallyRunJobs = new();
    Reporter = new();
  }

  [TestMethod]
  public async Task RunSuccessfulItemsInOrder()
  {
    var SubJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success);
    var Plan = GivenTrainingPlanForJobs(SubJobs);

    await WhenRunPlan(Plan);

    ThenRunJobsWere(SubJobs);
  }

  [TestMethod]
  public async Task ReturnsSuccessIfAllJobsSuccessful()
  {
    var SubJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success);
    var Plan = GivenTrainingPlanForJobs(SubJobs);

    var Result = await WhenRunPlan(Plan);

    ThenResultIs(Result, BehaviorRunStatus.Success);
  }

  [TestMethod]
  public async Task DoesNotRunAnythingAfterFailedSubJob()
  {
    var SuccessfulJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success);
    var FailedJob = GivenSelfTrackingRunnable(BehaviorRunStatus.Failure);
    var OtherJobs = GivenAnySelfTrackingRunnables(Any.EnumValue<BehaviorRunStatus>());

    var Plan = GivenTrainingPlanForJobs([.. SuccessfulJobs, FailedJob, .. OtherJobs]);

    await WhenRunPlan(Plan);

    ThenRunJobsWere([.. SuccessfulJobs, FailedJob]);
  }

  [TestMethod]
  public async Task ReportsFailureIfAnythingFails()
  {
    var SuccessfulJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success, 0, 2);
    var FailedJob = GivenSelfTrackingRunnable(BehaviorRunStatus.Failure);
    var OtherJobs = GivenAnySelfTrackingRunnables(BehaviorRunStatus.Success, 0, 2);

    var Plan = GivenTrainingPlanForJobs([.. SuccessfulJobs, FailedJob, .. OtherJobs]);

    var Result = await WhenRunPlan(Plan);

    ThenResultIs(Result, BehaviorRunStatus.Failure);
  }

  [TestMethod]
  public async Task ReportsStart()
  {
    var EnteredNodes = new List<ScenariosModelNode>();
    Reporter.ReportEnterBehavior = EnteredNodes.Add;
    var Plan = GivenTrainingPlanForJobs(GivenAnySelfTrackingRunnables(Any.EnumValue<BehaviorRunStatus>()));

    await Plan.Run();

    EnteredNodes.Should().BeEquivalentTo([PlanNode]);
  }

  [TestMethod]
  public async Task NeverReportsEntryAfterItemRun()
  {
    var Runnable = new MockRunnable
    {
      RunBehavior = delegate
      {
        Reporter.ReportEnterBehavior = delegate { Assert.Fail("This cannot happen after a sub-job is run."); };
        return Task.FromResult(new RunResult {Status = Any.EnumValue<BehaviorRunStatus>()});
      }
    };
    var Plan = GivenTrainingPlanForJobs([Runnable]);

    await Plan.Run();

    ThenRanToCompletion();
  }

  [TestMethod]
  public async Task ReportsEnd()
  {
    var ExitedNodes = new List<ScenariosModelNode>();
    Reporter.ReportExitBehavior = ExitedNodes.Add;
    var Plan = GivenTrainingPlanForJobs(GivenAnySelfTrackingRunnables(Any.EnumValue<BehaviorRunStatus>()));

    await Plan.Run();

    ExitedNodes.Should().BeEquivalentTo([PlanNode]);
  }

  [TestMethod]
  public async Task NeverRunsTaskAfterReportingExit()
  {
    var Runnable = new MockRunnable();
    Reporter.ReportExitBehavior = delegate
    {
      Runnable.RunBehavior = () => throw new AssertionFailedException("Cannot run tasks after reporting exit.");
    };
    var Plan = GivenTrainingPlanForJobs([Runnable]);

    await Plan.Run();

    ThenRanToCompletion();
  }

  TrainingPlan GivenTrainingPlanForJobs(IReadOnlyList<MockRunnable> SubJobs)
  {
    return new(PlanNode, [..SubJobs], Reporter);
  }

  IReadOnlyList<MockRunnable> GivenAnySelfTrackingRunnables(BehaviorRunStatus RunStatus, int Minimum = 1,
    int Maximum = 4)
  {
    return Any.ListOf(() => GivenSelfTrackingRunnable(RunStatus), Minimum, Maximum);
  }

  MockRunnable GivenSelfTrackingRunnable(BehaviorRunStatus RunStatus)
  {
    var Job = new MockRunnable();
    Job.RunBehavior = () =>
    {
      ActuallyRunJobs.Add(Job);
      return Task.FromResult(new RunResult {Status = RunStatus});
    };
    return Job;
  }

  static Task<RunResult> WhenRunPlan(TrainingPlan Plan)
  {
    return Plan.Run();
  }

  void ThenRunJobsWere(IReadOnlyList<MockRunnable> SubJobs)
  {
    ActuallyRunJobs.Should().BeEquivalentTo(SubJobs, O => O.WithStrictOrdering());
  }

  static void ThenResultIs(RunResult Result, BehaviorRunStatus Expected)
  {
    Result.Status.Should().Be(Expected);
  }

  void ThenRanToCompletion()
  {
  }
}