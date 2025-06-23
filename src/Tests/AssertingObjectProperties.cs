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
using Tests.Mocks;
using ThoughtSharp.Runtime;
using Assert = ThoughtSharp.Scenarios.Assert;

namespace Tests;

[TestClass]
public class AssertingObjectProperties : AssertingBase<MockData, MockData>
{
  [TestInitialize]
  public void SetUp()
  {
    Payload = new() { Value1 = Any.Float, Value2 = Any.Int() };
  }

  [TestMethod]
  public void AssertOnePropertyPass()
  {
    var R = GivenCognitiveResult();
    var Expected = Payload with {Value2 = Any.Int()};

    WhenAssertValue1InObject(R, Expected);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([Expected]);
  }

  [TestMethod]
  public void AssertOnePropertyFail()
  {
    var R = GivenCognitiveResult();
    var Expected = Payload with {Value1 = Any.FloatOutsideOf(Payload.Value1, .01f)};

    WhenAssertValue1InObject(R, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected.Value1} but found {Payload.Value1}");
    ThenModelWasTrainedWith([Expected]);
  }

  [TestMethod]
  public void AssertBothPropertiesPass()
  {
    var R = GivenCognitiveResult();

    WhenAssertBothPropertiesInObject(R, Payload);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([Payload]);
  }

  [TestMethod]
  public void AssertBothPropertiesFail()
  {
    var R = GivenCognitiveResult();
    var Expected = Payload with {Value2 = Any.IntOtherThan(Payload.Value2)};

    WhenAssertBothPropertiesInObject(R, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected.Value2} but found {Payload.Value2}");
    ThenModelWasTrainedWith([Expected]);
  }

  void WhenAssertValue1InObject(CognitiveResult<MockData, MockData> R, MockData Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(Expectation, C => C
      .Expect(D => D.Value1, Assert.ShouldBe)
    ));
  }

  void WhenAssertBothPropertiesInObject(CognitiveResult<MockData, MockData> R, MockData Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(Expectation, C => C
      .Expect(D => D.Value1, Assert.ShouldBe)
      .Expect(D => D.Value2, Assert.ShouldBe)
    ));
  }
}