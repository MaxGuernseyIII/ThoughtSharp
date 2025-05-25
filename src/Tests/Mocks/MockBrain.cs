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

using FluentAssertions;
using ThoughtSharp.Runtime;

namespace Tests.Mocks;

class MockBrain : MockDisposable, Brain
{
  public List<MockInference> MockInferences = [];
  public Func<float[], Inference> MakeInferenceFunc;

  readonly int InputLength;

  public MockBrain(int InputLength, int OutputLength)
  {
    this.InputLength = InputLength;
    MakeInferenceFunc = _ =>
    {
      var MockInference = new MockInference(new float[OutputLength]);
      MockInferences.Add(MockInference);
      return MockInference;
    };
  }

  public Inference MakeInference(float[] Parameters)
  {
    Parameters.Length.Should().Be(InputLength);

    var Result = MakeInferenceFunc(Parameters);

    return Result;
  }

  public void SetOutputForOnlyInput<TInput, TOutput>(TInput ExpectedInput, TOutput StipulatedOutput)
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    SetOutputsForInputs([(ExpectedInput, StipulatedOutput)]);
  }

  public void SetOutputsForInputs<TInput, TOutput>(IReadOnlyList<(TInput ExpectedInput, TOutput StipulatedOutput)> Sequence)
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    var Queue = new Queue<(TInput ExpectedInput, TOutput StipulatedOutput)>(Sequence);

    MakeInferenceFunc = Parameters =>
    {
      var (ExpectedInput, StipulatedOutput) = Queue.Dequeue();
      var ExpectedBrainInput = MakeReferenceFloats(ExpectedInput);
      var StipulatedBrainOutput = MakeReferenceFloats(StipulatedOutput);

      Parameters.Should().BeEquivalentTo(ExpectedBrainInput);

      return new MockInference(StipulatedBrainOutput);
    };

    static float[] MakeReferenceFloats<T>(T ToPersist) where T : CognitiveData<T>
    {
      var Result = new float[T.Length];

      ToPersist.MarshalTo(Result);

      return Result;
    }
  }
}