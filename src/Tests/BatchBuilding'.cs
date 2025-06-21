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
  [TestMethod]
  public void DefaultValue()
  {
    var Default = Any.Float;
    var SequenceCount = Any.Int(1, 10);
    var BatchBuilder = new Batch<float>.Builder(Default, SequenceCount);
    BatchBuilder.AddStep();

    var Batch = BatchBuilder.Build();

    Batch.Time[0].InSequence[Any.Int(0, SequenceCount - 1)].Should().Be(Default);
  }

  [TestMethod]
  public void AssignValues()
  {
    var SequenceCount = Any.Int(1, 10);
    var BatchBuilder = new Batch<float>.Builder(Any.Float, SequenceCount);
    var Step = BatchBuilder.AddStep();
    var AffectedSequenceNumber = Any.Int(0, SequenceCount - 1);
    var Expected = Any.Float;
    Step[AffectedSequenceNumber] = Expected;

    var Batch = BatchBuilder.Build();

    Batch.Time[0].InSequence[AffectedSequenceNumber].Should().Be(Expected);
  }
}