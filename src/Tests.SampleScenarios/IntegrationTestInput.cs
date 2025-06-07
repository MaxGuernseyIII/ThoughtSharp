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

using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;

namespace Tests.SampleScenarios;

public partial class IntegrationTestInput
{
  [Mind]
  public partial class TheMind;

  [MindPlace]
  public class MindPlace : MindPlace<TheMind, TorchBrain>
  {
    public override TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.For<TheMind>().Build();
    }

    public override void LoadSavedBrain(TorchBrain ToLoad)
    {
    }

    public override void SaveBrain(TorchBrain ToSave)
    {
    }
  }

  [Capability]
  public class Runnables(TheMind Mind)
  {
    [Behavior]
    public void WillPass()
    {
    }

    [Behavior]
    public void WillFail()
    {
      throw new InvalidOperationException("Fail!");
    }
    [Behavior]
    public Task WillPassAsync()
    {
      return Task.CompletedTask;
    }

    [Behavior]
    public Task WillAsync()
    {
      return Task.FromException(new InvalidOperationException("Fail!"));
    }
  }

  [Curriculum]
  public class FastSuccess
  {
    [Phase(1)]
    [MaximumAttempts(20)]
    [ConvergenceStandard(Fraction = 1, Of = 10)]
    [Include(typeof(Runnables), Behaviors = [nameof(Runnables.WillPass), nameof(Runnables.WillPassAsync)])]
    public class RunSuccesses;
  }
}