﻿// MIT License
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

using System.Collections.Immutable;
using System.Numerics;

namespace ThoughtSharp.Runtime.Codecs;

public class NormalizingCodec<T>(CognitiveDataCodec<T> Inner, T Minimum, T Maximum) : CognitiveDataCodec<T>
  where T : IFloatingPoint<T>, ISubtractionOperators<T, T, T>, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
{
  readonly T Size = Maximum - Minimum;
  public int FloatLength => Inner.FloatLength;

  public ImmutableArray<long> EncodedTokenClassCounts => Inner.EncodedTokenClassCounts;

  public void EncodeTo(T ObjectToEncode, Span<float> Target, Span<long> Tokens)
  {
    Inner.EncodeTo(GetNormalized(ObjectToEncode), Target, []);
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    Inner.WriteLossRulesFor(GetNormalized(Target), Writer);
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
    Inner.WriteIsolationBoundaries(Writer);
  }

  public T DecodeFrom(ReadOnlySpan<float> Source, ReadOnlySpan<long> _)
  {
    var Normalized = Inner.DecodeFrom(Source, []);

    var Scaled = Normalized * Size;

    return Minimum + Scaled;
  }

  T GetNormalized(T ObjectToEncode)
  {
    return (ObjectToEncode - Minimum) / Size;
  }
}