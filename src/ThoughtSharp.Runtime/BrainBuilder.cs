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

public sealed record BrainBuilder<TBrain, TModel, TDevice>
{
  readonly BrainFactory<TBrain, TModel, TDevice> Factory;
  readonly ModelConstructor Input;

  public BrainBuilder(BrainFactory<TBrain, TModel, TDevice> Factory,
    int InputFeatures,
    int OutputFeatures)
  {
    this.InputFeatures = InputFeatures;
    this.OutputFeatures = OutputFeatures;
    this.Factory = Factory;
    Input = new VirtualConstructor(InputFeatures);
    Constructor = new AdaptOutputConstructor(Factory,
      new SequenceConstructor(this, new VirtualConstructor(InputFeatures)), OutputFeatures);
    Device = this.Factory.GetDefaultOptimumDevice();
  }

  TDevice Device { get; init; }

  ModelConstructor Constructor { get; init; }
  public int InputFeatures { get; init; }
  public int OutputFeatures { get; init; }

  public TBrain Build()
  {
    return Factory.CreateBrain(Constructor.Build(), Device);
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingSequence(
    Func<SequenceConstructor, SequenceConstructor> TransformSequenceBuilder)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(Factory, TransformSequenceBuilder(new(this, Input)), OutputFeatures)
    };
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingParallel(Func<ParallelConstructor, ParallelConstructor> Transform)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(Factory, Transform(new(this, Input)), OutputFeatures)
    };
  }

  public void Deconstruct(out BrainFactory<TBrain, TModel, TDevice> Factory, out int InputFeatures,
    out int OutputFeatures)
  {
    Factory = this.Factory;
    InputFeatures = this.InputFeatures;
    OutputFeatures = this.OutputFeatures;
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingCPU()
  {
    return this with {Device = Factory.GetCPUDevice()};
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingCUDA()
  {
    return this with {Device = Factory.GetCUDADevice()};
  }

  public interface ModelConstructor
  {
    int OutputFeatures { get; }
    TModel Build();
  }

  public sealed record SequenceConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor Predecessor)
    : ModelConstructor
  {
    ImmutableArray<ModelConstructor> Constructors { get; init; } = [];

    ModelConstructor Tail => Constructors.LastOrDefault() ?? Predecessor;

    public int OutputFeatures => Tail.OutputFeatures;

    TModel ModelConstructor.Build()
    {
      return Host.Factory.CreateSequence(
      [
        ..Constructors.Select(M => M.Build())
      ]);
    }

    public SequenceConstructor AddLinear(int Features)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new LinearConstructor(Host.Factory, Tail, Features)
        ]
      };
    }

    public SequenceConstructor AddParallel(Func<ParallelConstructor, ParallelConstructor> Transform)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          Transform(new(Host, Tail))
        ]
      };
    }

    public SequenceConstructor AddGRU(int Features)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new GRUConstructor(Host.Factory, Tail, Features)
        ]
      };
    }

    public SequenceConstructor AddTanh()
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new TanhConstructor(Host.Factory, Tail)
        ]
      };
    }

    public SequenceConstructor AddReLU()
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new ReLUConstructor(Host.Factory, Predecessor)
        ]
      };
    }
  }

  sealed record LinearConstructor(
    BrainFactory<TBrain, TModel, TDevice> BrainFactory,
    ModelConstructor Predecessor,
    int OutputFeatures) : ModelConstructor
  {
    TModel ModelConstructor.Build()
    {
      return BrainFactory.CreateLinear(Predecessor.OutputFeatures, OutputFeatures);
    }
  }

  public sealed record ParallelConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor Predecessor)
    : ModelConstructor
  {
    ImmutableArray<ModelConstructor> Paths { get; init; } = [];

    public int OutputFeatures => Paths.Select(P => P.OutputFeatures).Sum();

    TModel ModelConstructor.Build()
    {
      return Host.Factory.CreateParallel(Paths.Select(P => P.Build()));
    }

    public ParallelConstructor AddPath(Func<SequenceConstructor, SequenceConstructor> Transform)
    {
      return this with {Paths = [..Paths, Transform(new(Host, Predecessor))]};
    }
  }

  sealed record VirtualConstructor(int OutputFeatures) : ModelConstructor
  {
    public TModel Build()
    {
      throw new NotSupportedException("This is a placeholder on top of which other models should be built.");
    }
  }

  sealed record AdaptOutputConstructor(
    BrainFactory<TBrain, TModel, TDevice> Factory,
    ModelConstructor CoreConstructor,
    int OutputFeatures) : ModelConstructor
  {
    public TModel Build()
    {
      return Factory.CreateSequence(
        CoreConstructor.Build(),
        Factory.CreateLinear(CoreConstructor.OutputFeatures, OutputFeatures));
    }
  }

  sealed record GRUConstructor(
    BrainFactory<TBrain, TModel, TDevice> BrainFactory,
    ModelConstructor Predecessor,
    int OutputFeatures) : ModelConstructor
  {
    public TModel Build()
    {
      return BrainFactory.CreateGRU(Predecessor.OutputFeatures, OutputFeatures);
    }
  }

  sealed record TanhConstructor(BrainFactory<TBrain, TModel, TDevice> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public TModel Build()
    {
      return BrainFactory.CreateTanh();
    }
  }

  record ReLUConstructor(BrainFactory<TBrain, TModel, TDevice> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public TModel Build()
    {
      return BrainFactory.CreateReLU();
    }
  }
}