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
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;

namespace Tests.SampleScenarios;

public partial class AttentionExample
{
  [Mind]
  public partial class AttentionMind
  {
    [Tell]
    public partial void LoadNumbers(IEnumerable<NumberBoat> Numbers);

    [Make]
    public partial CognitiveResult<NumberBoat, NumberBoat> GetHighest();
  }

  [CognitiveData]
  public partial class NumberBoat
  {
    public float Number;
  }

  [MindPlace]
  public class MindPlace : MindPlace<AttentionMind, TorchBrain>
  {
    public override TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.For<AttentionMind>().UsingSequence(S => S.AddTimeAware(A => A
        .AddAttention(1, 8)
        .AddGRU(16)
      )).Build();
    }

    public override void LoadSavedBrain(TorchBrain ToLoad)
    {
    }

    public override void SaveBrain(TorchBrain ToSave)
    {
    }
  }

  [Capability]
  public class FindHighest(AttentionMind Mind)
  {
    static readonly Random Source = new();

    [Behavior]
    public void AlwaysFindsHighest()
    {
      var M = Mind.WithChainedReasoning();
      var Inputs = Enumerable.Range(0, Source.Next(2, 10)).Select(S => new NumberBoat() { Number = Source.NextSingle() }).ToImmutableArray();
      M.LoadNumbers(Inputs);

      var R = M.GetHighest();

      Assert.That(R).Is(new() { Number = Inputs.Max(B => B.Number)}, C => C.Expect(B => B.Number, (Actual, Expected) => Actual.ShouldBeApproximately(Expected, 0.05f)));
    }
  }

  [MaximumAttempts(50000)]
  [ConvergenceStandard(Fraction = .99, Of = 100)]
  [Curriculum]
  public class LearnToPickMaximum
  {
    [Phase(1)]
    [Include(typeof(FindHighest))]
    public class LearnMaximum;
  }
}