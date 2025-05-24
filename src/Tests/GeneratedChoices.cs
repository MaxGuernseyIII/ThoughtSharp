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
using FluentAssertions.Primitives;
using ThoughtSharp.Runtime;

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
    var Category = new MockCategory(Options);

    var Batches = Category.ToInputBatches();

    Batches.Should().MarshalToSameTensor<MockCategory.Input>([
      new()
      {
        IsFinalBatch = true,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 0,
            Descriptor = Options[0].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 1,
            Descriptor = Options[1].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 2,
            Descriptor = Options[2].Descriptor
          },
        ]
      }
    ]);
  }

  [TestMethod]
  public void EncodeOnlyPartialBatch()
  {
    IReadOnlyList<CognitiveOption<MockPayload, MockData>> Options =
    [
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
    ];
    var Category = new MockCategory(Options);

    var Batches = Category.ToInputBatches();

    Batches.Should().MarshalToSameTensor<MockCategory.Input>([
      new()
      {
        IsFinalBatch = true,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 0,
            Descriptor = Options[0].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 1,
            Descriptor = Options[1].Descriptor
          },
          new()
          {
            IsHot = false,
            ItemNumber = 0,
            Descriptor = new()
          }
        ]
      }
    ]);
  }

  [TestMethod]
  public void EncodeMultipleFullBatches()
  {
    IReadOnlyList<CognitiveOption<MockPayload, MockData>> Options =
    [
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
    ];
    var Category = new MockCategory(Options);

    var Batches = Category.ToInputBatches();

    Batches.Should().MarshalToSameTensor<MockCategory.Input>([
      new()
      {
        IsFinalBatch = false,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 0,
            Descriptor = Options[0].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 1,
            Descriptor = Options[1].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 2,
            Descriptor = Options[2].Descriptor
          }
        ]
      },
      new()
      {
        IsFinalBatch = true,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 3,
            Descriptor = Options[3].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 4,
            Descriptor = Options[4].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 5,
            Descriptor = Options[5].Descriptor
          }
        ]
      },
    ]);
  }

  [TestMethod]
  public void EncodeMultipleFullBatchesWithPartialAtEnd()
  {
    IReadOnlyList<CognitiveOption<MockPayload, MockData>> Options =
    [
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
    ];
    var Category = new MockCategory(Options);

    var Batches = Category.ToInputBatches();

    Batches.Should().MarshalToSameTensor<MockCategory.Input>([
      new()
      {
        IsFinalBatch = false,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 0,
            Descriptor = Options[0].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 1,
            Descriptor = Options[1].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 2,
            Descriptor = Options[2].Descriptor
          }
        ]
      },
      new()
      {
        IsFinalBatch = false,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 3,
            Descriptor = Options[3].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 4,
            Descriptor = Options[4].Descriptor
          },
          new()
          {
            IsHot = true,
            ItemNumber = 5,
            Descriptor = Options[5].Descriptor
          }
        ]
      },
      new()
      {
        IsFinalBatch = true,
        Items = [
          new()
          {
            IsHot = true,
            ItemNumber = 6,
            Descriptor = Options[6].Descriptor
          },
          new()
          {
            IsHot = false,
            ItemNumber = 0,
            Descriptor = new()
          },
          new()
          {
            IsHot = false,
            ItemNumber = 0,
            Descriptor = new()
          }
        ]
      },
    ]);
  }

  [TestMethod]
  public void MakeSelection()
  {
    var Category = new MockCategory([
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
      new(new(), new() {Parameter = Any.Float}),
    ]);
    var Index = (ushort) Any.Int(0, Category.AllOptions.Count - 1);
    var Output = new MockCategory.Output() {Selection = Index};

    var Selection = Category.Interpret(Output);

    Selection.Should().BeSameAs(Category.AllOptions[Index].Payload);
  }

  class MockPayload;

  [CognitiveData]
  partial class MockData
  {
    public float Parameter;
  }

  [CognitiveCategory<MockPayload, MockData>(3)]
  partial class MockCategory;
}