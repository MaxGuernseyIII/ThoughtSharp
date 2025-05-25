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
using FluentAssertions;
using FluentAssertions.Collections;
using ThoughtSharp.Runtime;

namespace Tests;

static class AssertionExtensions
{
  public static GenericCollectionAssertions<T> MarshalToSameTensor<T>(this GenericCollectionAssertions<T> Assertions,
    IEnumerable<T> Expected)
    where T : CognitiveData<T>
  {
    var ActualBuffer = MarshalToBuffer(Assertions.Subject);
    var ExpectedBuffer = MarshalToBuffer(Expected);

    ActualBuffer.Should().Equal(ExpectedBuffer);

    return Assertions;
  }

  static float[] MarshalToBuffer<T>(IEnumerable<T> CollectionSubject) where T : CognitiveData<T>
  {
    var Items = CollectionSubject.ToImmutableArray();
    var Buffer = new float[T.Length * Items.Length];
    for (var I = 0; I < Items.Length; ++I)
      Items[I].MarshalTo(Buffer[(I * T.Length)..((I + 1) * T.Length)]);

    return Buffer;
  }
}