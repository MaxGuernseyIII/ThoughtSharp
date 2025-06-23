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

using DiffMatchPatch;
using FluentAssertions;
using ThoughtSharp.Runtime;
using Assert = ThoughtSharp.Scenarios.Assert;

namespace Tests;

[TestClass]
public class AssertingBatches : AssertingBase<IReadOnlyList<int>, IReadOnlyList<int>>
{
  IReadOnlyList<int> TrainingValue;

  [TestInitialize]
  public void SetUp()
  {
    TrainingValue = [Any.Int(), Any.Int()];
  }

  [TestMethod]
  public void AssertIsWithExactValues()
  {
    GivenPayloadIsConfiguredForBatches();
    var R = GivenCognitiveResult();

    WhenAssertResultIs(R, Payload);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([Payload]);
  }

  [TestMethod]
  public void AssertIsWithTooFewValues()
  {
    var NumberOfBatches = GivenPayloadIsConfiguredForBatches();
    var R = GivenCognitiveResult();
    var Expected = GivenExpectedListWithFewerItemsThan(NumberOfBatches);

    WhenAssertResultIs(R, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected.Count} batch(es) but found {Payload.Count}");
    ThenModelWasTrainedWith([Expected]);
  }

  [TestMethod]
  public void AssertIsWithTooManyValues()
  {
    var NumberOfBatches = GivenPayloadIsConfiguredForBatches();
    var R = GivenCognitiveResult();
    var Expected = GivenExpectedListWithMoreItemsThan(NumberOfBatches);

    WhenAssertResultIs(R, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected.Count} batch(es) but found {Payload.Count}");
    ThenModelWasTrainedWith([Expected]);
  }

  [TestMethod]
  public void AssertADifferingValue()
  {
    GivenPayloadIsConfiguredForBatches();
    var R = GivenCognitiveResult();
    var DifferingIndex = Any.Int(0, Payload.Count - 1);
    var Expected = GivenArrayWithOneDifferentValue(DifferingIndex);

    WhenAssertResultIs(R, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected[DifferingIndex]} but found {Payload[DifferingIndex]}");
    ThenModelWasTrainedWith([Expected]);
  }

  IReadOnlyList<int> GivenArrayWithOneDifferentValue(int DifferingIndex)
  {
    return [..Payload.Select((V, I) => I == DifferingIndex ? Any.IntOtherThan(V) : V)];
  }

  IReadOnlyList<int> GivenExpectedListWithFewerItemsThan(int NumberOfBatches)
  {
    return Any.ListOf(Any.Int, 0, NumberOfBatches - 1);
  }

  IReadOnlyList<int> GivenExpectedListWithMoreItemsThan(int NumberOfBatches)
  {
    return Any.ListOf(Any.Int, NumberOfBatches + 1, NumberOfBatches + 10);
  }

  int GivenPayloadIsConfiguredForBatches()
  {
    var NewPayload = Any.ListOf(Any.Int, 1, 3);
    Payload = NewPayload;

    return NewPayload.Count;
  }

  void WhenAssertResultIs(CognitiveResult<IReadOnlyList<int>, IReadOnlyList<int>> CognitiveResult, IReadOnlyList<int> Expected)
  {
    Action = FluentActions.Invoking(()=> Assert.That(CognitiveResult).Is(Expected));
  }
}