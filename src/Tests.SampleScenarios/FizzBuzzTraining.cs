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

using System.Text;
using Tests.SampleScenarios;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using ThoughtSharp.Scenarios;

[assembly: MindPlace<FizzBuzzTraining.FizzBuzzMindPlace, FizzBuzzTraining.FizzBuzzMind, TorchBrain>]

namespace Tests.SampleScenarios;

public static partial class FizzBuzzTraining
{
  public class FizzBuzzMindPlace(MindPlaceConfig Config) : MindPlace<FizzBuzzMind, TorchBrain>
  {
    public TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.ForTraining<FizzBuzzMind>()
        .UsingSequence(Outer =>
          Outer
            .AddGRU(128)
            .AddParallel(P => P
              .AddLogicPath(16, 4, 8)
              .AddPath(S => S))
        ).Build();
    }

    public void LoadSavedBrain(TorchBrain ToLoad, string Discriminator)
    {
      ToLoad.Load(GetSavePath(Discriminator));
    }

    public void SaveBrain(TorchBrain ToSave, string Discriminator)
    {
      ToSave.Save(GetSavePath(Discriminator));
    }

    string GetSavePath(string Discriminator)
    {
      return $"{Config.Strings["models.path"]}fizzbuzz{Discriminator}.pt";
    }
  }

  [Capability]
  public class FizzbuzzScenarios
  {
    record MockActionSurface : FizzBuzzTerminal
    {
      readonly StringBuilder ContentBuilder = new();

      public void WriteNumber(byte ToWrite)
      {
        ContentBuilder.Append(ToWrite);
      }

      public void Fizz()
      {
        ContentBuilder.Append("fizz");
      }

      public void Buzz()
      {
        ContentBuilder.Append("buzz");
      }
    }

    [Capability]
    public class Calculations
    {
      const int FizzFactor = 3;
      const int BuzzFactor = 5;
      static readonly Random Random = new();
      readonly FizzBuzzHybridReasoning Reasoning;
      readonly MockActionSurface Surface;

      public Calculations(FizzBuzzMind Mind)
      {
        Surface = new();
        Reasoning = new(Mind, Surface);
      }

      byte AnyByteDivisibleBy(int Factor)
      {
        return (byte) (Random.Next(byte.MaxValue / Factor) * Factor);
      }

      byte AnyByteNotDivisibleBy(params IEnumerable<int> ExcludedFactors)
      {
        var Available = Enumerable.Range(0, byte.MaxValue + 1)
          .Where(Candidate => ExcludedFactors.All(F => Candidate % F != 0)).ToArray();

        return (byte) Available[Random.Next(Available.Length)];
      }

      [Behavior]
      public void Fizz()
      {
        var Input = AnyByteDivisibleBy(FizzFactor);

        var Result = Reasoning.CalculateStepValue(Input);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Fizz()
        );
      }

      [Behavior]
      public void Buzz()
      {
        var Input = AnyByteDivisibleBy(BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Buzz()
        );
      }

      [Behavior]
      public void FizzBuzz()
      {
        var Input = AnyByteDivisibleBy(FizzFactor * BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Fizz(),
          S => S.Buzz()
        );
      }

      [Behavior]
      public void WriteValueScenario()
      {
        var Input = AnyByteNotDivisibleBy(FizzFactor, BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.WriteNumber(Input)
        );
      }
    }
  }

  [CognitiveActions]
  public partial interface FizzBuzzTerminal
  {
    void WriteNumber([Categorical] byte ToWrite);
    void Fizz();
    void Buzz();
  }

  [CognitiveData]
  public partial class FizzBuzzInput
  {
    public byte Value { get; set; }

    public static CognitiveDataCodec<byte> ValueCodec { get; } =
      new CompositeCodec<byte>(
        new BitwiseOneHotNumberCodec<byte>(),
        new NormalizeNumberCodec<byte, float>(
          byte.MinValue, byte.MaxValue, new RoundingCodec<float>(new CopyFloatCodec())));
  }

  [Mind]
  public partial class FizzBuzzMind
  {
    [Use]
    public partial CognitiveResult<bool, UseFeedbackMethod<FizzBuzzTerminal>> WriteForNumber(FizzBuzzTerminal Surface,
      FizzBuzzInput InputData);
  }

  public class FizzBuzzHybridReasoning(FizzBuzzMind Mind, FizzBuzzTerminal Terminal)
  {
    public AccumulatedUseFeedback<FizzBuzzTerminal> CalculateStepValue(byte Input)
    {
      return Mind.Use(M => M.WriteForNumber(Terminal, new() {Value = Input}));
    }
  }
}