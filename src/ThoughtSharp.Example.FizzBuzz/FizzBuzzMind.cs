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
using ThoughtSharp.Runtime.Codecs;

namespace ThoughtSharp.Example.FizzBuzz;

[CognitiveData]
partial class FizzBuzzInput
{
  public byte Value { get; set; }

  public static CognitiveDataCodec<byte> ValueCodec { get; } = 
    new CompositeCodec<byte>(
      new BitwiseOneHotNumberCodec<byte>(),
      new NormalizeNumberCodec<byte,float>(
        byte.MinValue, byte.MaxValue, new RoundingCodec<float>(new CopyFloatCodec())));
}

[Mind]
partial class FizzBuzzMind
{
  [Use]
  public partial Thought<bool, UseFeedback<FizzBuzzTerminal>> WriteForNumber(FizzBuzzTerminal Surface, FizzBuzzInput InputData);
}

[CognitiveData]
partial class ShapeInfo
{
  [CognitiveDataBounds<float>(-1e2f, 1e2f)]
  public float Area { get; set; }
}

[Mind]
partial class ShapesMind
{
  [Make]
  public partial Thought<ShapeInfo, MakeFeedback<ShapeInfo>> MakeSquare([CognitiveDataBounds<float>(-1e3f, 1e3f)] float SideLength);
}

[CognitiveData]
public partial class ComparisonData
{
  [CognitiveDataBounds<short>(-1, 1)]
  public short Comparison { get; set; }
}

[CognitiveData]
public partial class ComparisonInputData
{
  public byte Left { get; set; }
  static readonly CognitiveDataCodec<byte> LeftCodec = new BitwiseOneHotNumberCodec<byte>();
  public byte Right { get; set; }
  static readonly CognitiveDataCodec<byte> RightCodec = new BitwiseOneHotNumberCodec<byte>();

}

[Mind]
public partial class ComparisonMind
{
  [Make]
  public partial Thought<ComparisonData, MakeFeedback<ComparisonData>> Compare(ComparisonInputData Parameters);
}