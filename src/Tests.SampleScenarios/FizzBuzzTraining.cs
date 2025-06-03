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
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using ThoughtSharp.Scenarios;
using static Tests.SampleScenarios.FizzBuzzTraining.FizzbuzzScenarios;

namespace Tests.SampleScenarios;

public static partial class FizzBuzzTraining
{
  [MindPlace]
  public class FizzBuzzMindPlace(MindPlaceConfig Config) : MindPlace<FizzBuzzMind, TorchBrain>
  {
    public override TorchBrain MakeNewBrain()
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

    public override void LoadSavedBrain(TorchBrain ToLoad)
    {
      ToLoad.Load(GetSavePath());
    }

    public override void SaveBrain(TorchBrain ToSave)
    {
      ToSave.Save(GetSavePath());
    }

    string GetSavePath()
    {
      return $"{Config.Strings["models.path"]}fizzbuzz.pt";
    }
  }

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
    public class Calculations(FizzBuzzMind Mind)
    {
      const int FizzFactor = 3;
      const int BuzzFactor = 5;
      static readonly Random Random = new();
      readonly FizzBuzzHybridReasoning Reasoning = new(Mind);
      readonly MockActionSurface Surface = new();

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

        var Result = Reasoning.CalculateStepValue(Input, Surface);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Fizz()
        );
      }

      [Behavior]
      public void Buzz()
      {
        var Input = AnyByteDivisibleBy(BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input, Surface);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Buzz()
        );
      }

      [Behavior]
      public void FizzBuzz()
      {
        var Input = AnyByteDivisibleBy(FizzFactor * BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input, Surface);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.Fizz(),
          S => S.Buzz()
        );
      }

      [Behavior]
      public void WriteValue()
      {
        var Input = AnyByteNotDivisibleBy(FizzFactor, BuzzFactor);

        var Result = Reasoning.CalculateStepValue(Input, Surface);

        Assert.That(Result).ProducedCallsOn(Surface,
          S => S.WriteNumber(Input)
        );
      }
    }

    [Dependency(typeof(Calculations))]
    [Capability]
    public class Solution(FizzBuzzMind Mind)
    {
      const string ExpectedFinalOutput =
        "1 2 fizz 4 buzz fizz 7 8 fizz buzz 11 fizz 13 14 fizzbuzz 16 17 fizz 19 buzz fizz 22 23 fizz buzz 26 fizz 28 29 fizzbuzz 31 32 fizz 34 buzz fizz 37 38 fizz buzz 41 fizz 43 44 fizzbuzz 46 47 fizz 49 buzz fizz 52 53 fizz buzz 56 fizz 58 59 fizzbuzz 61 62 fizz 64 buzz fizz 67 68 fizz buzz 71 fizz 73 74 fizzbuzz 76 77 fizz 79 buzz fizz 82 83 fizz buzz 86 fizz 88 89 fizzbuzz 91 92 fizz 94 buzz fizz 97 98 fizz buzz";

      readonly FizzBuzzHybridReasoning Reasoning = new(Mind);

      [Behavior]
      public void RequireFullFizzBuzzSolution()
      {
        var Result = Reasoning.DoFullFizzBuzz();

        Assert.That(Result).Is(ExpectedFinalOutput);
      }
    }
  }

  // this is a curriculum
  [Curriculum]
  public static class FizzBuzzTrainingPlan
  {
    // this is the first phase
    [Phase(1)]
    // continue until each included lesson has had 160 of the last 200 trials succeed
    [ConvergenceStandard(Fraction = .8, Of = 200)]
    // include all lessons from the Calculations class
    [Include(typeof(Calculations))]
    public class InitialSteps;

    // this is the second phase
    [Phase(2)]
    // continue until each included lesson has had 490 of the last 500 trials succeed
    [ConvergenceStandard(Fraction = .98, Of = 500)]
    public class FocusedTraining
    {
      [Phase(2.1)]
      [Include(typeof(Calculations), Behaviors = [nameof(Calculations.Fizz)])]
      public class FocusOnFizz;

      [Phase(2.2)]
      [Include(typeof(Calculations), Behaviors = [nameof(Calculations.Buzz)])]
      public class FocusOnBuzz;

      [Phase(2.3)]
      [Include(typeof(Calculations), Behaviors = [nameof(Calculations.FizzBuzz)])]
      public class FocusOnFizzBuzz;

      [Phase(2.4)]
      [Include(typeof(Calculations), Behaviors = [nameof(Calculations.WriteValue)])]
      public class FocusOnWriting;
    }

    [Phase(3)]
    [ConvergenceStandard(Fraction = 1, Of = 50)]
    [Include(typeof(Calculations))]
    [Include(typeof(Solution))]
    public class FinalTraining;
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

  public class FizzBuzzHybridReasoning(FizzBuzzMind Mind)
  {
    public CognitiveResult<string, string> DoFullFizzBuzz()
    {
      var Terminal = new ProductionTerminal();

      for (byte I = 1; I <= 100; ++I)
      {
        var Sink = CalculateStepValue(I, Terminal);
        Terminal.Flush(Sink);
      }

      return Terminal.Result;
    }

    public AccumulatedUseFeedback<FizzBuzzTerminal> CalculateStepValue(byte Input, FizzBuzzTerminal Terminal)
    {
      return Mind.Use(M => M.WriteForNumber(Terminal, new() {Value = Input}));
    }

    class ProductionTerminal : FizzBuzzTerminal
    {
      StringBuilder CurrentContentBuilder = new();

      public IncrementalCognitiveResult<string, string, string, IEnumerable<Action<FizzBuzzTerminal>>> Result
      {
        get;
        private set;
      } =
        new(
          Parts => string.Join(" ", Parts),
          Whole => Whole.Split(" ").Select(TransformToken)
        );

      public void WriteNumber(byte ToWrite)
      {
        CurrentContentBuilder.Append(ToWrite);
      }

      public void Fizz()
      {
        CurrentContentBuilder.Append("fizz");
      }

      public void Buzz()
      {
        CurrentContentBuilder.Append("buzz");
      }

      static IEnumerable<Action<FizzBuzzTerminal>> TransformToken(string Arg)
      {
        switch (Arg.ToLower())
        {
          case "fizz":
            yield return T => T.Fizz();
            break;
          case "buzz":
            yield return T => T.Buzz();
            break;
          case "fizzbuzz":
            yield return T => T.Fizz();
            yield return T => T.Buzz();
            break;
          default:
            var Input = byte.Parse(Arg);
            yield return T => T.WriteNumber(Input);
            break;
        }
      }

      public void Flush(FeedbackSink<IEnumerable<Action<FizzBuzzTerminal>>> FeedbackSink)
      {
        Result = Result.Add(CognitiveResult.From(CurrentContentBuilder.ToString(), FeedbackSink));
        CurrentContentBuilder = new();
      }
    }
  }
}