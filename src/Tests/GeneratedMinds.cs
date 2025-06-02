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
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public partial class GeneratedMinds
{
  [TestMethod]
  public void DoSimpleMake()
  {
    var Brain = new MockBrain<SimpleMakeMockMind.Input, SimpleMakeMockMind.Output>();
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
    Brain.SetOutputForOnlyInput(new()
    {
      OperationCode = 1,
      Parameters =
      {
        MakeSimpleOutput =
        {
          Simple1 = InputToMakeCall
        }
      }
    }, new()
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
  public void TrainingOfMakeAsOutputThought()
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);
    var T = Mind.MakeSimpleOutput(new());
    var ExpectedObject = new SimpleOutputData
    {
      R1 = Any.Float
    };

    T.Feedback.TrainWith(ExpectedObject);

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith(new StatelessMind.Output
    {
      Parameters =
      {
        MakeSimpleOutput =
        {
          Value = ExpectedObject
        }
      }
    });
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
      ActionCode = (ushort) Any.Int(0, 2),
      MoreActions = ExpectedMore
    }, Surface);

    ActualMore.Should().Be(ExpectedMore);
  }

  [TestMethod]
  public void TrainingOfSynchronousUseAsOutputThought()
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);

    var T = Mind.SynchronousUseSomeInterface(new MockSynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100));

    var ExpectedMore = Any.Bool;
    var SomeOtherData = Any.Float;

    T.Feedback.ExpectationsWere((Mock, More) =>
    {
      Mock.DoSomething2(SomeOtherData);
      More.Value = ExpectedMore;
    });

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith(new StatelessMind.Output
    {
      Parameters =
      {
        SynchronousUseSomeInterface =
        {
          Surface =
          {
            ActionCode = 2,
            MoreActions = ExpectedMore,
            Parameters =
            {
              DoSomething2 =
              {
                SomeOtherData = SomeOtherData
              }
            }
          }
        }
      }
    });
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
      ActionCode = (ushort) Any.Int(0, 2),
      MoreActions = ExpectedMore
    }, Surface);

    ActualMore.Should().Be(ExpectedMore);
  }

  [TestMethod]
  public async Task TrainingOfAsynchronousUseAsOutputThought()
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);

    var T = await Mind.AsynchronousUseSomeInterface(new MockAsynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100));

    var ExpectedMore = Any.Bool;
    var SomeOtherData = Any.Float;

    T.Feedback.ExpectationsWere((Mock, More) =>
    {
      Mock.DoSomething2(SomeOtherData);
      More.Value = ExpectedMore;
      return Task.CompletedTask;
    });

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith(new StatelessMind.Output
    {
      Parameters =
      {
        AsynchronousUseSomeInterface =
        {
          Surface =
          {
            ActionCode = 2,
            MoreActions = ExpectedMore,
            Parameters =
            {
              DoSomething2 =
              {
                SomeOtherData = SomeOtherData
              }
            }
          }
        }
      }
    });
  }

  [TestMethod]
  public void ChooseFromSmallCompleteBatchOfOptions()
  {
    TestChooseBatches(new([AnyMockOption(), AnyMockOption(), AnyMockOption()]));
  }

  [TestMethod]
  public void ChooseFromManyBatchesOfOptions()
  {
    TestChooseBatches(new([
      AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(),
      AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(),
      AnyMockOption()
    ]));
  }

  [TestMethod]
  public void ChooseFromSingleOption()
  {
    TestChooseBatches(new([AnyMockOption()]));
  }

  [TestMethod]
  public void InferenceChainForChooseRewardedAsOnePiece()
  {
    var Category = new MockCategory([
      AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(),
      AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(), AnyMockOption(),
      AnyMockOption()
    ]);

    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);
    var ArgumentA = Any.Float;
    var Argument2 = Any.Float;
    var AThirdArgument = Any.Float;

    var SelectionLog =
      new List<(CognitiveOption<MockSelectable, MockDescriptor> Left, CognitiveOption<MockSelectable, MockDescriptor>
        Right, MockInference<StatelessMind.Input, StatelessMind.Output> Inference)>();

    Brain.MakeInferenceFunc = Input =>
    {
      var Cat = Input.Parameters.ChooseItems.Category;
      var Output = new StatelessMind.Output
      {
        Parameters =
        {
          ChooseItems =
          {
            Category =
            {
              RightIsWinner = Any.Bool
            }
          }
        }
      };

      var MockInference = new MockInference<StatelessMind.Input, StatelessMind.Output>(Output);
      SelectionLog.Add((Category.AllOptions.Single(C => Equals(C.Descriptor, Cat.Left)),
        Category.AllOptions.Single(C => Equals(C.Descriptor, Cat.Right)), MockInference));

      return MockInference;
    };
    var T = Mind.ChooseItems(Category, ArgumentA, Argument2, AThirdArgument);
    var Selected = Any.Of(Category.AllOptions);
    T.RaiseAnyExceptions();

    T.Feedback.SelectionShouldHaveBeen(Selected.Payload);

    var Offset = StatelessMind.Output.ParametersIndex + StatelessMind.Output.OutputParameters.ChooseItemsIndex +
                 StatelessMind.Output.OutputParameters.ChooseItemsParameters.CategoryIndex;

    var LeftItems = SelectionLog.Where(I => I.Left == Selected);
    var RightItems = SelectionLog.Where(I => I.Right == Selected);
    foreach (var Item in LeftItems)
      Item.Inference.ShouldHaveBeenTrainedWith(
        new MockCategory.Output {RightIsWinner = false}.ExtractLossRules(Offset));
    foreach (var Item in RightItems)
      Item.Inference.ShouldHaveBeenTrainedWith(new MockCategory.Output {RightIsWinner = true}.ExtractLossRules(Offset));
    foreach (var Item in SelectionLog.Except(LeftItems).Except(RightItems))
      Item.Inference.ShouldNotHaveBeenTrained();
  }

  static CognitiveOption<MockSelectable, MockDescriptor> AnyMockOption()
  {
    return new(new(), new() {P1 = Any.Float, P2 = Any.Float});
  }

  static void TestChooseBatches(MockCategory Category)
  {
    var Selected = Any.Of(Category.AllOptions);
    var ArgumentA = Any.Float;
    var Argument2 = Any.Float;
    var AThirdArg = Any.Float;
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    Brain.MakeInferenceFunc = MakeInferenceFunction;

    var Mind = new StatelessMind(Brain);

    var Result = Mind.ChooseItems(Category, ArgumentA, Argument2, AThirdArg).ConsumeDetached();

    Result.Should().BeSameAs(Selected.Payload);

    Inference MakeInferenceFunction(StatelessMind.Input Input)
    {
      Input.OperationCode.Should().Be(4);
      Input.Parameters.ChooseItems.ArgumentA.Should().Be(ArgumentA);
      Input.Parameters.ChooseItems.Argument2.Should().Be(Argument2);
      Input.Parameters.ChooseItems.AThirdArg.Should().Be(AThirdArg);
      var RightOption =
        Category.AllOptions.Single(C => Equals(C.Descriptor, Input.Parameters.ChooseItems.Category.Right));

      var Output = new StatelessMind.Output();
      Output.Parameters.ChooseItems.Category.RightIsWinner = RightOption == Selected;

      return new MockInference<StatelessMind.Input, StatelessMind.Output>(Output)
      {
        MakeInferenceFunc =
          _ => throw new InvalidOperationException("Should not get here.")
      };
    }
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
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var ExpectedInput = new StatelessMind.Input
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

    var StipulatedOutput = new StatelessMind.Output
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
    var Thought = new StatelessMind(Brain).SynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument1,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument2);
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

  static async Task<bool> ExecuteAsynchronousUseOperation(AsynchronousActionSurface.Output Selection,
    MockAsynchronousSurface Surface)
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var ExpectedInput = new StatelessMind.Input
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

    var StipulatedOutput = new StatelessMind.Output
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
    var Thought = await new StatelessMind(Brain).AsynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.AsynchronousUseSomeInterface.Argument1,
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
      return Thought.WithoutFeedback.Do(_ => { this.SomeOtherData = SomeOtherData; });
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
      return Task.FromResult(Thought.WithoutFeedback.Do(_ => { this.SomeOtherData = SomeOtherData; }));
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
    public partial Thought<SimpleOutputData, MakeFeedback<SimpleOutputData>> MakeSimpleOutput(SimpleInputData Simple1);
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
  partial class StatelessMind
  {
    [Make]
    public partial Thought<SimpleOutputData, MakeFeedback<SimpleOutputData>> MakeSimpleOutput(SimpleInputData Simple1);

    [Use]
    public partial Thought<bool, UseFeedback<SynchronousActionSurface>> SynchronousUseSomeInterface(
      SynchronousActionSurface Surface,
      int Argument1,
      int Argument2);

    [Use]
    public partial Task<Thought<bool, AsyncUseFeedback<AsynchronousActionSurface>>> AsynchronousUseSomeInterface(
      AsynchronousActionSurface Surface,
      int Argument1,
      int Argument2);

    [Choose]
    public partial Thought<MockSelectable, ChooseFeedback<MockSelectable>> ChooseItems(MockCategory Category,
      float ArgumentA, float Argument2,
      float AThirdArg);
  }

  class MockSelectable;

  [CognitiveData]
  partial record MockDescriptor
  {
    public float P1 { get; set; }
    public float P2 { get; set; }

    public static MockDescriptor Random => new()
    {
      P1 = Any.Float,
      P2 = Any.Float
    };
  }

  [CognitiveCategory<MockSelectable, MockDescriptor>(3)]
  partial class MockCategory
  {
  }
}