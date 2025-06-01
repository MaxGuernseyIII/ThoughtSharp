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
using static ThoughtSharp.Runtime.AccumulatedUseFeedback;

namespace Tests;

[TestClass]
public class AccumulatedUseFeedbackTraining
{
  public class MockToUse
  {
    public int Operation1CallCount;

    public void Operation1()
    {
      Operation1CallCount++;
    }

    public int Operation2CallCount;
    public int Operation2SomeArgument;

    public void Operation2(int SomeArgument)
    {
      Operation2CallCount++;
      Operation2SomeArgument = SomeArgument;
    }
  }

  [TestMethod]
  public void HandlesTheDoNothingCase()
  {
    var RequiresMore = new BoxedBool() { Value = true };
    var Source = GetSource<MockToUse>();
    var ActionSurfaceMock = new MockToUse();
    var OperationFeedback = new UseFeedback<MockToUse>(ActionSurfaceMock, B => RequiresMore.Value = B);
    Source.Configurator.AddStep(OperationFeedback);
    var Feedback = Source.CreateFeedback();

    Feedback.UseShouldHaveBeen();

    ActionSurfaceMock.Operation1CallCount.Should().Be(0);
    ActionSurfaceMock.Operation2CallCount.Should().Be(0);
    RequiresMore.Value.Should().Be(false);
  }

  [TestMethod]
  public void HandlesOneCallWithoutParameters()
  {
    var RequiresMore = new BoxedBool() { Value = true };
    var Source = GetSource<MockToUse>();
    var ActionSurfaceMock = new MockToUse();
    var OperationFeedback = new UseFeedback<MockToUse>(ActionSurfaceMock, B => RequiresMore.Value = B);
    Source.Configurator.AddStep(OperationFeedback);
    var Feedback = Source.CreateFeedback();

    Feedback.UseShouldHaveBeen(M => M.Operation1());

    ActionSurfaceMock.Operation1CallCount.Should().Be(1);
    ActionSurfaceMock.Operation2CallCount.Should().Be(0);
    RequiresMore.Value.Should().Be(false);
  }
}