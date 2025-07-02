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
using System.Numerics;

namespace ThoughtSharp.Runtime.Codecs;

// ReSharper disable once UnusedMember.Global
public class ValueWiseOneHotNumberCodec<T>(T Bound1, T Bound2) : CognitiveDataCodec<T>
  where T : unmanaged, IBinaryInteger<T>, ISubtractionOperators<T, T, T>, IAdditionOperators<T, T, T>
{
  readonly T Minimum = T.MinNumber(Bound1, Bound2);
  readonly T Maximum = T.MaxNumber(Bound1, Bound2);

  public int FloatLength => int.CreateChecked(Maximum) - int.CreateChecked(Minimum) + 1;

  public ImmutableArray<long> EncodedTokenClassCounts => throw new NotImplementedException();

  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    for (var I = 0; I < FloatLength; ++I)
      Target[I] = T.CreateChecked(I) + Minimum == ObjectToEncode ? 1 : 0;
  }

  public void WriteLossRulesFor(T Target, LossRuleWriter Writer)
  {
    Writer.WriteLossRule(0, new CrossEntropyLossRule(long.CreateChecked(Target - Minimum), FloatLength));
  }

  public void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
  {
  }

  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    var LargestScale = Source[0];
    var Largest = 0;

    for (var I = 0; I < FloatLength; ++I)
    {
      var CandidateScale = Source[I];
      if (CandidateScale > LargestScale)
      {
        LargestScale = CandidateScale;
        Largest = I;
      }
    }

    return T.CreateChecked(Largest) + Minimum;
  }
}