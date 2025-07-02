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
using Microsoft.Testing.Platform.Extensions;
using Tests.Mocks;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;

namespace Tests;

[TestClass]
public class TokenEncoding
{
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
  public void NormalizingCodecDefersTokenLayoutToInner()
  {
    var Inner = new MockTokenClassCountsCodec<float>(Any.ListOf(() => Any.Long, 1, 3));
    var Codec = new NormalizingCodec<float>(Inner, 0, 1);

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo(Inner.EncodedTokenClassCounts, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void NumberToFloatingPointCodecDefersTokenLayoutToInner()
  {
    var Inner = new MockTokenClassCountsCodec<float>(Any.ListOf(() => Any.Long, 1, 3));
    var Codec = new NumberToFloatingPointCodec<int, float>(Inner);

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo(Inner.EncodedTokenClassCounts, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void PackStringTokenCodecDoesNotSupportTokens()
  {
    CodecDoesNotSupportTokens(new PackStringTokenCodec("", 1));
  }

  [TestMethod]
  public void RoundingCodecDefersTokenLayoutToInner()
  {
    var Inner = new MockTokenClassCountsCodec<float>(Any.ListOf(() => Any.Long, 1, 3));
    var Codec = new RoundingCodec<float>(Inner);

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo(Inner.EncodedTokenClassCounts, O => O.WithStrictOrdering());
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
  public void SubDataCodecDelegatesToDataTypeForLayout()
  {
    var Codec = new SubDataCodec<MockCognitiveData>();

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo(MockCognitiveData.EncodedTokenClassCounts, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void SubDataCodecDelegatesToDataTypeForEncoding()
  {
    var ToMarshal = Any.ListOf(() => Any.Long, MockCognitiveData.EncodedTokenClassCounts.Length,
      MockCognitiveData.EncodedTokenClassCounts.Length);
    var Data = new MockCognitiveData
    {
      OnMarshalTo = (Target, Tokens) =>
      {
        foreach (var I in Enumerable.Range(0, ToMarshal.Count))
          Tokens[I] = ToMarshal[I];
      }
    };
    var Codec = new SubDataCodec<MockCognitiveData>();
    var Tokens = new long[Codec.EncodedTokenClassCounts.Length];

    Codec.EncodeTo(Data, [], Tokens);

    Tokens.Should().BeEquivalentTo(ToMarshal, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void IsolatingCodecPassesThroughLayout()
  {
    var ClassCounts = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var Codec = new IsolatingCodec<int>(new MockTokenClassCountsCodec<int>(ClassCounts));

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo(ClassCounts, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void IsolatingCodecPassesThroughTokenEncoding()
  {
    var ClassCounts = Any.ListOf(() => (long) Any.Int(), 0, 4);
    ImmutableArray<long> EncodedTokens = [];
    var Inner = new MockTokenClassCountsCodec<int>(ClassCounts)
    {
      OnEncode = (_, _, Tokens) => EncodedTokens = [..Tokens]
    };
    var Codec = new IsolatingCodec<int>(Inner);
    var Tokens = Any.ListOf(() => Any.Long, ClassCounts.Count, ClassCounts.Count).ToArray();

    Codec.EncodeTo(0, new float[Codec.FloatLength], Tokens);

    EncodedTokens.Should().BeEquivalentTo(Tokens, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void CompositeCodecMergesTokenClassCounts()
  {
    var ClassCounts1 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var ClassCounts2 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var Codec = new CompositeCodec<int>(new MockTokenClassCountsCodec<int>(ClassCounts1),
      new MockTokenClassCountsCodec<int>(ClassCounts2));

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo([..ClassCounts1, ..ClassCounts2], O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void TokenCodecAsksForASingleToken()
  {
    var NumberOfTokenClasses = Any.Int();
    var Codec = new TokenCodec<int>(NumberOfTokenClasses);

    var Actual = Codec.EncodedTokenClassCounts;

    Actual.Should().BeEquivalentTo([NumberOfTokenClasses]);
  }

  [TestMethod]
  public void TokenCodecCopiesToken()
  {
    var NumberOfTokenClasses = Any.Int();
    var Codec = new TokenCodec<int>(NumberOfTokenClasses);
    var Tokens = new long[1];
    var Token = Any.Int();

    Codec.EncodeTo(Token, [], Tokens);

    Tokens.Should().BeEquivalentTo([Token], O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void TokenCodecHasNoFloats()
  {
    var Codec = new TokenCodec<int>(Any.Int());

    var Actual = Codec.FloatLength;

    Actual.Should().Be(0);
  }

  [TestMethod]
  public void CompositeCodecTokenEncoding()
  {
    var ClassCounts1 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    var ClassCounts2 = Any.ListOf(() => (long) Any.Int(), 0, 4);
    ImmutableArray<long> FirstSlice = [];
    ImmutableArray<long> SecondSlice = [];
    var Codec1 = new MockTokenClassCountsCodec<int>(ClassCounts1)
    {
      OnEncode = (_, _, Tokens) => { FirstSlice = [..Tokens]; }
    };
    var Codec2 = new MockTokenClassCountsCodec<int>(ClassCounts2)
    {
      OnEncode = (_, _, Tokens) => { SecondSlice = [..Tokens]; }
    };
    var Codec = new CompositeCodec<int>(Codec1, Codec2);
    var InitialTokens = Any.ListOf(() => Any.Long, Codec.EncodedTokenClassCounts.Length,
      Codec.EncodedTokenClassCounts.Length).ToArray();

    Codec.EncodeTo(0, [], InitialTokens);

    FirstSlice.Should().BeEquivalentTo(InitialTokens[..ClassCounts1.Count], O => O.WithStrictOrdering());
    SecondSlice.Should().BeEquivalentTo(InitialTokens[ClassCounts1.Count..], O => O.WithStrictOrdering());
  }

  enum DummyEnum
  {
  }

  // ReSharper disable once ClassNeverInstantiated.Local
  class MockCognitiveData : CognitiveData<MockCognitiveData>
  {
    public static int FloatLength => 0;

    public static ImmutableArray<long> EncodedTokenClassCounts { get; } = [..Any.ListOf(() => (long) Any.Int(), 1, 5)];

    public delegate void MarshalToAction(Span<float> Target, Span<long> Tokens);

    public MarshalToAction OnMarshalTo { get; set; }= delegate { };

    public void MarshalTo(Span<float> Target, Span<long> Tokens)
    {
      OnMarshalTo(Target, Tokens);
    }

    public void WriteAsLossRules(LossRuleWriter Target)
    {
      throw new NotImplementedException();
    }

    public static MockCognitiveData UnmarshalFrom(ReadOnlySpan<float> Source)
    {
      throw new NotImplementedException();
    }

    public static void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)
    {
      throw new NotImplementedException();
    }
  }
}