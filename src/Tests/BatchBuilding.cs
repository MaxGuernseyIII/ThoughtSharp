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
    var S = GivenSequence();
    var StepCount = GivenStepsInSequence(S);
    var Value = GivenStepInSequence(S);

    WhenBuild();

    ThenBatchElementIs(SequenceCount, StepCount, Value);
  }

  int GivenStepsInSequence(Batch<float>.Builder.SequenceBuilder SequenceBuilder)
  {
    var Count = Any.Int(0, 3);
    foreach (var I in Enumerable.Range(0, Count))
      GivenStepInSequence(SequenceBuilder);

    return Count;
  }

  int GivenSequences()
  {
    var Count = Any.Int(0, 3);
    foreach (var I in Enumerable.Range(0, Count)) 
      GivenSequence();

    return Count;
  }

  Batch<float>.Builder.SequenceBuilder GivenSequence()
  {
    return BatchBuilder.AddStep();
  }

  float GivenStepInSequence(Batch<float>.Builder.SequenceBuilder SequenceBuilder)
  {
    var Result = Any.Float;
    SequenceBuilder.AddStep(Result);

    return Result;
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