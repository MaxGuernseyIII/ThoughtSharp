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

using ThoughtSharp.Runtime;
using FluentAssertions;
using Tests.Mocks;

namespace Tests;

[TestClass]
public partial class CognitiveCategoryTests
{
  [TestMethod]
  public void GenerateInputForSelection()
  {
    var Left = new TestDescriptor();
    var Right = new TestDescriptor();

    var I = TestCategory.GetContestBetween(Left, Right);

    I.Left.Should().BeSameAs(Left);
    I.Right.Should().BeSameAs(Right);
  }

  [TestMethod]
  public void InterpretLeftSelection()
  {
    var Left = new TestOption();
    var Right = new TestOption();
    var Output = new TestCategory.Output { RightIsWinner = false };

    var Selected = TestCategory.Interpret(Left, Right, Output, new MockInference(Any.Int(), Any.FloatArray()), Any.Int(0, 100)).ConsumeDetached();

    Selected.Should().BeSameAs(Left);
  }

  [TestMethod]
  public void InterpretRightSelection()
  {
    var Left = new TestOption();
    var Right = new TestOption();
    var Output = new TestCategory.Output { RightIsWinner = true };

    var Selected = TestCategory.Interpret(Left, Right, Output, new MockInference(Any.Int(), Any.FloatArray()), Any.Int(0, 100)).ConsumeDetached();

    Selected.Should().BeSameAs(Right);
  }

  [TestMethod]
  public void TrainLeftSelection()
  {
    var Left = new TestOption();
    var Right = new TestOption();
    var MockInference = new MockInference(Any.Int(), Any.FloatArray());
    var Offset = Any.Int(0, 100);
    var T = TestCategory.Interpret(Left, Right, new() { RightIsWinner = Any.Bool }, MockInference, Offset);

    T.Feedback.WinnerShouldBe(Left);

    MockInference.ShouldHaveBeenTrainedWith(new TestCategory.Output() { RightIsWinner = false }.ExtractLossRules(Offset));
  }

  [TestMethod]
  public void TrainRightSelection()
  {
    var Left = new TestOption();
    var Right = new TestOption();
    var MockInference = new MockInference(Any.Int(), Any.FloatArray());
    var Offset = Any.Int(0, 100);
    var T = TestCategory.Interpret(Left, Right, new() { RightIsWinner = Any.Bool }, MockInference, Offset);

    T.Feedback.WinnerShouldBe(Right);

    MockInference.ShouldHaveBeenTrainedWith(new TestCategory.Output() { RightIsWinner = true }.ExtractLossRules(Offset));
  }

  class TestOption;

  [CognitiveData]
  partial record TestDescriptor
  {
    float ArbitraryValue { get; set; } = Any.Float;
  }

  [CognitiveCategory<TestOption, TestDescriptor>(3)]
  partial class TestCategory;
}

static class LossRuleExtensions
{
  public static IReadOnlyList<(int, LossRule)> ExtractLossRules<T>(this T This, int Offset)
    where T : CognitiveData<T>
  {
    var Stream = new LossRuleStream();
    var Writer = new LossRuleWriter(Stream, Offset);

    This.WriteAsLossRules(Writer);

    return Stream.PositionRulePairs;
  }
}