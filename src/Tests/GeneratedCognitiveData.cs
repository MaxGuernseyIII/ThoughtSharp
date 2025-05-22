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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;

namespace Tests;

[TestClass]
public partial class GeneratedCognitiveData
{
  [TestMethod]
  public void LengthTest()
  {
    EmptyMockThoughtData.Length.Should().Be(0);
    SingleFloatMockThoughtData.Length.Should().Be(1);
    FloatArrayMockThoughtData.Length.Should().Be(4);
    CodecMockThoughtData.Length.Should().Be(9);
  }

  [TestMethod]
  public void MarshalSingleFloat()
  {
    var Values = new float[SingleFloatMockThoughtData.Length];
    var Expected = Any.Float;
    var Data = new SingleFloatMockThoughtData
    {
      P1 = Expected
    };

    Data.MarshalTo(Values);

    Values.Should().Equal(Expected);
  }

  [TestMethod]
  public void UnmarshalSingleFloat()
  {
    var Expected = Any.Float;
    var Values = new[] {Expected};
    var Data = new SingleFloatMockThoughtData();

    Data.MarshalFrom(Values);

    Data.P1.Should().Be(Expected);
  }

  [TestMethod]
  public void MarshalFloatArray()
  {
    var Expected0 = Any.Float;
    var Expected1 = Any.Float;
    var Expected2 = Any.Float;
    var Expected3 = Any.Float;
    var Data = new FloatArrayMockThoughtData
    {
      P2 = [Expected0, Expected1, Expected2, Expected3]
    };
    var Values = new float[FloatArrayMockThoughtData.Length];

    Data.MarshalTo(Values);

    Values.Should().Equal(Expected0, Expected1, Expected2, Expected3);
  }

  [TestMethod]
  public void UnmarshalFloatArray()
  {
    var Expected0 = Any.Float;
    var Expected1 = Any.Float;
    var Expected2 = Any.Float;
    var Expected3 = Any.Float;
    var Values = new[] {Expected0, Expected1, Expected2, Expected3};
    var Data = new FloatArrayMockThoughtData();

    Data.MarshalFrom(Values);

    Data.P2[0].Should().Be(Expected0);
    Data.P2[1].Should().Be(Expected1);
    Data.P2[2].Should().Be(Expected2);
    Data.P2[3].Should().Be(Expected3);
  }

  [TestMethod]
  public void RoundTripStrings()
  {
    RoundTripTest(new ThreeStringsMockThoughtData
    {
      S1 = Any.StringWithBitsLength(ThreeStringsMockThoughtData.S1Codec.Length).TrimEnd((char) 0),
      S2 = Any.StringWithBitsLength(ThreeStringsMockThoughtData.S2Codec.Length).TrimEnd((char) 0),
      S3 = Any.StringWithBitsLength(ThreeStringsMockThoughtData.S3Codec.Length).TrimEnd((char) 0)
    });
  }

  [TestMethod]
  public void RoundTripComplexData()
  {
    RoundTripTest(new ComplexDataStructureMockThoughtData
    {
      EncodedString = Any.StringWithBitsLength(ComplexDataStructureMockThoughtData.EncodedStringCodec.Length),
      SomeEnum = Any.EnumValue<MockPrivateEnum>(),
      SomeFlag = Any.Bool,
      SomeFloat = Any.Float,
      SomeInteger = Any.Int(0, 100),
      SomeLongs = [Any.Long, Any.Long, Any.Long],
      ImplicitlyEncodedString = Any.ASCIIString(ComplexDataStructureMockThoughtData.ImplicitlyEncodedStringLength)
    });
  }

  [TestMethod]
  public void RoundTripNesting()
  {
    RoundTripTest(new NestingMockThoughtData
    {
      First =
      {
        P1 = Any.Float
      },
      Second =
      {
        P2 = [Any.Float, Any.Float, Any.Float, Any.Float]
      },
      Third =
      {
        P1 = Any.Float
      }
    });
  }

  void RoundTripTest<T>(T ToTest)
    where T : CognitiveData<T>, new()
  {
    var Buffer = new float[T.Length];
    ToTest.MarshalTo(Buffer);
    var Actual = new T();

    Actual.MarshalFrom(Buffer);

    Actual.Should().BeEquivalentTo(ToTest);
  }

  [TestMethod]
  public void NormalizeData()
  {
    var Data = new DeclarativelyNormalizedMockThoughtData
    {
      ToNormalize = Any.Float * 5 + 1
    };

    var Target = new float[DeclarativelyNormalizedMockThoughtData.Length];
    Data.MarshalTo(Target);

    Target[0].Should().BeApproximately((Data.ToNormalize - 1f) / 5f, 0.01f);
  }

