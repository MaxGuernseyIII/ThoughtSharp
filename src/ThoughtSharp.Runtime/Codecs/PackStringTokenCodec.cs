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

using System.Collections.Immutable;
using JetBrains.Annotations;

namespace ThoughtSharp.Runtime.Codecs;

[PublicAPI]
public class PackStringTokenCodec(string ValidCharacters, int MaximumLength) : CognitiveDataCodec<string>
{
  public ImmutableArray<long> EncodedTokenClassCounts => [];

  public void EncodeTo(string ObjectToEncode, Span<float> Target, Span<long> _)
  {
    foreach (var I in Enumerable.Range(0, Math.Min(ObjectToEncode.Length, MaximumLength)))
    {
      var Base = LogitsPerCharacter * I;
      var Index = ValidCharacters.IndexOf(ObjectToEncode[I]) + 1;
      foreach (var Offset in Enumerable.Range(0, LogitsPerCharacter))
        Target[Base + Offset] = ((Index >> Offset) & 1) == 1 ? 1f : -1f;
    }
  }

  public void WriteLossRulesFor(string Target, LossRuleWriter Writer)
  {
    throw new NotSupportedException("This is an input-only codec");
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
  }

  public string DecodeFrom(ReadOnlySpan<float> Source)
  {
    throw new NotSupportedException("This is an input-only codec");
  }

  public int FloatLength => MaximumLength * LogitsPerCharacter;

  int LogitsPerCharacter { get; } = (int)Math.Ceiling(Math.Log2(ValidCharacters.Length + 1));
}