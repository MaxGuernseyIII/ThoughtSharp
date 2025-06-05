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
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Scenarios;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class AutomationLoopCreation
{
  [TestMethod]
  public void CreatesFromAssociatedBehaviorRunners()
  {
    var Reporter = new MockReporter();
    var Type1 = typeof(T1);
    var Method1 = Type1.GetMethod(nameof(T1.Method1))!;
    var Type2 = typeof(T2);
    var Method2 = Type2.GetMethod(nameof(T2.Method2))!;
    var Method3 = Type2.GetMethod(nameof(T2.Method3))!;

    var BehaviorNode1 = new BehaviorNode(Type1, Method1);
    var BehaviorNode2 = new BehaviorNode(Type1, Method2);
    var BehaviorNode3 = new BehaviorNode(Type2, Method3);
    var CapabilityNode = new CapabilityNode(Type2, [BehaviorNode2, BehaviorNode3]);

    var Model = new ScenariosModel([]);

    var MaximumAttempts = Any.Int(20, 100);
    var SampleSize = Any.Int(100, 200);
    var SuccessFraction = Any.Float;
    var TrainingMetadata = new TrainingMetadata
      {SampleSize = SampleSize, SuccessFraction = SuccessFraction, MaximumAttempts = MaximumAttempts};
    var PhaseNode = new CurriculumPhaseNode(null!, Any.Float,
      TrainingMetadata, [],
      [
        new FetchDataFromNode<ScenariosModelNode?>
        {
          VisitModel = _ => CapabilityNode
        },
        new FetchDataFromNode<ScenariosModelNode?>
        {
          VisitModel = _ => BehaviorNode3
        },
        new FetchDataFromNode<ScenariosModelNode?>
        {
          VisitModel = _ => BehaviorNode1
        }
      ]);

    var Scheme = new TrainingDataScheme(TrainingMetadata);
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    ImmutableArray<ScenariosModelNode> SourceNodes = [CapabilityNode, BehaviorNode3, BehaviorNode1];
    var ConvergenceGates = SourceNodes.GetBehaviorRunners(Pool)
      .Select(Pair => Pair.Node)
      .Select(N => Gate.ForConvergenceTrackerAndThreshold(Scheme.GetConvergenceTrackerFor(N),
        TrainingMetadata.SuccessFraction))
      .ToImmutableArray();
    var ConvergenceRule = ConvergenceGates[1..].Aggregate(ConvergenceGates[0], Gate.ForAnd);
    var Loop = Model.MakeAutomationLoopForPhase(PhaseNode, Pool, Reporter, Scheme);

    Loop.Should().BeEquivalentTo(
      new AutomationLoop(
        Model.GetTestPassFor(Pool, Reporter, [
          ..PhaseNode.IncludedTrainingScenarioNodeFinders.Select(F => Model.Query(F)).Where(R => R is not null)!
        ]),
        new AndGate(
          new CounterAndMaximumGate(Scheme.Attempts, MaximumAttempts),
          ConvergenceRule),
        ConvergenceRule,
        new CompoundIncrementable(Scheme.TimesSinceSaved, Scheme.Attempts)));
  }

  public class T1
  {
    public void Method1()
    {
    }
  }

  public class T2
  {
    public void Method2()
    {
    }

    public void Method3()
    {
    }
  }
}