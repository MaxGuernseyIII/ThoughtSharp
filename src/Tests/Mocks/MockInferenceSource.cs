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
using ThoughtSharp.Runtime;

namespace Tests.Mocks;

class MockInferenceSource : MockDisposable, InferenceSource
{
  public MockInferenceSource(int InputLength, int OutputLength)
  {
    this.InputLength = InputLength;
    MakeInferenceFunc = _ =>
    {
      var MockInference = new MockInference(InputLength, new float[OutputLength]);
      MockInferences.Add(MockInference);
      return MockInference;
    };
  }

  protected readonly int InputLength;
  public Func<float[], Inference> MakeInferenceFunc;
  public List<MockInference> MockInferences = [];

  public Inference MakeInference(float[] Parameters)
  {
    Parameters.Length.Should().Be(InputLength);

    var Result = MakeInferenceFunc(Parameters);

    return Result;
  }

  public MockInference SetOutputForOnlyInput<TInput, TOutput>(TInput ExpectedInput, TOutput StipulatedOutput)
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    var ExpectedBrainInput = MakeReferenceFloats(ExpectedInput);
    var StipulatedBrainOutput = MakeReferenceFloats(StipulatedOutput);
    var ResultInference = new MockInference(TInput.Length, StipulatedBrainOutput);

    MakeInferenceFunc = Parameters =>
    {
      Parameters.Should().BeEquivalentTo(ExpectedBrainInput);

      return ResultInference;
    };

    static float[] MakeReferenceFloats<T>(T ToPersist) where T : CognitiveData<T>
    {
      var Result = new float[T.Length];

      ToPersist.MarshalTo(Result);

      return Result;
    }

    return ResultInference;
  }

  public MockInference SetChainedOutputsForInputs<TInput, TOutput>(
    IReadOnlyList<(TInput ExpectedInput, TOutput StipulatedOutput)> Sequence)
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    var Source = this;
    MockInference Last = null!;

    foreach (var (Input, Output) in Sequence) 
      Source = Last = Source.SetOutputForOnlyInput(Input, Output);

    return Last;
  }
}