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
public class AutomationLoops
{
  MockGate Gate = null!;
  MockAutomationJob Pass = null!;
  MockIncrementable Counter = null!;
  AutomationJob Loop = null!;

  [TestInitialize]
  public void SetUp()
  {
    Gate = new();
    Pass = new();
    Counter = new();
    Loop = new AutomationLoop(Pass, Gate, Counter);
  }

  [TestMethod]
  public async Task RunsPassOnce()
  {
    var PassCount = Any.Int(0, 10);
    GivenGateWillCloseAfterPasses(PassCount);

    await WhenRunAutomationLoop();

    ThenPassesShouldBe(PassCount);
  }

  [TestMethod]
  public async Task IncrementsCounter()
  {
    var PassCount = Any.Int(0, 10);
    GivenGateWillCloseAfterPasses(PassCount);

    await WhenRunAutomationLoop();

    ThenIncrementableShouldBe(PassCount);
  }

  void ThenIncrementableShouldBe(int PassCount)
  {
    Counter.Count.Should().Be(PassCount);
  }

  void ThenPassesShouldBe(int PassCount)
  {
    Pass.RunCount.Should().Be(PassCount);
  }

  Task WhenRunAutomationLoop()
  {
    return Loop.Run();
  }

  void GivenGateWillCloseAfterPasses(int Count)
  {
    foreach (var _ in Enumerable.Range(0, Count)) 
      Gate.Answers.Enqueue(true);

    Gate.Answers.Enqueue(false);
  }
}