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

using System.Reflection;
using Tests.SampleScenarios;
using ThoughtSharp.Scenarios.Model;
using FluentAssertions;

namespace Tests.Integration;

[TestClass]
public class BehaviorInvocation
{
  static readonly string RootNamespace = typeof(Anchor).Namespace!;
  Assembly LoadedAssembly = null!;
  ScenariosModel Model = null!;

  [TestInitialize]
  public void SetUp()
  {
    var AbsoluteAssemblyPath = Path.GetFullPath(typeof(Anchor).Assembly.Location);
    var ProjectRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(AbsoluteAssemblyPath)!, @"..\..\..\"));
    var Relative = AbsoluteAssemblyPath[ProjectRoot.Length..];
    var LoadedAssemblyPath = Path.GetFullPath(Path.Combine(ProjectRoot, @"..\Tests.SampleScenarios", Relative));
    var Context = new ShapingAssemblyLoadContext(LoadedAssemblyPath);
    LoadedAssembly = Context.LoadFromAssemblyPath(LoadedAssemblyPath);
    Model = new AssemblyParser().Parse(LoadedAssembly);
  }

  [TestMethod]
  public async Task RunTests()
  {
    var SingleNode = Model.GetNodeAtEndOfPath(RootNamespace, nameof(IntegrationTestInput),
      nameof(IntegrationTestInput.Runnables), nameof(IntegrationTestInput.Runnables.WillPass));
    MindPool Pool = new(Model.MindPlaceIndex);
    var TestPass = Model.GetTestPassFor(Pool, new((ScenariosModelNode) null!, Any.TrainingMetadata()), new FalseGate(), Pool, new MockReporter(), SingleNode);

    var Result = await TestPass.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
  }

  Type? ReplaceProxyWithLoadedType(Type Proxy)
  {
    var Expected = LoadedAssembly.GetType(Proxy.FullName!);
    return Expected;
  }
}

static class Any
{
  public static TrainingMetadata TrainingMetadata()
  {
    return new() {MaximumAttempts = 1, SampleSize = 1, SuccessFraction = 1};
  }
}

public class MockReporter : Reporter
{
  public void Dispose()
  {

  }

  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
    
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
  }

  public void ReportExit(ScenariosModelNode Node)
  {
  }
}
