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
using Google.Protobuf.WellKnownTypes;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;

namespace Tests.SampleScenarios;

public partial class CountOnes
{
  [MindPlace]
  public class CountOnesMindPlace : MindPlace<CounterMind, TorchBrain>
  {
    public override TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.For<CounterMind>().UsingSequence(S => S.AddGRU(16).AddLinear(16)).Build();
    }

    public override void LoadSavedBrain(TorchBrain ToLoad)
    {
    }

    public override void SaveBrain(TorchBrain ToSave)
    {
    }
  }

  [MaximumAttempts(50000)]
  [ConvergenceStandard(Fraction = .95, Of = 50)]
  [Curriculum]
  public class Train
  {
    [Phase(1)]
    [Include(typeof(ItCountsTimes))]
    public class DoIt;
  }

  [Capability]
  public class ItCountsTimes(CounterMind Mind)
  {
    static readonly Random Source = new();

    [Behavior]
    public void TimeCountCorrect()
    {
      var TestMind = Mind.WithChainedReasoning();
      var Count = Source.Next(2, 12);

      TestMind.TellItHowManyTimes([..Enumerable.Repeat(new Token(), Count)]);

      var R = TestMind.GetTimeCount();

      Assert.That(R).Is(new() { Times = Count}, A => A.Expect(I => I.Times, (Actual, Expected) => Actual.ShouldBeApproximately(Expected, .125f)));
    }
  }

  [Mind]
  public partial class CounterMind
  {
    [Tell]
    public partial void TellItHowManyTimes(ImmutableArray<Token> Times);

    [Make]
    public partial CognitiveResult<Result, Result> GetTimeCount();
  }

  [CognitiveData]
  public partial class Token
  {
  }

  [CognitiveData]
  public partial class Result
  {
    public float Times;
  }
}