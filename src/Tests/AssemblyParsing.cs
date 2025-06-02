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
    var ContainerNodes = Path[..^1];
    var Parent =
      ContainerNodes.Aggregate((ScenariosModelNode)Model, (Predecessor, Name) => Predecessor.ChildNodes.Single(N => N.Name == Name));

    Parent.ChildNodes.Should().ContainSingle(HasNameAndType(Path[^1], NodeType));
  }

  void WhenParseAssembly()
  {
    Model =  new AssemblyParser().Parse(LoadedAssembly);
  }

  void ThenStructureContainsDirectory(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(Path, NodeType.Directory);
  }

  void ThenModelNameIs(string Expected)
  {
    Model.Name.Should().Be(Expected);
  }

  void ThenModelTypeIs(NodeType Expected)
  {
    Model.Type.Should().Be(Expected);
  }

  static Expression<Func<ScenariosModelNode, bool>> HasNameAndType(string? FullName, NodeType NodeType)
  {
    return I => I.Name == FullName && I.Type == NodeType;
  }
}