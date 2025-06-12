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
    MakeInferenceFunc = _ =>
    {
      var MockInference = new MockInference<TInput, TOutput>(new());
      MockInferences.Add(MockInference);
      return MockInference;
    };
  }

  public Func<ImmutableArray<TInput>, Inference> MakeInferenceFunc;
  public List<MockInference<TInput, TOutput>> MockInferences = [];

  public Inference MakeInference(float[][] Parameters)
  {
    var Inputs = Parameters.Select(StepInput =>
    {
      StepInput.Length.Should().Be(TInput.Length);
      var Input = TInput.UnmarshalFrom(StepInput);
      return Input;
    }).ToImmutableArray();

    var Result = MakeInferenceFunc(Inputs);

    return Result;
  }

  public MockInference<TInput, TOutput> SetOutputForOnlyInput(ImmutableArray<TInput> ExpectedInput, TOutput StipulatedOutput)
  {
    var ResultInference = new MockInference<TInput, TOutput>(StipulatedOutput);

    MakeInferenceFunc = ActualInput =>
    {
      AssertEx.AssertJsonDiff(ExpectedInput, ActualInput);
      
      return ResultInference;
    };

    return ResultInference;
  }
}