﻿// MIT License
// 
// Copyright (c) 2024-2024 Hexagon Software LLC
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
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public partial class GeneratedMinds
{
  [TestMethod]
  public void DoSimpleMake()
  {
    var Brain = new MockBrain(SimpleMakeMockMind.Input.Length, SimpleMakeMockMind.Output.Length);
    var Mind = new SimpleMakeMockMind(Brain);
    var InputToMakeCall = new SimpleInputData
    {
      P1 = Any.Float,
      P2 = Any.Float
    };
    var ExpectedBrainInput = MakeReferenceFloats(new SimpleMakeMockMind.Input
    {
      OperationCode = 1,
      Parameters =
      {
        MakeSimpleOutput =
        {
          Simple1 = InputToMakeCall
        }
      }
    });
    var ExpectedOutput = new SimpleOutputData
    {
      R1 = Any.Float
    };
    var StipulatedBrainOutput = MakeReferenceFloats(new SimpleMakeMockMind.Output
    {
      Parameters =
      {
        MakeSimpleOutput =
        {
          Value = ExpectedOutput
        }
      }
    });
    Brain.MakeInferenceFunc = Parameters =>
    {
      Parameters.Should().BeEquivalentTo(ExpectedBrainInput);

      return new MockInference(StipulatedBrainOutput);
    };

    var Actual = Mind.MakeSimpleOutput(InputToMakeCall).ConsumeDetached();

    Actual.Should().BeEquivalentTo(ExpectedOutput);
  }

  [TestMethod]
  public void StateIsCopiedIntoMakeInput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var OriginalState = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain)
    {
      SomeState = OriginalState
    };
    float[]? Actual = null;

    var CoreMakeInference = Brain.MakeInferenceFunc;
    Brain.MakeInferenceFunc = Parameters =>
    {
      var CapturedInput = StatefulMind.Input.UnmarshalFrom(Parameters);
      Actual = CapturedInput.Parameters.SomeState.Value;

      return CoreMakeInference(Parameters);
    };

    Mind.MakeSimpleOutput(new()
    {
      P1 = Any.Float,
      P2 = Any.Float
    }).ConsumeDetached();

    Actual.Should().BeEquivalentTo(OriginalState);
  }

  [TestMethod]
  public void StateIsCopiedFromMakeOutput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var FinalState = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain);

    Brain.MakeInferenceFunc = Parameters =>
    {
      var Output = new StatefulMind.Output();
      Output.Parameters.SomeState.Value = FinalState;
      var Buffer = new float[StatefulMind.Output.Length];
      Output.MarshalTo(Buffer);

      return new MockInference(Buffer);
    };

    Mind.MakeSimpleOutput(new()
    {
      P1 = Any.Float,
      P2 = Any.Float
    }).ConsumeDetached();

    Mind.SomeState.Should().BeEquivalentTo(FinalState);
  }

  [TestMethod]
  public void TrainingAsOutputThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var Thought = Mind.MakeSimpleOutput(new());
    var Reward = Any.PositiveOrNegativeFloat;

    Thought.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.MakeSimpleOutputIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.MakeSimpleOutputParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] { OutputStart..OutputEnd })
    ]);
  }

  [TestMethod]
  public void TrainingAsContributingThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var T = Thought.Do(R =>
    {
      R.Consume(Mind.MakeSimpleOutput(new()));
      R.Incorporate(Thought.Capture(new object(), new MockTrainingPolicy() { Mind = Mind }));
    });
    var Reward = Any.PositiveOrNegativeFloat;

    T.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.MakeSimpleOutputIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.MakeSimpleOutputParameters.Length;
    var StateStart = StatefulMind.Output.ParametersIndex +
                     StatefulMind.Output.OutputParameters.SomeStateIndex;
    var StateEnd = StateStart + StatefulMind.Output.OutputParameters.SomeStateParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] { OutputStart..OutputEnd, StateStart..StateEnd })
    ]);
  }

  float[] MakeReferenceFloats<T>(T ToPersist) where T : CognitiveData<T>
  {
    var Result = new float[T.Length];

    ToPersist.MarshalTo(Result);

    return Result;
  }

  [CognitiveData]
  public partial class SimpleInputData
  {
    public float P1;
    public float P2;
  }

  [CognitiveData]
  public partial class SimpleOutputData
  {
    public float R1;
  }

  [Mind]
  partial class SimpleMakeMockMind
  {
    [Make]
    public partial Thought<SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);
  }

  [Mind]
  partial class StatefulMind
  {
    public const int StateCount = 128;
    [CognitiveDataCount(StateCount), State] 
    public float[] SomeState = new float[128];

    [Make]
    public partial Thought<SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);
  }
}