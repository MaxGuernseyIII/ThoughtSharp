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
public class TrainingPlanConstruction
{
  BehaviorNode BehaviorNode1 = null!;
  BehaviorNode BehaviorNode2 = null!;
  BehaviorNode BehaviorNode3 = null!;
  ImmutableArray<ScenariosModelNode> BehaviorNodes;
  ScenariosModel Model = null!;
  MindPool Pool = null!;
  MockReporter Reporter = null!;
  TrainingDataScheme Scheme = null!;
  MockNode SchemeNode;

  [TestInitialize]
  public void SetUp()
  {
    Model = new([]);
    Pool = new(ImmutableDictionary<Type, MindPlace>.Empty);
    var Type1 = typeof(Host1);
    var Type2 = typeof(Host2);
    BehaviorNode1 = new(Type1, Type1.GetMethod(nameof(Host1.Method1))!);
    BehaviorNode2 = new(Type2, Type2.GetMethod(nameof(Host2.Method2))!);
    BehaviorNode3 = new(Type2, Type2.GetMethod(nameof(Host2.Method3))!);
    BehaviorNodes = [BehaviorNode1, BehaviorNode2, BehaviorNode3];
    Reporter = new();
    SchemeNode = new();
    Scheme = new(SchemeNode, Any.TrainingMetadata(), Reporter);
  }

  [TestMethod]
  public void CurriculumPhaseConvertsToConcretePlan()
  {
    var PlanNode = GivenCurriculumPhaseNode([], BehaviorNodes);

    var Plan = Model.BuildTrainingPlanFor(PlanNode, Pool, Scheme, Reporter);

    Plan.Should().BeEquivalentTo(
      new TrainingPlan(PlanNode,
        [Model.MakeAutomationLoopForPhase(PlanNode, Pool, new(SchemeNode, PlanNode.TrainingMetadata, Reporter), Reporter)],
        Scheme.GetChildScheme(PlanNode)));
  }

  [TestMethod]
  public void CurriculumPhaseWithChildPhasesConvertsToAbstractPlan()
  {
    var ChildPlan1 = GivenCurriculumPhaseNode([], [BehaviorNode1, BehaviorNode2]);
    var ChildPlan2 = GivenCurriculumPhaseNode([], [BehaviorNode3]);
    var PlanNode = GivenCurriculumPhaseNode([
      ChildPlan1,
      ChildPlan2
    ], []);

    var Plan = Model.BuildTrainingPlanFor(PlanNode, Pool, Scheme, Reporter);

    Plan.Should().BeEquivalentTo(
      new TrainingPlan(PlanNode,
        [
          ..PlanNode.GetChildPhases().Select(Child => Model.BuildTrainingPlanFor(Child, Pool, Scheme.GetChildScheme(PlanNode), Reporter))
        ],
        Scheme.GetChildScheme(PlanNode)));
  }

  CurriculumPhaseNode GivenCurriculumPhaseNode(
    ImmutableArray<ScenariosModelNode> Children,
    ImmutableArray<ScenariosModelNode> IncludedBehaviors)
  {
    return new CurriculumPhaseNode(null!, Any.Float, Any.TrainingMetadata(), Children,
    [
      ..IncludedBehaviors.Select(N => new FetchDataFromNode<ScenariosModelNode?>
      {
        VisitModel = _ => N
      })
    ]);
  }

  public class Host1
  {
    public void Method1()
    {
    }
  }

  public class Host2
  {
    public void Method2()
    {
    }

    public void Method3()
    {
    }
  }
}