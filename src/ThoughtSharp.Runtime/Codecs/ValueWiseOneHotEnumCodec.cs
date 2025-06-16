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

public class ValueWiseOneHotEnumCodec<T, U> : CognitiveDataCodec<T>
  where T : struct, Enum
  where U : unmanaged, IBinaryInteger<U>, IBitwiseOperators<U, U, U>, IShiftOperators<U, int, U>
{
  static readonly List<T> ValuesInOrder = Enum.GetValues<T>().ToList();
  static readonly ValueWiseOneHotNumberCodec<U> Inner = new(U.Zero, U.CreateChecked(ValuesInOrder.Count));

  public ValueWiseOneHotEnumCodec()
  {
    if (Enum.GetUnderlyingType(typeof(T)) != typeof(U))
      throw new InvalidOperationException(
        $"Cannot one-hot encode type {typeof(T)} to {typeof(U)} because its underlying type is {Enum.GetUnderlyingType(typeof(T))}");
  }

  public int Length => Inner.Length;

  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    var Index = ValuesInOrder.IndexOf(ObjectToEncode);
    Inner.EncodeTo(U.CreateChecked(Index), Target);
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    var Index = ValuesInOrder.IndexOf(Target);
    Inner.WriteLossRulesFor(U.CreateChecked(Index), Writer);
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
    Inner.WriteIsolationBoundaries(Writer);
  }

  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    var Index = int.CreateChecked(Inner.DecodeFrom(Source));
    return ValuesInOrder[Index];
  }
}