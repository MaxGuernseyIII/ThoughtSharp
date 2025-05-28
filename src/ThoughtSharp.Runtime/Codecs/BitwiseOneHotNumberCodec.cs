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
public class BitwiseOneHotNumberCodec<T> : CognitiveDataCodec<T>
  where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
{
  public int Length { get; } = typeof(T) switch
  {
    var T when T == typeof(byte) => 8,
    var T when T == typeof(sbyte) => 8,
    var T when T == typeof(short) => 16,
    var T when T == typeof(ushort) => 16,
    var T when T == typeof(int) => 32,
    var T when T == typeof(uint) => 32,
    var T when T == typeof(long) => 64,
    var T when T == typeof(ulong) => 64,
    var T when T == typeof(char) => 16,
    _ => throw new NotSupportedException($"Unsupported type {typeof(T)}")
  };

  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    for (var I = 0; I < Length; I++)
    {
      var BitMask = T.One << I;
      var BitSet = (ObjectToEncode & BitMask) != T.Zero;
      Target[I] = BitSet ? 1.0f : 0.0f;
    }
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    this.WriteStandardLossRulesFor(Target, Writer);
  }

  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    var Result = T.Zero;

    for (var Index = 0; Index < Length; Index++)
      if (Source[Index] >= 0.5f)
        Result |= T.One << Index;

    return Result;
  }
}