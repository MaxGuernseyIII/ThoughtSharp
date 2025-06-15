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

public static class ScenariosModelNodeExtensions
{
  public static Runnable BuildTrainingPlanFor(
    this ScenariosModel This,
    ScenariosModelNode PhaseNode,
    MindPool Pool,
    TrainingDataScheme ParentScheme,
    Reporter Reporter)
  {
    var Scheme = ParentScheme.GetChildScheme(PhaseNode);

    var ChildPhases = PhaseNode.GetChildPhases();
    if (ChildPhases.Any())
      return new TrainingPlan(
        PhaseNode,
        [..ChildPhases.Select(P => This.BuildTrainingPlanFor(P, Pool, Scheme, Reporter))],
        Scheme,
        Reporter);

    return new TrainingPlan(
      PhaseNode,
      [This.MakeAutomationLoopForPhase(PhaseNode, Pool, Scheme, Reporter)],
      Scheme,
      Reporter);
  }

  public static Runnable MakeAutomationLoopForPhase(
    this ScenariosModel This,
    ScenariosModelNode PhaseNode,
    MindPool Pool,
    TrainingDataScheme TrainingDataScheme,
    Reporter Reporter)
  {
    var IncludedScenariosFinders = PhaseNode.Query(new TargetedVisitor<ScenariosModelNodeVisitor<ScenariosModelNode?>>
    {
      VisitCurriculumPhase = Phase => Phase.IncludedTrainingScenarioNodeFinders
    });

    var Behaviors = IncludedScenariosFinders.Select(This.Query).Where(B => B is not null).OfType<ScenariosModelNode>()
      .ToImmutableArray();

    var Pass = This.GetTestPassFor(
      Pool, 
      TrainingDataScheme, 
      Gate.ForCounterAndMinimum(TrainingDataScheme.TimesSinceSaved, 100), 
      new ResetCounterSaver(Pool, TrainingDataScheme.TimesSinceSaved), 
      Reporter,
      TrainingDataScheme.Attempts,
      Behaviors);
    var Nodes = Behaviors.GetBehaviorRunners(Pool).Select(R => R.Node).Select(N =>
      Gate.ForConvergenceTrackerAndThreshold(TrainingDataScheme.GetConvergenceTrackerFor(N), TrainingDataScheme.Metadata.SuccessFraction));

    var SuccessGate = Nodes.Aggregate(Gate.AlwaysOpen, Gate.ForAnd);
    return new AutomationLoop(
      Pass,
      Gate.ForAnd(
        Gate.ForCounterAndMaximum(TrainingDataScheme.Attempts, TrainingDataScheme.Metadata.MaximumAttempts), Gate.NotGate(SuccessGate)),
        SuccessGate,
        new CompoundIncrementable(TrainingDataScheme.Attempts, TrainingDataScheme.TimesSinceSaved)
    );
  }

  public static Runnable GetTestPassFor(this ScenariosModel This,
    MindPool Pool,
    TrainingDataScheme Scheme,
    Gate SaveGate,
    Saver Saver,
    Reporter Reporter,
    Incrementable TrialCounter,
    params ImmutableArray<ScenariosModelNode> Nodes)
  {
    return new AutomationPass([..Nodes.GetBehaviorRunners(Pool).Select(T => T with
    {
      Runner = new DynamicWeightedRunnable(
        T.Runner, 
        Scheme.Metadata.MinimumDynamicWeight, 
        Scheme.Metadata.MaxinimumDynamicWeight, 
        Scheme.GetConvergenceTrackerFor(T.Node), 
        Scheme.Metadata.SuccessFraction, 
        TrialCounter)
    })], SaveGate, Saver, Scheme, Reporter);
  }

  public static IEnumerable<CurriculumPhaseNode> GetChildPhases(this ScenariosModelNode Node)
  {
    return Node.ChildNodes.OfType<CurriculumPhaseNode>().OrderBy(N => N.PhaseNumber).ToImmutableArray();
  }

  public static IEnumerable<(ScenariosModelNode Node, Runnable Runner)> GetBehaviorRunners(
    this ScenariosModelNode Node, MindPool Pool)
  {
    return Node.Query(new CrawlingVisitor<(ScenariosModelNode Node, Runnable Runner)>
      {VisitBehavior = VisitBehavior});

    IEnumerable<(ScenariosModelNode Node, Runnable Runner)> VisitBehavior(BehaviorNode BehaviorNode)
    {
      yield return (BehaviorNode, new BehaviorRunner(Pool, BehaviorNode.HostType, BehaviorNode.Method));
    }
  }

  public static IEnumerable<(ScenariosModelNode Node, Runnable Runner)> GetBehaviorRunners(
    this IEnumerable<ScenariosModelNode> Nodes,
    MindPool Pool)
  {
    return Nodes.SelectMany(N => N.GetBehaviorRunners(Pool)).Distinct();
  }

  public static IEnumerable<ScenariosModelNode> GetStepsAlongPath(this ScenariosModelNode Node,
    params ImmutableArray<string?> Steps)
  {
    var Current = Node;

    foreach (var Step in Steps)
    {
      yield return Current;
      Current = Current.ChildNodes.Single(N => N.Name == Step);
    }

    yield return Current;
  }

  public static ScenariosModelNode GetNodeAtEndOfPath(this ScenariosModelNode Node,
    params ImmutableArray<string?> Steps)
  {
    return Node.GetStepsAlongPath(Steps).Last();
  }
}