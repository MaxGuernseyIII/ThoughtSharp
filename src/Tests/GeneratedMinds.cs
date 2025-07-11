﻿// MIT License
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
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;

namespace Tests;

[TestClass]
public partial class GeneratedMinds
{
  [TestMethod]
  public void DoTimeAwareMake()
  {
    var Brain = new MockBrain<HasTimeStepsMind.Input, HasTimeStepsMind.Output>();
    var Mind = new HasTimeStepsMind(Brain);
    var InputToMakeCall = Any.FloatArray();
    var ExpectedOutput = new SimpleOutputData
    {
      R1 = Any.Float
    };
    Brain.SetOutputForOnlyInput(
      [
        ..InputToMakeCall.Select(Step => new HasTimeStepsMind.Input
        {
          OperationCode = 1,
          Parameters =
          {
            TimeAwareMake =
            {
              Params = Step
            }
          }
        })
      ],
      new()
      {
        Parameters =
        {
          TimeAwareMake =
          {
            Value = ExpectedOutput
          }
        }
      });

    var Actual = Mind.TimeAwareMake(InputToMakeCall).Payload;

    Actual.Should().BeEquivalentTo(ExpectedOutput);
  }

  [TestMethod]
  public void DoBatchTimeAwareMake()
  {
    var Brain = new MockBrain<HasTimeStepsMind.Input, HasTimeStepsMind.Output>();
    var Mind = new HasTimeStepsMind(Brain);
    var InputToMakeCallBatch0 = Any.FloatArray();
    var InputToMakeCallBatch1 = Any.FloatArray();
    var ExpectedOutput0 = new SimpleOutputData
    {
      R1 = Any.Float
    };
    var ExpectedOutput1 = new SimpleOutputData
    {
      R1 = Any.Float
    };
    Brain.SetOutputsForBatchedInputs(
      [
        [
          ..InputToMakeCallBatch0.Select(Step => new HasTimeStepsMind.Input
          {
            OperationCode = 1,
            Parameters =
            {
              TimeAwareMake =
              {
                Params = Step
              }
            }
          })
        ],
        [
          ..InputToMakeCallBatch1.Select(Step => new HasTimeStepsMind.Input
          {
            OperationCode = 1,
            Parameters =
            {
              TimeAwareMake =
              {
                Params = Step
              }
            }
          })
        ]
      ],
      [
        new()
        {
          Parameters =
          {
            TimeAwareMake =
            {
              Value = ExpectedOutput0
            }
          }
        },
        new()
        {
          Parameters =
          {
            TimeAwareMake =
            {
              Value = ExpectedOutput1
            }
          }
        }
      ]);

    var Actual = Mind.TimeAwareMakeBatch([new(InputToMakeCallBatch0), new(InputToMakeCallBatch1)]).Payload;

    Actual.Should().BeEquivalentTo([ExpectedOutput0, ExpectedOutput1]);
  }

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
    Brain.SetOutputForOnlyInput([
        new()
        {
          OperationCode = 1,
          Parameters =
          {
            MakeSimpleOutput =
            {
              Simple1 = InputToMakeCall
            }
          }
        }
      ],
      new()
      {
        Parameters =
        {
          MakeSimpleOutput =
          {
            Value = ExpectedOutput
          }
        }
      });

    var Actual = Mind.MakeSimpleOutput(InputToMakeCall).Payload;

