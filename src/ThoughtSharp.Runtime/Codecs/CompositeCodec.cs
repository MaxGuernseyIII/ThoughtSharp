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

namespace ThoughtSharp.Runtime.Codecs;

public class CompositeCodec<T>(CognitiveDataCodec<T> First, CognitiveDataCodec<T> Second) : CognitiveDataCodec<T>
{
  public int FloatLength => First.FloatLength + Second.FloatLength;

  public ImmutableArray<long> EncodedTokenClassCounts =>
    [..First.EncodedTokenClassCounts, ..Second.EncodedTokenClassCounts];

  public void EncodeTo(T ObjectToEncode, Span<float> Target, Span<long> Tokens)
  {
    First.EncodeTo(ObjectToEncode, Target[..First.FloatLength], Tokens[..First.EncodedTokenClassCounts.Length]);
    Second.EncodeTo(ObjectToEncode, Target[First.FloatLength..], Tokens[First.EncodedTokenClassCounts.Length..]);
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    First.WriteLossRulesFor(Target, Writer);
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
    First.WriteIsolationBoundaries(Writer);
    Second.WriteIsolationBoundaries(Writer.AddOffset(First.FloatLength));
  }

  public T DecodeFrom(ReadOnlySpan<float> Source, ReadOnlySpan<long> Tokens)
  {
    return First.DecodeFrom(Source[..First.FloatLength], Tokens[..First.EncodedTokenClassCounts.Length]);
  }
}