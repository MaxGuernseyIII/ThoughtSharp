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
using FluentAssertions;
using ThoughtSharp.Runtime;

namespace Tests.Mocks;

class BatchTerminal(Batch<TensorData> Features)
{
  public Batch<TensorData> Features { get; } = Features;
}

class MockInferenceSource<TInput, TOutput> : MockDisposable, InferenceSource
  where TInput : CognitiveData<TInput>
  where TOutput : CognitiveData<TOutput>, new()
{
  public Func<ImmutableArray<ImmutableArray<TInput>>, Inference> MakeInferenceFunc;
  public List<MockInference<TInput, TOutput>> MockInferences = [];

  public MockInferenceSource()
  {
    MakeInferenceFunc = Batch =>
    {
      var MockInference = new MockInference<TInput, TOutput>([..Enumerable.Repeat(new TOutput(), Batch.Length)]);
      MockInferences.Add(MockInference);
      return MockInference;
    };
  }

  public Inference MakeInference(Batch<TensorData> Features)
  {
    var InputSequences = new List<ImmutableArray<TInput>>();

    for (var TerminalNumber = 0; TerminalNumber < Features.TerminalCount; ++TerminalNumber)
    {
      var FeaturesSequences = GetTerminal(Features).Features.Sequences;
      for (var Index = 0; Index < FeaturesSequences.Length; Index++)
      {
        var Timeline = FeaturesSequences[Index];
        var Inputs = new List<TInput>();

        for (var I = 0; I < Timeline.Steps.Count; I++)
        {
          var StepInput = Timeline.Steps[I];
          StepInput.Features.Length.Should().Be(TInput.FloatLength);
          StepInput.Tokens.Length.Should().Be(TInput.EncodedTokenClassCounts.Length);
          var Input = TInput.UnmarshalFrom(StepInput.Features, StepInput.Tokens);
          Inputs.Add(Input);
        }

        InputSequences.Add([..Inputs]);
      }
    }

    var Result = MakeInferenceFunc([.. InputSequences]);

    return Result;
  }

  static BatchTerminal GetTerminal(Batch<TensorData> Features)
  {
    return new BatchTerminal(Features);
  }

  static ImmutableArray<Batch<TensorData>.Sequence> _(BatchTerminal BatchTerminal)
  {
    var Features = BatchTerminal.Features;
    return Features.Sequences;
  }

  public MockInference<TInput, TOutput> SetOutputForOnlyInput(
    ImmutableArray<TInput> ExpectedInput,
    TOutput StipulatedOutput)
  {
    return SetOutputsForBatchedInputs([ExpectedInput], [StipulatedOutput]);
  }

  public MockInference<TInput, TOutput> SetOutputsForBatchedInputs(
    ImmutableArray<ImmutableArray<TInput>> ExpectedInputs,
    ImmutableArray<TOutput> StipulatedOutputs)
  {
    var ResultInference = new MockInference<TInput, TOutput>(StipulatedOutputs);

    MakeInferenceFunc = ActualInputs =>
    {
      AssertEx.AssertJsonDiff(ExpectedInputs, ActualInputs);

      return ResultInference;
    };

    return ResultInference;
  }
}
