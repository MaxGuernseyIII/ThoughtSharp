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

namespace ThoughtSharp.Runtime.Codecs;

// ReSharper disable once UnusedMember.Global
public class BitwiseOneHotStringCodec(int Length) : CognitiveDataCodec<string>
{
  static readonly BitwiseOneHotNumberCodec<char> Inner = new();

  // ReSharper disable once ReplaceWithPrimaryConstructorParameter
  readonly int MaximumCharacters = Length;
  public int FloatLength { get; } = Length * Inner.FloatLength;

  public ImmutableArray<long> EncodedTokenClassCounts => Inner.EncodedTokenClassCounts;

  public void EncodeTo(string ObjectToEncode, Span<float> Target, Span<long> _)
  {
    var Padded = ObjectToEncode.PadRight(MaximumCharacters, (char) 0);
    var Index = 0;

    foreach (var C in Padded)
    {
      Inner.EncodeTo(C, Target[Index..(Index + Inner.FloatLength)], []);
      Index += Inner.FloatLength;
    }
  }

  public void WriteLossRulesFor(string Target, LossRuleWriter Writer)
  {
    this.WriteStandardLossRulesFor(Target, Writer);
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
  }

  public string DecodeFrom(ReadOnlySpan<float> Source)
  {
    var ResultBuffer = new char[MaximumCharacters];

    foreach (var I in Enumerable.Range(0, MaximumCharacters))
      ResultBuffer[I] = Inner.DecodeFrom(Source[(I * Inner.FloatLength)..((I + 1) * Inner.FloatLength)]);

    return new string(ResultBuffer).TrimEnd((char) 0);
  }
}