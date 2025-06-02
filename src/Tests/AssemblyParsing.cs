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
      typeof(FizzBuzzTraining).Namespace,
      nameof(FizzBuzzTraining));
  }

  [TestMethod]
  public void FindsCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculum(
      typeof(FizzBuzzTraining).Namespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan));
  }

  [TestMethod]
  public void FindsCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      typeof(FizzBuzzTraining).Namespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations));

    ThenStructureContainsCapability(
      typeof(FizzBuzzTraining).Namespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Solution));
  }

  [TestMethod]
  public void FindsMindPlace()
  {
    WhenParseAssembly();

    ThenStructureContainsMindPlace(
      typeof(FizzBuzzTraining).Namespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzMindPlace));
  }

  [TestMethod]
  public void FindsNestedCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      typeof(ArbitraryDirectory).Namespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability));

    ThenStructureContainsCapability(
      typeof(ArbitraryDirectory).Namespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability1));

    ThenStructureContainsCapability(
      typeof(ArbitraryDirectory).Namespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  [TestMethod]
  public void FindsRootCapabilitiesAndCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsDirectory(
      typeof(ArbitraryRootCapability).Namespace);

    ThenStructureContainsCapability(
      typeof(ArbitraryRootCapability).Namespace,
      nameof(ArbitraryRootCapability));

    ThenStructureContainsCurriculum(
      typeof(ArbitraryRootCurriculum).Namespace,
      nameof(ArbitraryRootCurriculum));
  }

  [TestMethod]
  public void DoesNotFindClassesWithoutMeaningfulContent()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      typeof(Anchor).Namespace,
      nameof(Anchor));
  }

  void WhenParseAssembly()
  {
    Model =  new AssemblyParser().Parse(LoadedAssembly);
  }

  void ThenStructureDoesNotContainNode(params ImmutableArray<string?> Path)
  {
    var (Parent, FinalName) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().NotContain(I => I.Name == FinalName);
  }

  void ThenStructureContainsCapability(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(Path, NodeType.Capability);
  }

  void ThenStructureContainsCurriculum(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(Path, NodeType.Curriculum);
  }

  void ThenStructureHasNodeOfTypeAtPath(ImmutableArray<string?> Path, NodeType NodeType)
  {
    var (Parent, Name) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().ContainSingle(HasNameAndType(Name, NodeType));
  }

  void ThenStructureContainsDirectory(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(Path, NodeType.Directory);
  }

  void ThenModelNameIs(string Expected)
  {
    Model.Name.Should().Be(Expected);
  }

  void ThenStructureContainsMindPlace(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(Path, NodeType.MindPlace);
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
    var ContainerNodes = Path[..^1];
    var FinalName = Path[^1];
    var Parent =
      ContainerNodes.Aggregate((ScenariosModelNode)Model, (Predecessor, Name) => Predecessor.ChildNodes.Single(N => N.Name == Name));
    return (Parent, FinalName);
  }

  public enum NodeType
  {
    Model,
    Directory,
    Curriculum,
    Capability,
    MindPlace
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
  }
}