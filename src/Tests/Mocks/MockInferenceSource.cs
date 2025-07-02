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

class MockInferenceSource<TInput, TOutput> : MockDisposable, InferenceSource
  where TInput : CognitiveData<TInput>
  where TOutput : CognitiveData<TOutput>, new()
{
  public MockInferenceSource()
  {
    MakeInferenceFunc = Batch =>
    {
      var MockInference = new MockInference<TInput, TOutput>([..Enumerable.Repeat(new TOutput(), Batch.Length)]);
      MockInferences.Add(MockInference);
      return MockInference;
    };
  }

  public Func<ImmutableArray<ImmutableArray<TInput>>, Inference> MakeInferenceFunc;
  public List<MockInference<TInput, TOutput>> MockInferences = [];

  public Inference MakeInference(Batch<float[]> Features, Batch<long[]> Tokens)
  {
    var Inputs = Features.Sequences.Select(Timeline =>
    {
      return Timeline.Steps.Select(StepInput =>
      {
        StepInput.Length.Should().Be(TInput.FloatLength);
        var Input = TInput.UnmarshalFrom(StepInput);
        return Input;
      }).ToImmutableArray();
    }).ToImmutableArray();

    var Result = MakeInferenceFunc(Inputs);

    return Result;
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