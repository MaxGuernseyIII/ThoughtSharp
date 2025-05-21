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

using Tests.Mocks;

namespace Tests;

using ThoughtSharp.Runtime;
using FluentAssertions;

[TestClass]
public partial class GeneratedThoughtData
{
  [ThoughtData]
  public partial class EmptyMockThoughtData
  {

  }

  [ThoughtData]
  public partial class SingleFloatMockThoughtData
  {
    public float P1;
  }

  [ThoughtData]
  public partial class FloatArrayMockThoughtData
  {
    [ThoughtDataLength(4)]
    public float[] P2 = [0,0,0,0];
  }

  [ThoughtData]
  public partial class CodecMockThoughtData
  {
    public string SomeText = "";

    public static ThoughtDataCodec<string> SomeTextCodec => new MockLengthCodec<string>(9);
  }

  [TestMethod]
  public void LengthTest()
  {
    EmptyMockThoughtData.Length.Should().Be(0);
    SingleFloatMockThoughtData.Length.Should().Be(1);
    FloatArrayMockThoughtData.Length.Should().Be(4);
    CodecMockThoughtData.Length.Should().Be(9);
  }

  [TestMethod]
  public void MarshalSingleFloat()
  {
    var Values = new float[SingleFloatMockThoughtData.Length];
    var Expected = Any.Float;
    var Data = new SingleFloatMockThoughtData()
    {
      P1 = Expected
    };

    Data.MarshalTo(Values);

    Values.Should().Equal([Expected]);
  }

  [TestMethod]
  public void UnmarshalSingleFloat()
  {
    var Expected = Any.Float;
    var Values = new[] { Expected };
    var Data = new SingleFloatMockThoughtData();

    Data.MarshalFrom(Values);

    Data.P1.Should().Be(Expected);
  }
}