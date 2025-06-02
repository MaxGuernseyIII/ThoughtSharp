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

    ThenStructureContainsDirectory(typeof(FizzBuzzTraining).FullName);
  }

  [TestMethod]
  public void FindsCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculum(
      typeof(FizzBuzzTraining).FullName,
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan));
  }

  void ThenStructureContainsCurriculum(params ImmutableArray<string?> Path)
  {
    var ContainerNodes = Path[..^1];
    var Parent =
      ContainerNodes.Aggregate((ScenariosModelNode)Model, (Predecessor, Name) => Predecessor.ChildNodes.Single(N => N.Name == Name));

    Parent.ChildNodes.Should().ContainSingle(HasNameAndType(Path[^1], NodeType.Curriculum));
  }

  void WhenParseAssembly()
  {
    Model =  new AssemblyParser().Parse(LoadedAssembly);
  }

  void ThenStructureContainsDirectory(string? FullName)
  {
    Model.ChildNodes.Should().ContainSingle(
      HasNameAndType(FullName, NodeType.Directory)
    );
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