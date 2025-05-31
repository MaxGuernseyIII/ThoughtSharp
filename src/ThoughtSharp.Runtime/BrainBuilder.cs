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

namespace ThoughtSharp.Runtime;

public sealed record BrainBuilder<BuiltBrain, BuiltModel>(
  BrainFactory<BuiltBrain, BuiltModel> Factory,
  int InputFeatures,
  int OutputFeatures)
{
  readonly BrainFactory<BuiltBrain, BuiltModel> Factory = Factory;
  readonly ModelConstructor Input = new VirtualConstructor(InputFeatures);

  ModelConstructor Constructor { get; init; } = new DefaultConstructor(Factory, InputFeatures, OutputFeatures);

  public BuiltBrain Build()
  {
    return Factory.CreateBrain(Constructor.Build());
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingStandard()
  {
    return this with {Constructor = new StandardConstructor(Factory, InputFeatures, OutputFeatures)};
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingSequence(
    Func<SequenceBuilder, SequenceBuilder> TransformSequenceBuilder)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(Factory, TransformSequenceBuilder(new(Factory, Input)), OutputFeatures)
    };
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingParallel(Func<ParallelBuilder, ParallelBuilder> Transform)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(Factory, Transform(new(Factory, Input)), OutputFeatures)
    };
  }

  public interface ModelConstructor
  {
    int OutputFeatures { get; }
    BuiltModel Build();
  }

  record DefaultConstructor(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int InputFeatures, int OutputFeatures)
    : ModelConstructor
  {
    public BuiltModel Build()
    {
      return BrainFactory.CreateLinear(InputFeatures, OutputFeatures);
    }
  }

  record StandardConstructor(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int InputFeatures, int OutputFeatures)
    : ModelConstructor
  {
    public BuiltModel Build()
    {
      var Layer1Features = InputFeatures * 20;
      var Layer2Features = (Layer1Features + OutputFeatures) / 2;
      return BrainFactory.CreateSequence(
        BrainFactory.CreateLinear(InputFeatures, Layer1Features),
        BrainFactory.CreateTanh(),
        BrainFactory.CreateLinear(Layer1Features, Layer2Features),
        BrainFactory.CreateTanh(),
        BrainFactory.CreateLinear(Layer2Features, OutputFeatures)
      );
    }
  }

  public sealed record SequenceBuilder(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    ImmutableArray<ModelConstructor> Constructors { get; init; } = [];

    public int OutputFeatures => Tail.OutputFeatures;

    BuiltModel ModelConstructor.Build()
    {
      return BrainFactory.CreateSequence(
      [
        ..Constructors.Select(M => M.Build())
      ]);
    }

    public SequenceBuilder AddLinear(int Features)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new LinearConstructor(BrainFactory, Tail, Features)
        ]
      };
    }

    public SequenceBuilder AddParallel(Func<ParallelBuilder, ParallelBuilder> Transform)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          Transform(new(BrainFactory, Tail))
        ]
      };
    }

    public SequenceBuilder AddGRU(int Features)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new GRUConstructor(BrainFactory, Tail, Features)
        ]
      };
    }

    ModelConstructor Tail => Constructors.LastOrDefault() ?? Predecessor;
  }

  sealed record LinearConstructor(
    BrainFactory<BuiltBrain, BuiltModel> BrainFactory,
    ModelConstructor Predecessor,
    int OutputFeatures) : ModelConstructor
  {
    BuiltModel ModelConstructor.Build()
    {
      return BrainFactory.CreateLinear(Predecessor.OutputFeatures, OutputFeatures);
    }
  }

  public sealed record ParallelBuilder(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    ImmutableArray<ModelConstructor> Paths { get; init; } = [];

    public int OutputFeatures => Paths.Select(P => P.OutputFeatures).Sum();

    BuiltModel ModelConstructor.Build()
    {
      return BrainFactory.CreateParallel(Paths.Select(P => P.Build()));
    }

    public ParallelBuilder AddPath(Func<SequenceBuilder, SequenceBuilder> Transform)
    {
      return this with {Paths = [..Paths, Transform(new(BrainFactory, Predecessor))]};
    }
  }

  sealed record VirtualConstructor(int OutputFeatures) : ModelConstructor
  {
    public BuiltModel Build()
    {
      throw new NotSupportedException("This is a placeholder on top of which other models should be built.");
    }
  }

  sealed record AdaptOutputConstructor(
    BrainFactory<BuiltBrain, BuiltModel> Factory,
    ModelConstructor CoreConstructor,
    int OutputFeatures) : ModelConstructor
  {
    public BuiltModel Build()
    {
      return Factory.CreateSequence(
        CoreConstructor.Build(),
        Factory.CreateLinear(CoreConstructor.OutputFeatures, OutputFeatures));
    }
  }

  sealed record GRUConstructor(
    BrainFactory<BuiltBrain, BuiltModel> BrainFactory,
    ModelConstructor Predecessor,
    int OutputFeatures) : ModelConstructor
  {
    public BuiltModel Build()
    {
      return BrainFactory.CreateGRU(Predecessor.OutputFeatures, OutputFeatures);
    }
  }
}