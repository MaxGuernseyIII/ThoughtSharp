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

namespace Tests;

[TestClass]
public class BatchBuilding
{
  Batch<float>.Builder BatchBuilder = null!;
  Batch<float> Batch = null!;

  [TestInitialize]
  public void SetUp()
  {
    BatchBuilder = new();
  }

  [TestMethod]
  public void AddValueToSequence()
  {
    var SequenceCount = GivenSequences();
    var StepCount = Any.Int(0, 3);
    var Value = Any.Float;
    GivenSequence(
      GivenStepsInSequence(StepCount),
      GivenStepInSequence(Value)
      );

    WhenBuild();

    ThenBatchElementIs(SequenceCount, StepCount, Value);
  }

  static Func<Batch<float>.Builder.SequenceBuilder, Batch<float>.Builder.SequenceBuilder> GivenStepsInSequence(int Count)
  {
    return S =>
    {
      foreach (var I in Enumerable.Range(0, Count))
        S = GivenStepInSequence(Any.Float)(S);

      return S;
    };
  }

  int GivenSequences()
  {
    var Count = Any.Int(0, 3);
    foreach (var I in Enumerable.Range(0, Count)) 
      GivenSequence();

    return Count;
  }

  void GivenSequence(params IEnumerable<Func<Batch<float>.Builder.SequenceBuilder, Batch<float>.Builder.SequenceBuilder>> Configs)
  {
    BatchBuilder = BatchBuilder.AddSequence(S =>
    {
      return Configs.Aggregate(S, (Current, Config) => Config(Current));
    });
  }

  static Func<Batch<float>.Builder.SequenceBuilder, Batch<float>.Builder.SequenceBuilder> GivenStepInSequence(float Value)
  {
    return S => S.AddStep(Value);
  }

  void WhenBuild()
  {
    Batch = BatchBuilder.Build();
  }

  void ThenBatchElementIs(int SequenceNumber, int StepNumber, float Expected)
  {
    Batch.Sequences[SequenceNumber].Steps[StepNumber].Should().Be(Expected);
  }
}