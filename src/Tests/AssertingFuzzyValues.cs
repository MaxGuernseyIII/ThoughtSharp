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
using ThoughtSharp.Scenarios;
using Assert = ThoughtSharp.Scenarios.Assert;

namespace Tests;

[TestClass]
public class AssertingFuzzyValues : AssertingBase<float, float>
{
  float Epsilon;
  float TrainingValue;

  [TestInitialize]
  public void SetUp()
  {
    Payload = Any.Float;
    Epsilon = Any.Float;
    TrainingValue = Any.Float;
  }

  [TestMethod]
  public void PassedAssertApproximately()
  {
    var R = GivenCognitiveResult();

    WhenAssertThatResultIsApproximately(R, Payload, Epsilon);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([Payload]);
  }

  [TestMethod]
  public void FailedAssertApproximately()
  {
    var R = GivenCognitiveResult();
    var Expected = Any.FloatOutsideOf(Payload, Epsilon);

    WhenAssertThatResultIsApproximately(R, Expected, Epsilon);

    ThenAssertionThrewAssertionFailedException($"Expected {Expected}±{Epsilon} but found {Payload}");
    ThenModelWasTrainedWith([Expected]);
  }

  [TestMethod]
  public void PassedAssertMoreThan()
  {
    var R = GivenCognitiveResult();

    float Expectation = Any.FloatLessThan(Payload);
    WhenAssertThatResultIsMoreThan(R, TrainingValue, Expectation);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void FailedAssertMoreThan()
  {
    var R = GivenCognitiveResult();
    var Expected = Any.FloatGreaterThanOrEqualTo(Payload);

    WhenAssertThatResultIsMoreThan(R, TrainingValue, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected >{Expected} but found {Payload}");
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void PassedAssertAtLeast()
  {
    var R = GivenCognitiveResult();
    float Expectation = Any.FloatLessThanOrEqualTo(Payload);

    WhenAssertThatResultIsAtLeast(R, TrainingValue, Expectation);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void FailedAssertAtLeast()
  {
    var R = GivenCognitiveResult();
    var Expected = Any.FloatGreaterThan(Payload);

    WhenAssertThatResultIsAtLeast(R, TrainingValue, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected >={Expected} but found {Payload}");
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void PassedAssertLessThan()
  {
    var R = GivenCognitiveResult();

    float Expectation = Any.FloatGreaterThan(Payload);
    WhenAssertThatResultIsLessThan(R, TrainingValue, Expectation);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void FailedAssertLessThan()
  {
    var R = GivenCognitiveResult();
    var Expected = Any.FloatLessThanOrEqualTo(Payload);

    WhenAssertThatResultIsLessThan(R, TrainingValue, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected <{Expected} but found {Payload}");
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void PassedAssertAtMost()
  {
    var R = GivenCognitiveResult();
    float Expectation = Any.FloatGreaterThanOrEqualTo(Payload);

    WhenAssertThatResultIsAtMost(R, TrainingValue, Expectation);

    ThenAssertionDidNotThrowAnException();
    ThenModelWasTrainedWith([TrainingValue]);
  }

  [TestMethod]
  public void FailedAssertAtMost()
  {
    var R = GivenCognitiveResult();
    var Expected = Any.FloatLessThan(Payload);

    WhenAssertThatResultIsAtMost(R, TrainingValue, Expected);

    ThenAssertionThrewAssertionFailedException($"Expected <={Expected} but found {Payload}");
    ThenModelWasTrainedWith([TrainingValue]);
  }

  void WhenAssertThatResultIsApproximately(CognitiveResult<float, float> R, float Expectation, float Epsilon)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(Expectation, V => V.ShouldBeApproximately(Expectation, Epsilon)));
  }

  void WhenAssertThatResultIsMoreThan(CognitiveResult<float, float> R, float TrainingValue, float Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(TrainingValue, V => V.ShouldBeMoreThan(Expectation)));
  }

  void WhenAssertThatResultIsAtLeast(CognitiveResult<float, float> R, float TrainingValue, float Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(TrainingValue, V => V.ShouldBeAtLeast(Expectation)));
  }

  void WhenAssertThatResultIsLessThan(CognitiveResult<float, float> R, float TrainingValue, float Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(TrainingValue, V => V.ShouldBeLessThan(Expectation)));
  }

  void WhenAssertThatResultIsAtMost(CognitiveResult<float, float> R, float TrainingValue, float Expectation)
  {
    Action = FluentActions.Invoking(() => Assert.That(R).Is(TrainingValue, V => V.ShouldBeAtMost(Expectation)));
  }
}
