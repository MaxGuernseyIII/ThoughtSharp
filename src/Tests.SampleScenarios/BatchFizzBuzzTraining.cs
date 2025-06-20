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
using System.Text;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using ThoughtSharp.Scenarios;
using TorchSharp;

namespace Tests.SampleScenarios;

public static partial class BatchFizzBuzzTraining
{
  static Func<byte, bool> IsDivisibleBy(int Denominator)
  {
    return B => B % Denominator == 0;
  }

  static Func<byte, bool> IsNotDivisibleBy(int Denominator)
  {
    return B => B % Denominator != 0;
  }

  static class TrainingData
  {
    static readonly Random Core = new();

    public static IEnumerable<byte> Byte
    {
      get
      {
        var Buffer = new byte[1];

        while (true)
        {
          Core.NextBytes(Buffer);
          yield return Buffer[0];
        }
        // ReSharper disable once IteratorNeverReturns
      }
    }
  }

  [MindPlace]
  public class FizzBuzzMindPlace : MindPlace<FizzBuzzMind, TorchBrain>
  {
    static FizzBuzzMindPlace()
    {
      try
      {
        torch.InitializeDeviceType(DeviceType.CUDA);
      }
      catch
      {
      }
    }

    static BrainBuilder<TorchBrain, torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device> Builder
    {
      get
      {
        return TorchBrainBuilder.For<FizzBuzzMind>()
          .UsingSequence(Outer =>
            Outer
              .AddTimeAware(A => A.AddGRU(128))
              .AddParallel(P => P
                //.AddLogicPath(160, 40, 80)
                .AddLogicPath(16, 4, 8)
                .AddPath(S => S))
          );
      }
    }

    public override TorchBrain MakeNewBrain()
    {
      return Builder.Build();
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
      return $"fizzbuzz-{Builder.CompactDescriptiveText}.pt";
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

      public virtual bool Equals(MockActionSurface? Other)
      {
        if (Other is null) return false;
        if (ReferenceEquals(this, Other)) return true;
        return ContentBuilder.Equals(Other.ContentBuilder);
      }

      public override string ToString()
      {
        return $"surface:<{ContentBuilder}>";
      }

      public override int GetHashCode()
      {
        return ContentBuilder.GetHashCode();
      }
    }

    [Capability]
    public class Calculations(FizzBuzzMind Mind)
    {
      const int FizzFactor = 3;
      const int BuzzFactor = 5;

      readonly FizzBuzzHybridReasoning Reasoning = new(Mind);
      readonly FizzBuzzMind MindWithChainedReasoning = Mind.WithChainedReasoning();

      [Behavior]
      public void Fizz()
      {
        var Inputs = 
            Enumerable.Range(0, byte.MaxValue + 1)
              .Select(I => (byte)I)
          .Where(IsDivisibleBy(FizzFactor))
          .Where(IsNotDivisibleBy(BuzzFactor))
          .ToArray();

        var Cases = Inputs.Select(Input => (Input : new FizzBuzzInput() { Value = Input }, Surface: new MockActionSurface()));

        var Result = MindWithChainedReasoning.WriteForNumberBatch(
          [..Cases.Select(Pair => new FizzBuzzMind.WriteForNumberArgs(Pair.Surface, Pair.Input))]);

        UseFeedbackMethod<FizzBuzzTerminal> Expected = (Mock, More) =>
        {
          Mock.Fizz();
          More.Value = false;
        };

        Result.FeedbackSink.TrainWith(Enumerable.Repeat(Expected, Inputs.Length).ToImmutableArray());
        Result.Payload.ShouldBe(Enumerable.Repeat(false, Inputs.Length));
      }

      [Behavior]
      public void Buzz()
      {
        var Inputs =
          Enumerable.Range(0, byte.MaxValue + 1)
            .Select(I => (byte)I)
            .Where(IsDivisibleBy(BuzzFactor))
            .Where(IsNotDivisibleBy(FizzFactor))
            .ToArray();

        var Cases = Inputs.Select(Input => (Input: new FizzBuzzInput() { Value = Input }, Surface: new MockActionSurface()));

        var Result = MindWithChainedReasoning.WriteForNumberBatch(
          [.. Cases.Select(Pair => new FizzBuzzMind.WriteForNumberArgs(Pair.Surface, Pair.Input))]);


        UseFeedbackMethod<FizzBuzzTerminal> Expected = (Mock, More) =>
        {
          Mock.Buzz();
          More.Value = false;
        };

        Result.FeedbackSink.TrainWith(Enumerable.Repeat(Expected, Inputs.Length).ToImmutableArray());
        Result.Payload.ShouldBe(Enumerable.Repeat(false, Inputs.Length));
      }

