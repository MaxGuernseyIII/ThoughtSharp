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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using static TorchSharp.torch;

namespace Tests;

[TestClass]
public class TokenEncoding
{
  enum DummyEnum {}

  void CodecDoesNotSupportTokens<T>(CognitiveDataCodec<T> Encoder)
  {
    Encoder.EncodedTokenClassCounts.Should().BeEquivalentTo(Array.Empty<long>());
  }

  [TestMethod]
  public void BitwiseOneHotEnumCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new BitwiseOneHotEnumCodec<DummyEnum, int>());
  }

  [TestMethod]
  public void BitwiseOneHotNumberCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new BitwiseOneHotNumberCodec<int>());
  }

  [TestMethod]
  public void BitwiseOneHotStringCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new BitwiseOneHotStringCodec(Any.Int()));
  }

  [TestMethod]
  public void CopyBoolCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new CopyBoolCodec(0));
  }

  [TestMethod]
  public void CopyFloatCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new CopyFloatCodec());
  }

  [TestMethod]
  public void NormalizingCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new NormalizingCodec<float>(new MockLengthCodec<float>(1), 0, 1));
  }

  [TestMethod]
  public void NumberToFloatingPointCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new NumberToFloatingPointCodec<int, float>(new MockLengthCodec<float>(1)));
  }

  [TestMethod]
  public void PackStringTokenCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new PackStringTokenCodec("", 1));
  }

  [TestMethod]
  public void RoundingCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new RoundingCodec<float>(new MockLengthCodec<float>(0)));
  }

  [TestMethod]
  public void ValueWiseOneHotEnumCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new ValueWiseOneHotEnumCodec<DummyEnum, int>());
  }

  [TestMethod]
  public void ValueWiseOneHotNumberCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new ValueWiseOneHotNumberCodec<int>(0, 1));
  }

  [TestMethod]
  public void SubDataCodecDelegatesToDataType()
  {
    Assert.Fail("This test is not implemented, yet.");
  }

  [TestMethod]
  public void IsolatingCodecPassesThrough()
  {
    var ClassCounts = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var Codec = new IsolatingCodec<int>(new MockTokenClassCountsCodec<int>(ClassCounts));

    var Actual = Codec;

    Actual.Should().BeEquivalentTo(ClassCounts, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void CompositeCodecMergesTokenClassCounts()
  {
    var ClassCounts1 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var ClassCounts2 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var Codec = new CompositeCodec<int>(new MockTokenClassCountsCodec<int>(ClassCounts1), new MockTokenClassCountsCodec<int>(ClassCounts2));

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo([..ClassCounts1, ..ClassCounts2], O => O.WithStrictOrdering());
  }
}