  [TestMethod]
  public void DenormalizeData()
  {
    var Target = new float[DeclarativelyNormalizedMockThoughtData.Length];
    Target[0] = Any.Float;
    var Data = new DeclarativelyNormalizedMockThoughtData();

    Data.MarshalFrom(Target);

    Data.ToNormalize.Should().BeApproximately(Target[0] * 5 + 1, 0.01f);
  }

  [TestMethod]
  public void MinimumAutoBounds()
  {
    BoundsMarshallingTest(new()
    {
      Byte = byte.MinValue,
      SByte = sbyte.MinValue,
      UShort = ushort.MinValue,
      Short = short.MinValue,
      Char = char.MinValue
    }, 0);
  }

  [TestMethod]
  public void MaximumAutoBounds()
  {
    BoundsMarshallingTest(new()
    {
      Byte = byte.MaxValue,
      SByte = sbyte.MaxValue,
      UShort = ushort.MaxValue,
      Short = short.MaxValue,
      Char = char.MaxValue
    }, 1);
  }

  [TestMethod]
  public void AutoBoundsRounding()
  {
    BoundsUnmarshallingTest(new()
    {
      Byte = byte.MaxValue,
      SByte = sbyte.MaxValue,
      UShort = ushort.MaxValue,
      Short = short.MaxValue,
      Char = char.MaxValue
    }, 0.9999999f);
  }

  void BoundsMarshallingTest(DefaultNormalizationMockThoughtData Data, float Expected)
  {
    var Buffer = new float[DefaultNormalizationMockThoughtData.Length];
    Data.MarshalTo(Buffer);

    foreach (var F in Buffer)
      F.Should().BeApproximately(Expected, 0.01f);
  }

  void BoundsUnmarshallingTest(DefaultNormalizationMockThoughtData Data, float Value)
  {
    var Buffer = new float[DefaultNormalizationMockThoughtData.Length];
    for (var I = 0; I < Buffer.Length; ++I)
      Buffer[I] = Value;

    var NewData = new DefaultNormalizationMockThoughtData();
    NewData.MarshalFrom(Buffer);

    NewData.Should().BeEquivalentTo(Data);
  }

  [CognitiveData]
  public partial class EmptyMockThoughtData
  {
  }

  [CognitiveData]
  public partial class SingleFloatMockThoughtData
  {
    public float P1;
  }

  [CognitiveData]
  public partial class FloatArrayMockThoughtData
  {
    [CognitiveDataCount(4)] public float[] P2 = [0, 0, 0, 0];
  }

  [CognitiveData]
  public partial class CodecMockThoughtData
  {
    public string SomeText = "";

    public static CognitiveDataCodec<string> SomeTextCodec => new MockLengthCodec<string>(9);
  }

  [CognitiveData]
  public partial class ThreeStringsMockThoughtData
  {
    public string S1 = "";
    public string S2 = "";
    public string S3 = "";

    public static BitwiseOneHotStringCodec S1Codec => new(20);
    public static BitwiseOneHotStringCodec S2Codec => new(3);
    public static BitwiseOneHotStringCodec S3Codec => new(7);
  }

  enum MockPrivateEnum
  {
    Value1,
    Value2,
    Value3
  }

  [CognitiveData]
  partial class ComplexDataStructureMockThoughtData
  {
    public const int ImplicitlyEncodedStringLength = 40;

    public string EncodedString = "";

    public MockPrivateEnum SomeEnum;

    public float SomeFloat;

    [CognitiveDataCount(3)] public long[] SomeLongs = [0, 0, 0];

    public bool SomeFlag { get; set; }
    public int SomeInteger { get; set; }
    public static BitwiseOneHotStringCodec EncodedStringCodec => new(20);

    [CognitiveDataLength(ImplicitlyEncodedStringLength)]
    public string ImplicitlyEncodedString { get; set; } = "";
  }

  [CognitiveData]
  public partial class NestingMockThoughtData
  {
    public SingleFloatMockThoughtData First = new();

    // ReSharper disable once RedundantNameQualifier
    public FloatArrayMockThoughtData Second = new();
    public SingleFloatMockThoughtData Third = new();
  }

  [CognitiveData]
  public partial class DeclarativelyNormalizedMockThoughtData
  {
    [CognitiveDataBounds<float>(1, 6)] public float ToNormalize;
  }

  [CognitiveData]
  public partial class DefaultNormalizationMockThoughtData
  {
    public byte Byte;
    public char Char;
    public sbyte SByte;
    public short Short;
    public ushort UShort;
  }
}