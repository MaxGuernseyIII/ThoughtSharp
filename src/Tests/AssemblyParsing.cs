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
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Tests.SampleScenarios;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class AssemblyParsing
{
  static readonly string RootNamespace = typeof(Anchor).Namespace!;

  ScenariosModel Model = null!;
  Assembly LoadedAssembly = null!;

  [TestInitialize]
  public void SetUp()
  {
    Model = null!;
    LoadedAssembly = typeof(Anchor).Assembly;
  }

  [TestMethod]
  public void ModelProperties()
  {
    WhenParseAssembly();

    ThenModelNameIs("");
    ThenModelTypeIs(NodeType.Model);
  }

  [TestMethod]
  public void FindsDirectories()
  {
    WhenParseAssembly();

    ThenStructureContainsDirectory(
      RootNamespace,
      nameof(FizzBuzzTraining));
  }

  [TestMethod]
  public void FindsCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculum(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan));
  }

  [TestMethod]
  public void FindsCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Solution));
  }

  [TestMethod]
  public void FindsBehaviors()
  {
    WhenParseAssembly();

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Fizz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Buzz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.FizzBuzz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.WriteValue));
  }

  [TestMethod]
  public void FindsCurriculumPhases()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.InitialSteps));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnBuzz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizzBuzz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnWriting));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FinalTraining));
  }

  [TestMethod]
  public void IncludesCapabilitiesInPhases()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.InitialSteps)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations)
        ]
      ]);
  }

  [TestMethod]
  public void MultipleInclusionsAllowed()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FinalTraining)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
        ],
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Solution)
        ]
      ]);
  }

  [TestMethod]
  public void IncludesSpecificBehaviorsInPhases()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizz)],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Fizz)
        ]
      ]);

    ThenIncludedNodesForPhaseIs(
      [RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnBuzz)],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Buzz)
        ]
      ]);
  }

  [TestMethod]
  public void DoesNotFindStaticBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.StaticBehavior));
  }

  [TestMethod]
  public void DoesNotFindNonPublicBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.InternalBehavior));
  }

  [TestMethod]
  public void DoesNotFindNonVoidBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.IntBehavior));
  }

  [TestMethod]
  public void DoesNotFindMethodsWithoutBehaviorAttribute()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.NonBehaviorMethod));
  }

  [TestMethod]
  public void FindsAsyncTaskBehaviors()
  {
    WhenParseAssembly();

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.AsyncBehavior));
  }

  [TestMethod]
  public void FindsMindPlace()
  {
    WhenParseAssembly();

    ThenStructureContainsMindPlace(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzMindPlace));
  }

  [TestMethod]
  public void FindsNestedCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability1));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  [TestMethod]
  public void FindsRootCapabilitiesAndCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsDirectory(
      RootNamespace);

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryRootCapability));

    ThenStructureContainsCurriculum(
      RootNamespace,
      nameof(ArbitraryRootCurriculum));
  }

  [TestMethod]
  public void DoesNotFindClassesWithoutMeaningfulContent()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(Anchor));
  }

  void WhenParseAssembly()
  {
    Model =  new AssemblyParser().Parse(LoadedAssembly);
  }

  void ThenStructureContainsBehavior(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Behavior, Path);
  }

  void ThenStructureContainsCapability(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Capability, Path);
  }

  void ThenStructureContainsCurriculumPhase(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.CurriculumPhase, Path);
  }

  void ThenStructureContainsCurriculum(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Curriculum, Path);
  }

  void ThenStructureHasNodeOfTypeAtPath(NodeType NodeType, params ImmutableArray<string?> Path)
  {
    var (Parent, Name) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().ContainSingle(HasNameAndType(Name, NodeType));
  }

  void ThenStructureContainsDirectory(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Directory, Path);
  }

  void ThenModelNameIs(string Expected)
  {
    Model.Name.Should().Be(Expected);
  }

  void ThenStructureContainsMindPlace(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.MindPlace, Path);
  }

  void ThenModelTypeIs(NodeType Expected)
  {
    GetNodeType(Model).Should().Be(Expected);
  }

  static NodeType GetNodeType(ScenariosModelNode Node)
  {
    return Node.Query(new GetNodeTypeVisitor());
  }

  static Expression<Func<ScenariosModelNode, bool>> HasNameAndType(string? Name, NodeType NodeType)
  {
    return I => I.Name == Name && GetNodeType(I) == NodeType;
  }

  (ScenariosModelNode Parent, string? FinalName) GetParentNodeAndFinalNodeName(ImmutableArray<string?> Path)
  {
    return (Parent: GetNodeAtPath(Path[..^1]), FinalName: Path[^1]);
  }

  ScenariosModelNode GetNodeAtPath(ImmutableArray<string?> Path)
  {
    return Path.Aggregate((ScenariosModelNode)Model, (Predecessor, Name) => Predecessor.ChildNodes.Single(N => N.Name == Name));
  }

  void ThenStructureDoesNotContainNode(params ImmutableArray<string?> Path)
  {
    var (Parent, FinalName) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().NotContain(I => I.Name == FinalName);
  }

  void ThenIncludedNodesForPhaseIs(ImmutableArray<string?> Path, ImmutableArray<ImmutableArray<string?>> ExpectedPaths)
  {
    var Node = GetNodeAtPath(Path);
    var ExpectedNodes = ExpectedPaths.Select(GetNodeAtPath).ToImmutableArray();

    var ActualNodes = Node.Query(new FetchIncludedTrainingScenarios(Model));
    ActualNodes.Should().BeEquivalentTo(ExpectedNodes);
  }

  public enum NodeType
  {
    Model,
    Directory,
    Curriculum,
    Capability,
    MindPlace,
    Behavior,
    CurriculumPhase
  }

  class GetNodeTypeVisitor : ScenariosModelNodeVisitor<NodeType>
  {
    public NodeType Visit(ScenariosModel Model)
    {
      return NodeType.Model;
    }

    public NodeType Visit(DirectoryNode Directory)
    {
      return NodeType.Directory;
    }

    public NodeType Visit(CurriculumNode Curriculum)
    {
      return NodeType.Curriculum;
    }

    public NodeType Visit(CapabilityNode Capability)
    {
      return NodeType.Capability;
    }

    public NodeType Visit(MindPlaceNode MindPlace)
    {
      return NodeType.MindPlace;
    }

    public NodeType Visit(BehaviorNode Behavior)
    {
      return NodeType.Behavior;
    }

    public NodeType Visit(CurriculumPhaseNode CurriculumPhase)
    {
      return NodeType.CurriculumPhase;
    }
  }

  class FetchIncludedTrainingScenarios(ScenariosModel Model) : FetchDataFromNode
  {
    public override ImmutableArray<ScenariosModelNode?> Visit(CurriculumPhaseNode CurriculumPhase)
    {
      return [..CurriculumPhase.IncludedTrainingScenarioNodeFinders.Select(Model.Query)];
    }
  }

  abstract class FetchDataFromNode : ScenariosModelNodeVisitor<ImmutableArray<ScenariosModelNode?>>
  {
    public virtual ImmutableArray<ScenariosModelNode?> Visit(ScenariosModel _)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(DirectoryNode _)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(CurriculumNode Curriculum)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(CapabilityNode Capability)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(MindPlaceNode MindPlace)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(BehaviorNode Behavior)
    {
      throw new AssertFailedException("Should not get here.");
    }

    public virtual ImmutableArray<ScenariosModelNode?> Visit(CurriculumPhaseNode CurriculumPhase)
    {
      throw new AssertFailedException("Should not get here.");
    }
  }
}