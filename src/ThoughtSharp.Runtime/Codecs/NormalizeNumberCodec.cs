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

using System.Numerics;

namespace ThoughtSharp.Runtime.Codecs;

// ReSharper disable once UnusedMember.Global
public class NormalizeNumberCodec<T, U>(
  T Minimum,
  T Maximum,
  ThoughtDataCodec<U> Inner) : ThoughtDataCodec<T>
  where T : INumber<T>
  where U : INumber<U>, IFloatingPoint<U>, IAdditionOperators<U, U, U>, IMultiplyOperators<U, U, U>
{
  readonly U TargetTypedMinimum = U.CreateChecked(Minimum);
  readonly U TargetTypedMaximum = U.CreateChecked(Maximum);

  public int Length => Inner.Length;

  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    var Value = (U.CreateChecked(ObjectToEncode) - TargetTypedMinimum) / (TargetTypedMaximum - TargetTypedMinimum);
    Inner.EncodeTo(Value, Target);
  }

  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    return T.Zero;
  }
}