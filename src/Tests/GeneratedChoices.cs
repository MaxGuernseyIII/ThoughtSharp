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
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;

namespace Tests;

[TestClass]
public partial class GeneratedChoices
{
  [TestMethod]
  public void EncodeOnlyFullBatch()
  {
    IReadOnlyList<CognitiveOption<MockPayload, MockData>> Options =
    [
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float})
    ];
    ushort NextPosition = 0;
    CognitiveCategory<MockPayload, MockData> Category = new MockCategory();
    var Buffer = new float[MockCategory.EncodeLength];
    var ReferenceBuffer = GetReferenceBuffer(Options, NextPosition, 3, 3, true);
    Options[0].Descriptor.MarshalTo(Buffer[..]);

    Category.EncodeBatch(Buffer, ref NextPosition, out var IsLastBatch);

    NextPosition.Should().Be(3);
    IsLastBatch.Should().Be(true);
    Buffer.Should().Equal(ReferenceBuffer);
  }

  class MockPayload;

  [CognitiveData]
  partial class MockData
  {
    public float Parameter;
  }

  [CognitiveCategory<MockPayload, MockData>(3)]
  class MockCategory
  {
  }

  float[] GetReferenceBuffer<TPayload, TDescriptor>(
    IReadOnlyList<CognitiveOption<TPayload, TDescriptor>> Options,
    ushort First,
    int BatchSize,
    int Count,
    bool IsLast) 
    where TDescriptor : CognitiveData<TDescriptor>
  {
    var ItemNumberCodec = new NumberToFloatingPointCodec<short, float>(
      new RoundingCodec<float>(new NormalizingCodec<float>(new CopyFloatCodec(), ushort.MinValue, ushort.MaxValue)));
    var BoolCodec = new CopyBoolCodec();
    var DescriptorCodec = new SubDataCodec<TDescriptor>();
    var ItemSize = ItemNumberCodec.Length + BoolCodec.Length + DescriptorCodec.Length;

    var Result = new float[ItemSize * Count + BoolCodec.Length];

    foreach (var I in Enumerable.Range(0, Count))
    {
      var ItemIsHot = I < BatchSize;
      var ItemOffset = I*ItemSize;
      BoolCodec.EncodeTo(ItemIsHot, Result[ItemOffset..]);
      ItemOffset += BoolCodec.Length;
      if (!ItemIsHot)
        continue;

      ItemNumberCodec.EncodeTo((short) (First + I), Result[ItemOffset..]);
      ItemOffset += ItemNumberCodec.Length;
      DescriptorCodec.EncodeTo(Options[I].Descriptor, Result[ItemOffset..]);
    }

    BoolCodec.EncodeTo(IsLast, Result[(Count * ItemSize)..]);

    return Result;
  }
}