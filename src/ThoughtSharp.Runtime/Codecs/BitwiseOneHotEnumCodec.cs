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

using System.Numerics;

namespace ThoughtSharp.Runtime.Codecs;

// ReSharper disable once UnusedMember.Global
public class BitwiseOneHotEnumCodec<T, U> : CognitiveDataCodec<T>
  where T : Enum
  where U : unmanaged, INumber<U>, IBitwiseOperators<U, U, U>, IShiftOperators<U, int, U>
{
  static readonly BitwiseOneHotNumberCodec<U> Inner = new();

  public BitwiseOneHotEnumCodec()
  {
    if (Enum.GetUnderlyingType(typeof(T)) != typeof(U))
      throw new InvalidOperationException(
        $"Cannot one-hot encode type {typeof(T)} to {typeof(U)} because its underlying type is {Enum.GetUnderlyingType(typeof(T))}");
  }

  public int Length => Inner.Length;

  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    Inner.EncodeTo((U)(object)ObjectToEncode, Target);
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    Inner.WriteLossRulesFor((U)(object)Target, Writer);
  }


  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    return (T)(object)Inner.DecodeFrom(Source);
  }
}