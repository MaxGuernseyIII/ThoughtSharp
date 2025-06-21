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
  float Default;
  int SequenceCount;
  Batch<float>.Builder BatchBuilder;
  Batch<float> Batch;
  Batch<float>.Builder.StepBuilder Step;

  [TestInitialize]
  public void SetUp()
  {
    Default = Any.Float;
    SequenceCount = Any.Int(1, 10);
    BatchBuilder = new(Default, SequenceCount);
  }

  [TestMethod]
  public void DefaultValue()
  {
    GivenStep();

    WhenBuild();

    ThenBatchElementIs(0, AnyValidSequenceNumber, Default);
  }

  [TestMethod]
  public void ExposesSequenceCount()
  {
    WhenBuild();

    ThenSequenceCountIs(SequenceCount);
  }

  [TestMethod]
  public void ExposesStepCount()
  {
    var Count = Any.Int(1, 9);
    GivenSteps(Count);

    WhenBuild();

    ThenStepCountIs(Count);
  }

  [TestMethod]
  public void AssignValues()
  {
    var Expected = Any.Float;
    var AffectedSequenceNumber = AnyValidSequenceNumber;
    GivenStep();
    GivenAssignmentInBuilder(AffectedSequenceNumber, Expected);

    WhenBuild();

    ThenBatchElementIs(0, AffectedSequenceNumber, Expected);
  }

  void GivenAssignmentInBuilder(int AffectedSequenceNumber, float Expected)
  {
    Step[AffectedSequenceNumber] = Expected;
  }

  void GivenStep()
  {
    Step = BatchBuilder.AddStep();
  }

  void GivenSteps(int Count)
  {
    foreach (var _ in Enumerable.Range(0, Count)) GivenStep();
  }

  void WhenBuild()
  {
    Batch = BatchBuilder.Build();
  }

  void ThenBatchElementIs(int StepNumber, int SequenceNumber, float Expected)
  {
    Batch.Time[StepNumber].InSequence[SequenceNumber].Should().Be(Expected);
  }

  void ThenSequenceCountIs(int Expected)
  {
    Batch.SequenceCount.Should().Be(Expected);
  }

  void ThenStepCountIs(int Expected)
  {
    Batch.StepCount.Should().Be(Expected);
  }

  int AnyValidSequenceNumber => Any.Int(0, SequenceCount - 1);
}