    Actual.Should().BeEquivalentTo(ExpectedOutput);
  }

  [TestMethod]
  public void DoMakeWithTokens()
  {
    var Brain = new MockBrain<TokenMindWithAllOperations.Input, TokenMindWithAllOperations.Output>();
    var Mind = new TokenMindWithAllOperations(Brain);
    var InputToMakeCall = new TokenBucket()
    {
      Token = Any.Long
    };
    var ExpectedOutput = new TokenBucket()
    {
      Token = Any.Long
    };
    Brain.SetOutputForOnlyInput([
        new()
        {
          OperationCode = 1,
          Parameters =
          {
            MakeTokenBucket =
            {
              Inputs = InputToMakeCall
            }
          }
        }
      ],
      new()
      {
        Parameters =
        {
          MakeTokenBucket =
          {
            Value = ExpectedOutput
          }
        }
      });

    var Actual = Mind.MakeTokenBucket(InputToMakeCall).Payload;

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

    T.FeedbackSink.TrainWith(ExpectedObject);

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith([
      new StatelessMind.Output
      {
        Parameters =
        {
          MakeSimpleOutput =
          {
            Value = ExpectedObject
          }
        }
      }
    ]);
  }

  [TestMethod]
  public void TrainBatchMake()
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);
    var T = Mind.MakeSimpleOutputBatch([new(new()), new(new())]);
    var ExpectedObject1 = new SimpleOutputData
    {
      R1 = Any.Float
    };

    var ExpectedObject2 = new SimpleOutputData
    {
      R1 = Any.Float
    };

    T.FeedbackSink.TrainWith([ExpectedObject1, ExpectedObject2]);

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith([
      new StatelessMind.Output
      {
        Parameters =
        {
          MakeSimpleOutput =
          {
            Value = ExpectedObject1
          }
        }
      },
      new StatelessMind.Output
      {
        Parameters =
        {
          MakeSimpleOutput =
          {
            Value = ExpectedObject2
          }
        }
      }
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
  public void UseActionWithTokenTriggersOperation1()
  {
    var ExpectedSomeData = Any.Float;

    TestSynchronousUseMethodWithTokens(new()
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

    T.FeedbackSink.TrainWith((Mock, More) =>
    {
      Mock.DoSomething2(SomeOtherData);
      More.Value = ExpectedMore;
    });

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith([
      new StatelessMind.Output
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
      }
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

  [TestMethod]
  public async Task UseAsynchronousActionSurfaceOperationsInBatch()
  {
    var ExpectedSomeData = Any.Float;
    var ExpectedSomeOtherDataData = Any.Float;

    await TestAsynchronousBatchUseMethod([
      (new()
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
      }, ExpectedSomeData, null),
      (new()
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
      }, null, ExpectedSomeOtherDataData)
    ]);
  }

  [DataRow(true, true)]
  [DataRow(false, true)]
  [DataRow(true, false)]
  [DataRow(false, false)]
  [TestMethod]
  public async Task UseAsynchronousActionSurfaceInBatchReturnsMore(bool ExpectedMore1, bool ExpectedMore2)
  {
    var Surface = new MockAsynchronousSurface();
    var ActualMore = await ExecuteAsynchronousUseBatchOperation([
      (new()
      {
        ActionCode = (ushort) Any.Int(0, 2),
        MoreActions = ExpectedMore1
      }, Surface),
      (new()
      {
        ActionCode = (ushort) Any.Int(0, 2),
        MoreActions = ExpectedMore2
      }, Surface)
    ]);

    ActualMore.Should().BeEquivalentTo([ExpectedMore1, ExpectedMore2], C => C.WithStrictOrdering());
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

    T.FeedbackSink.TrainWith((Mock, More) =>
    {
      Mock.DoSomething2(SomeOtherData);
      More.Value = ExpectedMore;
      return Task.CompletedTask;
    });

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith([
      new StatelessMind.Output
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
      }
    ]);
  }

  [TestMethod]
  public async Task TrainingOfAsynchronousBatchUseAsOutputThought()
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var Mind = new StatelessMind(Brain);

    var T = await Mind.AsynchronousUseSomeInterfaceBatch(
      [
        new(new MockAsynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100)),
        new(new MockAsynchronousSurface(), Any.Int(0, 10), Any.Int(-100, 100)),
      ]);

    var ExpectedMore1 = Any.Bool;
    var ExpectedMore2 = Any.Bool;
    var SomeOtherData1 = Any.Float;
    var SomeData2 = Any.Float;

    T.FeedbackSink.TrainWith([
        (Mock, More) =>
        {
          Mock.DoSomething2(SomeOtherData1);
          More.Value = ExpectedMore1;
          return Task.CompletedTask;
        },
        (Mock, More) =>
        {
          Mock.DoSomething1(SomeData2);
          More.Value = ExpectedMore2;
          return Task.CompletedTask;
        }
      ]);

    var Inference = Brain.MockInferences.Single();
    Inference.ShouldHaveBeenTrainedWith([
      new StatelessMind.Output
      {
        Parameters =
        {
          AsynchronousUseSomeInterface =
          {
            Surface =
            {
              ActionCode = 2,
              MoreActions = ExpectedMore1,
              Parameters =
              {
                DoSomething2 =
                {
                  SomeOtherData = SomeOtherData1
                }
              }
            }
          }
        }
      },
      new()
      {
        Parameters =
        {
          AsynchronousUseSomeInterface =
          {
            Surface =
            {
              ActionCode = 1,
              MoreActions = ExpectedMore2,
              Parameters =
              {
                DoSomething1 =
                {
                  SomeData = SomeData2
                }
              }
            }
          }
        }
      }
    ]);
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
  public void ChooseWithTokensFromTwoOptions()
  {
    TestChooseBatchesWithToken(new([AnyMockOption(), AnyMockOption()]));
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

    Brain.MakeInferenceFunc = Batches =>
    {
      Batches.Length.Should().Be(1);
      var Inputs = Batches[0];
      Inputs.Length.Should().Be(1);
      var Input = Inputs.Single();
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

    T.FeedbackSink.TrainWith(Selected.Payload);

    var Offset = StatelessMind.Output.ParametersIndex + StatelessMind.Output.OutputParameters.ChooseItemsIndex +
                 StatelessMind.Output.OutputParameters.ChooseItemsParameters.CategoryIndex;

    var LeftItems = SelectionLog.Where(I => I.Left == Selected);
    var RightItems = SelectionLog.Where(I => I.Right == Selected);
    foreach (var Item in LeftItems)
      Item.Inference.ShouldHaveBeenTrainedWith(
        new MockCategory.Output {RightIsWinner = false}.ExtractLossRules(0, Offset));
    foreach (var Item in RightItems)
      Item.Inference.ShouldHaveBeenTrainedWith(
        new MockCategory.Output {RightIsWinner = true}.ExtractLossRules(0, Offset));
    foreach (var Item in SelectionLog.Except(LeftItems).Except(RightItems))
      Item.Inference.ShouldNotHaveBeenTrained();
  }

  [TestMethod]
  public void TellMindSequence()
  {
    var Brain = new MockBrain<CanBeTold.Input, CanBeTold.Output>();
    var Mind = new CanBeTold(Brain);

    ImmutableArray<Token> Tokens =
    [
      new() {C1 = Any.Char, C2 = Any.Char},
      new() {C1 = Any.Char, C2 = Any.Char}
    ];

    Brain.SetOutputForOnlyInput(
      [
        new()
        {
          OperationCode = 0,
          Parameters =
          {
            Tell =
            {
              Tokens = Tokens[0]
            }
          }
        },
        new()
        {
          OperationCode = 0,
          Parameters =
          {
            Tell =
            {
              Tokens = Tokens[1]
            }
          }
        }
      ],
      new());

    Mind.Tell(Tokens);
  }

  [TestMethod]
  public void TellOnlyMakesOneInferenceNoMatterHowManyItemsAreGiven()
  {
    var Brain = new MockBrain<CanBeTold.Input, CanBeTold.Output>();
    var Mind = new CanBeTold(Brain);

    ImmutableArray<Token> Tokens = [..Any.ListOf(() => new Token {C1 = Any.Char, C2 = Any.Char}, 1, 5)];

    var CallCount = 0;

    Brain.MakeInferenceFunc = delegate
    {
      CallCount++;
      return new MockInference<CanBeTold.Input, CanBeTold.Output>([new()]);
    };

    Mind.Tell(Tokens);

    CallCount.Should().Be(1);
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

    var Result = Mind.ChooseItems(Category, ArgumentA, Argument2, AThirdArg).Payload;

    Result.Should().BeSameAs(Selected.Payload);
    return;

    Inference MakeInferenceFunction(ImmutableArray<ImmutableArray<StatelessMind.Input>> Batches)
    {
      Batches.Length.Should().Be(1);
      var Inputs = Batches[0];
      Inputs.Length.Should().Be(1);
      var Input = Inputs.Single();
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

  static void TestChooseBatchesWithToken(MockCategory Category)
  {
    var Selected = Any.Of(Category.AllOptions);
    var ArgumentA = Any.Long;
    var Brain = new MockBrain<TokenMindWithAllOperations.Input, TokenMindWithAllOperations.Output>();
    Brain.MakeInferenceFunc = MakeInferenceFunction;

    var Mind = new TokenMindWithAllOperations(Brain);

    var Result = Mind.ChooseWithTokenBucket(Category, new() { Token = ArgumentA}).Payload;

    Result.Should().BeSameAs(Selected.Payload);
    return;

    Inference MakeInferenceFunction(ImmutableArray<ImmutableArray<TokenMindWithAllOperations.Input>> Batches)
    {
      Batches.Length.Should().Be(1);
      var Inputs = Batches[0];
      Inputs.Length.Should().Be(1);
      var Input = Inputs.Single();
      Input.OperationCode.Should().Be(3);
      Input.Parameters.ChooseWithTokenBucket.Inputs.Token.Should().Be(ArgumentA);
      var RightOption =
        Category.AllOptions.Single(C => Equals(C.Descriptor, Input.Parameters.ChooseWithTokenBucket.Category.Right));

      var Output = new TokenMindWithAllOperations.Output();
      Output.Parameters.ChooseWithTokenBucket.Category.RightIsWinner = RightOption == Selected;

      return new MockInference<TokenMindWithAllOperations.Input, TokenMindWithAllOperations.Output>(Output)
      {
        MakeInferenceFunc =
          _ => throw new InvalidOperationException("Should not get here.")
      };
    }
  }

  static void TestSynchronousUseMethodWithTokens(SynchronousActionSurface.Output Selection, float? ExpectedSomeData,
    float? ExpectedSomeOtherData)
  {
    var Surface = new MockSynchronousSurface();
    var Brain = new MockBrain<TokenMindWithAllOperations.Input, TokenMindWithAllOperations.Output>();
    var ExpectedInput = new TokenMindWithAllOperations.Input
    {
      OperationCode = 2,
      Parameters =
      {
        UseWithTokenBucket = 
        {
          Inputs =
          {
            Token = Any.Int(0, 100)
          }
        }
      }
    };

    var StipulatedOutput = new TokenMindWithAllOperations.Output
    {
      Parameters =
      {
        UseWithTokenBucket =
        {
          ToUse = Selection
        }
      }
    };
    Brain.SetOutputForOnlyInput([ExpectedInput], StipulatedOutput);
    new TokenMindWithAllOperations(Brain).UseWithTokenBucket(
      ExpectedInput.Parameters.UseWithTokenBucket.Inputs,
      Surface);

    Surface.SomeData.Should().Be(ExpectedSomeData);
    Surface.SomeOtherData.Should().Be(ExpectedSomeOtherData);
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
    Brain.SetOutputForOnlyInput([ExpectedInput], StipulatedOutput);
    var Thought = new StatelessMind(Brain).SynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument1,
      ExpectedInput.Parameters.SynchronousUseSomeInterface.Argument2);
    var More = Thought.Payload;
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
    Brain.SetOutputForOnlyInput([ExpectedInput], StipulatedOutput);
    var Thought = await new StatelessMind(Brain).AsynchronousUseSomeInterface(Surface,
      ExpectedInput.Parameters.AsynchronousUseSomeInterface.Argument1,
      ExpectedInput.Parameters.AsynchronousUseSomeInterface.Argument2);
    var More = Thought.Payload;
    return More;
  }

  static async Task TestAsynchronousBatchUseMethod(ImmutableArray<(
    AsynchronousActionSurface.Output Selection,
    float? ExpectedSomeData,
    float? ExpectedSomeOtherData)> Actions
  )
  {
    var Operations = Actions.Select(A => (
      A.Selection,
      Surface: new MockAsynchronousSurface(),
      A.ExpectedSomeData,
      A.ExpectedSomeOtherData)).ToImmutableArray();

    await ExecuteAsynchronousUseBatchOperation([..Operations.Select(A => (A.Selection, A.Surface))]);

    foreach (var (_, Surface, ExpectedSomeData, ExpectedSomeOtherData) in Operations)
    {
      Surface.SomeData.Should().Be(ExpectedSomeData);
      Surface.SomeOtherData.Should().Be(ExpectedSomeOtherData);
    }
  }

  static async Task<IReadOnlyList<bool>> ExecuteAsynchronousUseBatchOperation(
    ImmutableArray<(
      AsynchronousActionSurface.Output Selection,
      MockAsynchronousSurface Surface)> Actions
  )
  {
    var Brain = new MockBrain<StatelessMind.Input, StatelessMind.Output>();
    var ExpectedInputs = new List<StatelessMind.Input>();
    var StipulatedOutputs = new List<StatelessMind.Output>();

    foreach (var (Selection, Surface) in Actions)
    {
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
      ExpectedInputs.Add(ExpectedInput);
      StipulatedOutputs.Add(StipulatedOutput);
    }

    Brain.SetOutputsForBatchedInputs([..ExpectedInputs.Select(I => new[] {I}.ToImmutableArray())],
      [..StipulatedOutputs]);
    var Thought = await new StatelessMind(Brain).AsynchronousUseSomeInterfaceBatch(
      [
        ..ExpectedInputs.Zip(Actions).Select(Pair => new StatelessMind.AsynchronousUseSomeInterfaceArgs(Pair.Second.Surface,
          Pair.First.Parameters.AsynchronousUseSomeInterface.Argument1,
          Pair.First.Parameters.AsynchronousUseSomeInterface.Argument2)
        )
      ]
    );
    return Thought.Payload;
  }

  [TestMethod]
  public void CognitiveDataIsolation()
  {
    var S = new IsolationBoundaryStream();
    var Offset = Any.Int(0, 1000);
    var W = new IsolationBoundariesWriter(S, Offset);

    HasIsolationBoundaries.WriteIsolationBoundaries(W);

    S.Boundaries.Should().BeEquivalentTo([
      Offset + HasIsolationBoundaries.P3Index,
      Offset + HasIsolationBoundaries.P3Index + HasIsolationBoundaries.P3Codec.FloatLength
    ]);
  }

  [TestMethod]
  public void MindOutputIsolationBoundaries()
  {
    var S = new IsolationBoundaryStream();
    var Offset = Any.Int(0, 1000);
    var W = new IsolationBoundariesWriter(S, Offset);
    var ExpectedStream = new IsolationBoundaryStream();
    UsesIsolationMind.Output.WriteIsolationBoundaries(new(ExpectedStream, Offset));

    UsesIsolationMind.WriteIsolationBoundaries(W);

    S.Boundaries.Should().BeEquivalentTo(ExpectedStream.Boundaries);
  }

  [Mind]
  public partial class HasTimeStepsMind
  {
    [Make]
    public partial CognitiveResult<SimpleOutputData, SimpleOutputData> TimeAwareMake([TimeSteps] float[] Params);
  }

  class MockSynchronousSurface : SynchronousActionSurface
  {
    public float? SomeData;
    public float? SomeOtherData;

    public void DoSomething1(float SomeData)
    {
      this.SomeData = SomeData;
    }

    public void DoSomething2(float SomeOtherData)
    {
      this.SomeOtherData = SomeOtherData;
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

    public Task DoSomething2(float SomeOtherData)
    {
      this.SomeOtherData = SomeOtherData;
      return Task.CompletedTask;
    }
  }

  [CognitiveData]
  public partial class HasIsolationBoundaries
  {
    public float P1;
    public float P2;

    [Isolated] public float P3;

    public float P4;
  }

  [Mind]
  public partial class UsesIsolationMind
  {
    [Make]
    public partial CognitiveResult<HasIsolationBoundaries, HasIsolationBoundaries> MakeIsolatedContent();
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
    public partial CognitiveResult<SimpleOutputData, SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);
    //public partial CognitiveData<SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);
  }

  [CognitiveActions]
  partial interface SynchronousActionSurface
  {
    void DoSomething1(float SomeData);
    void DoSomething2(float SomeOtherData);
  }

  [CognitiveActions]
  partial interface AsynchronousActionSurface
  {
    void DoSomething1(float SomeData);
    Task DoSomething2(float SomeOtherData);
  }

  [Mind]
  partial class StatelessMind
  {
    [Make]
    public partial CognitiveResult<SimpleOutputData, SimpleOutputData> MakeSimpleOutput(SimpleInputData Simple1);

    [Use]
    public partial CognitiveResult<bool, UseFeedbackMethod<SynchronousActionSurface>> SynchronousUseSomeInterface(
      SynchronousActionSurface Surface,
      int Argument1,
      int Argument2);

    [Use]
    public partial Task<CognitiveResult<bool, AsyncUseFeedbackMethod<AsynchronousActionSurface>>>
      AsynchronousUseSomeInterface(
        AsynchronousActionSurface Surface,
        int Argument1,
        int Argument2);

    [Choose]
    public partial CognitiveResult<MockSelectable, MockSelectable> ChooseItems(MockCategory Category,
      float ArgumentA, float Argument2,
      float AThirdArg);
  }

  [Mind]
  public partial class CanBeTold
  {
    [Tell]
    public partial void Tell(IEnumerable<Token> Tokens);
  }

  [CognitiveData]
  public partial class Token
  {
    public char C1;
    public char C2;
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
  partial class MockCategory;

  [TestMethod]
  public void TokenLayout()
  {
    TokenContainer.EncodedTokenClassCounts.Should().BeEquivalentTo([100, 900], O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void TokenLayoutForMind()
  {
    TokenUsingMind.EncodedTokenClassCounts.Should()
      .BeEquivalentTo(TokenUsingMind.Input.EncodedTokenClassCounts, O => O.WithStrictOrdering());
  }

  [CognitiveData]
  partial class TokenContainer
  {
    public long Token1;
    public static readonly TokenCodec<long> Token1Codec = new(100);

    public int Token2;
    public static readonly TokenCodec<int> Token2Codec = new(900);
  }

  [CognitiveData]
  public partial class DummyData
  {

  }

  [Mind]
  partial class TokenUsingMind
  {
    [Make]
    public partial CognitiveResult<DummyData, DummyData> Make(TokenContainer ContainsToken);
  }

  [CognitiveData]
  partial class TokenBucket
  {
    public long Token;

    public static readonly CognitiveDataCodec<long> TokenCodec = new TokenCodec<long>(3000);
  }

  [Mind]
  partial class TokenMindWithAllOperations
  {
    [Make]
    public partial CognitiveResult<TokenBucket, TokenBucket> MakeTokenBucket(TokenBucket Inputs);

    [Choose]
    public partial CognitiveResult<MockSelectable, MockSelectable> ChooseWithTokenBucket(MockCategory Category, TokenBucket Inputs);

    [Use]
    public partial CognitiveResult<bool, UseFeedbackMethod<SynchronousActionSurface>> UseWithTokenBucket(TokenBucket Inputs, SynchronousActionSurface ToUse);

    [Tell]
    public partial void TellWithTokenBucket(IEnumerable<TokenBucket> Inputs);
  }
}