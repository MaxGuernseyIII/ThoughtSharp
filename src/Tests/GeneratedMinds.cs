// MIT License
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

using System.Net.Http.Headers;
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
    var ExpectedOutput = new SimpleOutputData
    {
      R1 = Any.Float
    };
    Brain.SetOutputForOnlyInput(new SimpleMakeMockMind.Input
    {
      OperationCode = 1,
      Parameters =
      {
        MakeSimpleOutput =
        {
          Simple1 = InputToMakeCall
        }
      }
    }, new SimpleMakeMockMind.Output
    {
      Parameters =
      {
        MakeSimpleOutput =
        {
          Value = ExpectedOutput
        }
      }
    });

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
    var ActualCapture = CaptureInputState(Brain);

    Mind.MakeSimpleOutput(new()
    {
      P1 = Any.Float,
      P2 = Any.Float
    }).ConsumeDetached();

    ActualCapture.Captured.Should().BeEquivalentTo(OriginalState);
  }

  static Capture<float[]> CaptureInputState(MockBrain Brain)
  {
    var ActualCapture = new Capture<float[]>();

    var CoreMakeInference = Brain.MakeInferenceFunc;
    Brain.MakeInferenceFunc = Parameters =>
    {
      var CapturedInput = StatefulMind.Input.UnmarshalFrom(Parameters);
      ActualCapture.Captured = CapturedInput.Parameters.SomeState.Value;

      return CoreMakeInference(Parameters);
    };
    return ActualCapture;
  }

  [TestMethod]
  public void StateIsCopiedFromMakeOutput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var FinalState = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain);

    SetOutputState(Brain, FinalState);

    Mind.MakeSimpleOutput(new()
    {
      P1 = Any.Float,
      P2 = Any.Float
    }).ConsumeDetached();

    Mind.SomeState.Should().BeEquivalentTo(FinalState);
  }

  static void SetOutputState(MockBrain Brain, float[] FinalState)
  {
    Brain.MakeInferenceFunc = Parameters =>
    {
      var Output = new StatefulMind.Output();
      Output.Parameters.SomeState.Value = FinalState;
      var Buffer = new float[StatefulMind.Output.Length];
      Output.MarshalTo(Buffer);

      return new MockInference(Buffer);
    };
  }

  [TestMethod]
  public void TrainingOfMakeAsOutputThought()
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
      (Reward, new[] {OutputStart..OutputEnd})
    ]);
  }

  [TestMethod]
  public void TrainingOfMakeAsContributingThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var T = Thought.Do(R =>
    {
      R.Consume(Mind.MakeSimpleOutput(new()));
      R.Incorporate(Thought.Capture(new object(), new MockTrainingPolicy {Mind = Mind}));
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
      (Reward, new[] {OutputStart..OutputEnd, StateStart..StateEnd})
    ]);
  }

  [TestMethod]
  public void UseSynchronousActionSurfaceOperation1()
  {
    var ExpectedSomeData = Any.Float;

    TestSynchronousUseMethod(new()
    {
      ActionCode = 1,
      MoreActions = false,
      Parameters =
      {
        DoSomething1 =
        {
          SomeData = ExpectedSomeData
        }
      }
    }, ExpectedSomeData, null);
  }

  [TestMethod]
  public void UseSynchronousActionSurfaceOperation2()
  {
    var ExpectedSomeOtherDataData = Any.Float;

    TestSynchronousUseMethod(new()
    {
      ActionCode = 2,
      MoreActions = false,
      Parameters =
      {
        DoSomething2 =
        {
          SomeOtherData = ExpectedSomeOtherDataData
        }
      }
    }, null, ExpectedSomeOtherDataData);
  }

  [DataRow(true)]
  [DataRow(false)]
  [TestMethod]
  public void UseSynchronousActionSurfaceReturnsMore(bool ExpectedMore)
  {
    var Surface = new MockSynchronousSurface();
    var ActualMore = ExecuteSynchronousUseOperation(new()
    {
      ActionCode = (ushort)Any.Int(0, 2),
      MoreActions = ExpectedMore
    }, Surface);

    ActualMore.Should().Be(ExpectedMore);
  }

  [TestMethod]
  public void UseSynchronousActionSurfaceFeedsStateAsInput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var State = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain)
    {
      SomeState = State
    };
    var ActualCapture = CaptureInputState(Brain);

    Mind.SynchronousUseSomeInterface(new MockSynchronousSurface(), Any.Int(0, 100), Any.Int(0, 100));

    ActualCapture.Captured.Should().Equal(State);
  }

  [TestMethod]
  public void UseSynchronousActionSurfaceCapturesStateFromOutput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var State = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain);
    SetOutputState(Brain, State);

    Mind.SynchronousUseSomeInterface(new MockSynchronousSurface(), Any.Int(0, 100), Any.Int(0, 100));

    Mind.SomeState.Should().Equal(State);
  }

  [TestMethod]
  public void TrainingOfSynchronousUseAsOutputThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var Thought = Mind.SynchronousUseSomeInterface(new MockSynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100));
    var Reward = Any.PositiveOrNegativeFloat;

    Thought.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.SynchronousUseSomeInterfaceIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.SynchronousUseSomeInterfaceParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] {OutputStart..OutputEnd})
    ]);
  }

  [TestMethod]
  public void TrainingOfSynchronousUseAsContributingThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var T = Thought.Do(R =>
    {
      R.Consume(Mind.SynchronousUseSomeInterface(new MockSynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100)));
      R.Incorporate(Thought.Capture(new object(), new MockTrainingPolicy { Mind = Mind }));
    });
    var Reward = Any.PositiveOrNegativeFloat;

    T.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.SynchronousUseSomeInterfaceIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.SynchronousUseSomeInterfaceParameters.Length;
    var StateStart = StatefulMind.Output.ParametersIndex +
                     StatefulMind.Output.OutputParameters.SomeStateIndex;
    var StateEnd = StateStart + StatefulMind.Output.OutputParameters.SomeStateParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] {OutputStart..OutputEnd, StateStart..StateEnd})
    ]);
  }

  [TestMethod]
  public async Task UseAsynchronousActionSurfaceOperation1()
  {
    var ExpectedSomeData = Any.Float;

    await TestAsynchronousUseMethod(new()
    {
      ActionCode = 1,
      MoreActions = false,
      Parameters =
      {
        DoSomething1 =
        {
          SomeData = ExpectedSomeData
        }
      }
    }, ExpectedSomeData, null);
  }

  [TestMethod]
  public async Task UseAsynchronousActionSurfaceOperation2()
  {
    var ExpectedSomeOtherDataData = Any.Float;

    await TestAsynchronousUseMethod(new()
    {
      ActionCode = 2,
      MoreActions = false,
      Parameters =
      {
        DoSomething2 =
        {
          SomeOtherData = ExpectedSomeOtherDataData
        }
      }
    }, null, ExpectedSomeOtherDataData);
  }

  [DataRow(true)]
  [DataRow(false)]
  [TestMethod]
  public async Task UseAsynchronousActionSurfaceReturnsMore(bool ExpectedMore)
  {
    var Surface = new MockAsynchronousSurface();
    var ActualMore = await ExecuteAsynchronousUseOperation(new()
    {
      ActionCode = (ushort)Any.Int(0, 2),
      MoreActions = ExpectedMore
    }, Surface);

    ActualMore.Should().Be(ExpectedMore);
  }

  [TestMethod]
  public async Task UseAsynchronousActionSurfaceFeedsStateAsInput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var State = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain)
    {
      SomeState = State
    };
    var ActualCapture = CaptureInputState(Brain);

    await Mind.AsynchronousUseSomeInterface(new MockAsynchronousSurface(), Any.Int(0, 100), new MockSynchronousSurface(), Any.Int(0, 100));

    ActualCapture.Captured.Should().Equal(State);
  }

  [TestMethod]
  public async Task UseAsynchronousActionSurfaceCapturesStateFromOutput()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var State = Any.FloatArray(StatefulMind.StateCount);
    var Mind = new StatefulMind(Brain);
    SetOutputState(Brain, State);

    await Mind.AsynchronousUseSomeInterface(new MockAsynchronousSurface(), Any.Int(0, 100), new MockSynchronousSurface(), Any.Int(0, 100));

    Mind.SomeState.Should().Equal(State);
  }

  [TestMethod]
  public async Task TrainingOfUseAsOutputThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var Thought = await Mind.AsynchronousUseSomeInterface(new MockAsynchronousSurface(), Any.Int(0, 10), new MockSynchronousSurface(), Any.Int(-100, 100));
    var Reward = Any.PositiveOrNegativeFloat;

    Thought.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.AsynchronousUseSomeInterfaceIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.AsynchronousUseSomeInterfaceParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] {OutputStart..OutputEnd})
    ]);
  }

  [TestMethod]
  public async Task TrainingOfUseAsContributingThought()
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var Mind = new StatefulMind(Brain);

    var T = await Thought.DoAsync(async R =>
    {
      R.Consume(await Mind.AsynchronousUseSomeInterface(new MockAsynchronousSurface(), Any.Int(0, 10), new MockSynchronousSurface(), Any.Int(-100, 100)));
      R.Incorporate(Thought.Capture(new object(), new MockTrainingPolicy { Mind = Mind }));
    });
    var Reward = Any.PositiveOrNegativeFloat;

    T.ApplyIncentive(Reward);

    var Inference = Brain.MockInferences.Single();
    var OutputStart = StatefulMind.Output.ParametersIndex +
                      StatefulMind.Output.OutputParameters.AsynchronousUseSomeInterfaceIndex;
    var OutputEnd = OutputStart + StatefulMind.Output.OutputParameters.AsynchronousUseSomeInterfaceParameters.Length;
    var StateStart = StatefulMind.Output.ParametersIndex +
                     StatefulMind.Output.OutputParameters.SomeStateIndex;
    var StateEnd = StateStart + StatefulMind.Output.OutputParameters.SomeStateParameters.Length;
    Inference.Incentives.Should().BeEquivalentTo([
      (Reward, new[] {OutputStart..OutputEnd, StateStart..StateEnd})
    ]);
  }

  static void TestSynchronousUseMethod(SynchronousActionSurface.Output Selection, float? ExpectedSomeData,
    float? ExpectedSomeOtherData)
  {
    var Surface = new MockSynchronousSurface();
    ExecuteSynchronousUseOperation(Selection, Surface);

    Surface.SomeData.Should().Be(ExpectedSomeData);
    Surface.SomeOtherData.Should().Be(ExpectedSomeOtherData);
  }

  static bool ExecuteSynchronousUseOperation(SynchronousActionSurface.Output Selection, MockSynchronousSurface Surface)
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var ExpectedInput = new StatefulMind.Input
    {
      OperationCode = 2,
      Parameters =
      {
        SynchronousUseSomeInterface =
        {
          Argument1 = Any.Int(0, 100),
          Argument2 = Any.Int(0, 10000)
        }
      }
    };

    var StipulatedOutput = new StatefulMind.Output
    {
      Parameters =
      {
        SynchronousUseSomeInterface =
        {
          Surface = Selection
        }
      }
    };
    Brain.SetOutputForOnlyInput(ExpectedInput, StipulatedOutput);
    var Thought1 = new StatefulMind(Brain).SynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument1,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument2);
    var Thought = Thought1;
    var More = Thought.ConsumeDetached();
    return More;
  }

  static async Task TestAsynchronousUseMethod(AsynchronousActionSurface.Output Selection, float? ExpectedSomeData,
    float? ExpectedSomeOtherData)
  {
    var Surface = new MockAsynchronousSurface();
    await ExecuteAsynchronousUseOperation(Selection, Surface);

    Surface.SomeData.Should().Be(ExpectedSomeData);
    Surface.SomeOtherData.Should().Be(ExpectedSomeOtherData);
  }

  static async Task<bool> ExecuteAsynchronousUseOperation(AsynchronousActionSurface.Output Selection, MockAsynchronousSurface Surface)
  {
    var Brain = new MockBrain(StatefulMind.Input.Length, StatefulMind.Output.Length);
    var ExpectedInput = new StatefulMind.Input
    {
      OperationCode = 3,
      Parameters =
      {
        AsynchronousUseSomeInterface =
        {
          Argument1 = Any.Int(0, 100),
          Argument2 = Any.Int(0, 10000)
        }
      }
    };

    var StipulatedOutput = new StatefulMind.Output
    {
      Parameters =
      {
        AsynchronousUseSomeInterface =
        {
          Surface = Selection
        }
      }
    };
    Brain.SetOutputForOnlyInput(ExpectedInput, StipulatedOutput);
    var Thought = await new StatefulMind(Brain).AsynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.AsynchronousUseSomeInterface.Argument1, 
      new MockSynchronousSurface(),
      ExpectedInput.Parameters.AsynchronousUseSomeInterface.Argument2);
    var More = Thought.ConsumeDetached();
    return More;
  }

  class Capture<T>
  {
    public T? Captured { get; set; }
  }

  class MockSynchronousSurface : SynchronousActionSurface
  {
    public float? SomeData;
    public float? SomeOtherData;

    public void DoSomething1(float SomeData)
    {
      this.SomeData = SomeData;
    }

    public Thought DoSomething2(float SomeOtherData)
    {
      return Thought.Do(_ => { this.SomeOtherData = SomeOtherData; });
    }
  }

  class MockAsynchronousSurface : AsynchronousActionSurface
  {
    public float? SomeData;
    public float? SomeOtherData;

    public void DoSomething1(float SomeData)
    {
      this.SomeData = SomeData;
    }

    public Task<Thought> DoSomething2(float SomeOtherData)
    {
      return Task.FromResult(Thought.Do(_ => { this.SomeOtherData = SomeOtherData; }));
    }
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

  [CognitiveActions]
  partial interface SynchronousActionSurface
  {
    void DoSomething1(float SomeData);
    Thought DoSomething2(float SomeOtherData);
  }

  [CognitiveActions]
  partial interface AsynchronousActionSurface
  {
    void DoSomething1(float SomeData);
    Task<Thought> DoSomething2(float SomeOtherData);
  }

  [Mind]
  partial class StatefulMind
  {
    public const int StateCount = 128;

    [CognitiveDataCount(StateCount)] [State]
    public float[] SomeState = new float[128];

    [Make]
    public partial Thought<SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);

    [Use]
    public partial Thought<bool> SynchronousUseSomeInterface(
      SynchronousActionSurface Surface,
      int Argument1,
      int Argument2);

    [Use]
    public partial Task<Thought<bool>> AsynchronousUseSomeInterface(
      AsynchronousActionSurface Surface,
      int Argument1,
      SynchronousActionSurface SynchronousSurface,
      int Argument2);
  }
}