      [Behavior]
      public void FizzBuzz()
      {
        var Inputs =
          Enumerable.Range(0, byte.MaxValue + 1)
            .Select(I => (byte)I)
            .Where(IsDivisibleBy(BuzzFactor * FizzFactor))
            .ToArray();

        var Cases = Inputs.Select(Input => (Input: new FizzBuzzInput() { Value = Input }, Surface: new MockActionSurface()));

        var Result = MindWithChainedReasoning.WriteForNumberBatch(
          [.. Cases.Select(Pair => new FizzBuzzMind.WriteForNumberArgs(Pair.Surface, Pair.Input))]);


        Result.FeedbackSink.TrainWith(Enumerable.Repeat((UseFeedbackMethod<FizzBuzzTerminal>) ((Mock, More) =>
        {
          Mock.Fizz();
          More.Value = true;
        }), Inputs.Length).ToImmutableArray());
        Result.Payload.ShouldBe(Enumerable.Repeat(true, Inputs.Length));

        Result = Mind.WriteForNumberBatch(
          [.. Cases.Select(Pair => new FizzBuzzMind.WriteForNumberArgs(Pair.Surface, Pair.Input))]);

        Result.FeedbackSink.TrainWith(Enumerable.Repeat((UseFeedbackMethod<FizzBuzzTerminal>) ((Mock, More) =>
        {
          Mock.Buzz();
          More.Value = false;
        }), Inputs.Length).ToImmutableArray());
        Result.Payload.ShouldBe(Enumerable.Repeat(false, Inputs.Length));
      }

      [Behavior]
      public void WriteValue()
      {

        var Inputs =
          Enumerable.Range(0, byte.MaxValue + 1)
            .Select(I => (byte)I)
            .Where(IsNotDivisibleBy(BuzzFactor))
            .Where(IsNotDivisibleBy(FizzFactor))
            .ToArray();

        var Cases = Inputs.Select(Input => (Input: new FizzBuzzInput() { Value = Input }, Surface: new MockActionSurface()));

        var Result = MindWithChainedReasoning.WriteForNumberBatch(
          [.. Cases.Select(Pair => new FizzBuzzMind.WriteForNumberArgs(Pair.Surface, Pair.Input))]);


        Result.FeedbackSink.TrainWith(Inputs.Select(Input => (UseFeedbackMethod<FizzBuzzTerminal>)((Mock, More) =>
        {
          Mock.WriteNumber(Input);
          More.Value = false;
        })).ToImmutableArray());
        Result.Payload.ShouldBe(Enumerable.Repeat(false, Inputs.Length));
      }
    }
  }

  [Curriculum]
  [MaximumAttempts(200000)]
  public static class BatchFizzBuzzTrainingPlan
  {
    [Phase(1)]
    [ConvergenceStandard(Fraction = .6, Of = 200)]
    [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations))]
    public class InitialSteps;

    [Phase(2)]
    [MaximumAttempts(80000)]
    [ConvergenceStandard(Fraction = .98, Of = 500)]
    public class FocusedTraining
    {
      [Phase(2.1)]
      [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Fizz)])]
      public class FocusOnFizz;

      [Phase(2.2)]
      [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Buzz)])]
      // TODO: Support weights in training
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.Fizz)], Weight = 0.25)]
      public class FocusOnBuzz;

      [Phase(2.3)]
      [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.FizzBuzz)])]
      // TODO: Support weights in training
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.Buzz)], Weight = 0.05)]
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.Fizz)], Weight = 0.05)]
      public class FocusOnFizzBuzz;

      [Phase(2.4)]
      [ConvergenceStandard(Fraction = .98, Of = 2000)]
      [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.WriteValue)])]
      // TODO: Support weights in training
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.Buzz)], Weight = 0.05)]
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.Fizz)], Weight = 0.05)]
      //[Include(typeof(Calculations), Behaviors = [nameof(Calculations.FizzBuzz)], Weight = 0.05)]
      public class FocusOnWriting;
    }

    [Phase(3)]
    [ConvergenceStandard(Fraction = 1, Of = 1000)]
    [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations))]
    public class AllOperationsMustWork;

    [Phase(4)]
    [ConvergenceStandard(Fraction = 1, Of = 50)]
    [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Calculations))]
    [Include(typeof(FizzBuzzTraining.FizzbuzzScenarios.Solution))]
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
        new NumberToFloatingPointCodec<byte, float>(new NormalizingCodec<float>(
          new RoundingCodec<float>(new CopyFloatCodec()), byte.MinValue, byte.MaxValue)));
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
    public string DoFullFizzBuzz()
    {
      var Terminal = new ProductionTerminal();

      for (byte I = 1; I <= 100; ++I)
      {
        var Sink = CalculateStepValue(I, Terminal);
        Terminal.Flush();
      }

      return Terminal.Result.Trim();
    }

    public AccumulatedUseFeedback<FizzBuzzTerminal> CalculateStepValue(byte Input, FizzBuzzTerminal Terminal)
    {
      return Mind.Use(M => M.WriteForNumber(Terminal, new() {Value = Input}));
    }

    class ProductionTerminal : FizzBuzzTerminal
    {
      readonly StringBuilder CurrentContentBuilder = new();

      public string Result => CurrentContentBuilder.ToString();

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

      public void Flush()
      {
        CurrentContentBuilder.Append(" ");
      }
    }
  }
}