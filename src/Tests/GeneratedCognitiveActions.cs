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

namespace Tests;

[TestClass]
public partial class GeneratedCognitiveActions
{
  [CognitiveActions]
  partial interface BasicAction
  {
    void Empty1();
    void Empty2();
  }

  [CognitiveActions]
  partial interface ActionWithParameters
  {
    void UsesParameters(float Parameter1, [CognitiveDataLength(5)] string Parameter2);
  }

  [CognitiveActions]
  partial interface MultipleChoicesWithParameters
  {
    void Action1(float Parameter1, [CognitiveDataLength(5)] string Parameter2);
    void Action2();
    void Action3(int Parameter1, char Parameter2);
  }

  class MockMultipleChoicesWithParameters : MultipleChoicesWithParameters
  {
    public Action<float, string> Action1Handler { get; set; } = delegate { Assert.Fail("Action1 should be invoked"); };
    public Action Action2Handler { get; set; } = delegate { Assert.Fail("Action2 should be invoked"); };
    public Action<int, char> Action3Handler { get; set; } = delegate { Assert.Fail("Action3 should be invoked"); };

    public void Action1(float Parameter1, string Parameter2)
    {
      Action1Handler(Parameter1, Parameter2);
    }

    public void Action2()
    {
      Action2Handler();
    }

    public void Action3(int Parameter1, char Parameter2)
    {
      Action3Handler(Parameter1, Parameter2);
    }
  }

  [TestMethod]
  public void InvokeNoAction()
  {
    var Choices = new MockMultipleChoicesWithParameters();

    var P = new MultipleChoicesWithParameters.__AllParameters();

    P.InterpretFor(Choices);
  }

  [TestMethod]
  public void InvokeSecondAction()
  {
    var Invoked = false;
    var Choices = new MockMultipleChoicesWithParameters()
    {
      Action2Handler = delegate
      {
        Invoked = true;
      }
    };
    var P = new MultipleChoicesWithParameters.__AllParameters()
    {
      __ActionCode = 2
    };

    P.InterpretFor(Choices);

    Invoked.Should().BeTrue();
  }
}