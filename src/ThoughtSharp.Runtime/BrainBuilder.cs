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
    Constructor = new AdaptOutputConstructor(this,
      new SequenceConstructor(this, Input), true);
    IsolationBoundaries = [0, OutputFeatures];
    Device = this.Factory.GetDefaultOptimumDevice();
  }


  TDevice Device { get; init; }

  ModelConstructor Constructor { get; init; }

  public int InputFeatures { get; }
  public int OutputFeatures { get; }
  public ImmutableArray<int> IsolationBoundaries { get; init; }

  public string CompactDescriptiveText => Constructor.CompactDescriptiveText;


  public TBrain Build()
  {
    return Factory.CreateBrain(Constructor.Build(), Device);
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingSequence(
    Func<SequenceConstructor, SequenceConstructor> TransformSequenceBuilder,
    bool WithFinalBias = true)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(this, TransformSequenceBuilder(new(this, Input)), WithFinalBias)
    };
  }

  public BrainBuilder<TBrain, TModel, TDevice> UsingParallel(Func<ParallelConstructor, ParallelConstructor> Transform,
    bool WithFinalBias = true)
  {
    return this with
    {
      Constructor = new AdaptOutputConstructor(this, Transform(new(this, Input)), WithFinalBias)
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
    string CompactDescriptiveText { get; }
    TModel Build();
  }

  public sealed record SequenceConstructor : ModelConstructor
  {
    readonly ImmutableArray<ModelConstructor> Predecessors;

    public SequenceConstructor(BrainBuilder<TBrain, TModel, TDevice> Host,
      ModelConstructor Predecessor)
    {
      this.Host = Host;
      Predecessors = [Predecessor];
    }

    ImmutableArray<ModelConstructor> Constructors { get; init; } = [];

    ModelConstructor Tail => Predecessors.Concat(Constructors).Last();
    public BrainBuilder<TBrain, TModel, TDevice> Host { get; init; }

    public int OutputFeatures => Tail.OutputFeatures;

    public string CompactDescriptiveText => string.Join("", Constructors.Select(C => C.CompactDescriptiveText));

    TModel ModelConstructor.Build()
    {
      return Host.Factory.CreateSequence(
      [
        ..Constructors.Select(M => M.Build())
      ]);
    }

    public SequenceConstructor AddLinear(int Features, bool WithBias = true)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new LinearConstructor(Host.Factory, Tail, Features, WithBias)
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
          new ReLUConstructor(Host.Factory, Tail)
        ]
      };
    }

    public SequenceConstructor AddSiLU()
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          new SiLUConstructor(Host.Factory, Tail)
        ]
      };
    }

    public SequenceConstructor AddTimeAware(
      Func<TimeAwareConstructor, TimeAwareConstructor> Configure)
    {
      return this with
      {
        Constructors =
        [
          ..Constructors,
          Configure(new(Host, Tail))
        ]
      };
    }
  }

  public record TimeAwareConstructor : ModelConstructor
  {
    public TimeAwareConstructor(BrainBuilder<TBrain, TModel, TDevice> Host, ModelConstructor Predecessor)
    {
      this.Host = Host;
      Predecessors = [Predecessor];
      Pooling = new MeanOverTimeStepsPoolingConstructor(Host, Tail);
    }

    BrainBuilder<TBrain, TModel, TDevice> Host { get; }
    ModelConstructor Pooling { get; init; }
    ImmutableArray<ModelConstructor> Constructors { get; init; } = [];
    ModelConstructor Tail => Predecessors.Concat(Constructors).Last();

    ImmutableArray<ModelConstructor> Predecessors { get; }

    public int OutputFeatures => Tail.OutputFeatures;

    public string CompactDescriptiveText =>
      string.Join(string.Empty, Constructors.Select(C => C.CompactDescriptiveText));

    public TModel Build()
    {
      return Host.Factory.CreateTimeAware(Constructors.Select(C => C.Build()), Pooling.Build());
    }

    public TimeAwareConstructor AddGRU(int Features, int Layers = 1)
    {
      var NewTail = new GRUConstructor(Host, Tail, Features, Layers);
      return this with
      {
        Pooling = new LatestTimeStepOfStatePoolingConstructor(Host, NewTail),
        Constructors =
        [
          ..Constructors,
          NewTail
        ]
      };
    }

    public TimeAwareConstructor AddAttention(int Heads, int FeaturesPerHead)
    {
      var NewTail = new AttentionConstructor(Host, Tail.OutputFeatures, Heads, FeaturesPerHead);
      return this with
      {
        Pooling = new AttentionPoolingConstructor(Host, NewTail),
        Constructors =
        [
          ..Constructors,
          NewTail
        ]
      };
    }

    class AttentionPoolingConstructor(BrainBuilder<TBrain, TModel, TDevice> Host, ModelConstructor Predecessor)
      : ModelConstructor
    {
      public int OutputFeatures => Predecessor.OutputFeatures;

      public string CompactDescriptiveText => "";

      public TModel Build()
      {
        return Host.Factory.CreateAttentionPooling(Predecessor.OutputFeatures);
      }
    }
  }

  sealed record LinearConstructor(
    BrainFactory<TBrain, TModel, TDevice> BrainFactory,
    ModelConstructor Predecessor,
    int OutputFeatures,
    bool WithBias) : ModelConstructor
  {
    public string CompactDescriptiveText => $"x{OutputFeatures}";

    TModel ModelConstructor.Build()
    {
      return BrainFactory.CreateLinear(Predecessor.OutputFeatures, OutputFeatures, WithBias);
    }
  }

  public sealed record ParallelConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor Predecessor)
    : ModelConstructor
  {
    ImmutableArray<ModelConstructor> Paths { get; init; } = [];

    public int OutputFeatures => Paths.Select(P => P.OutputFeatures).Sum();

    public string CompactDescriptiveText => string.Join("_", Paths.Select(P => P.CompactDescriptiveText));

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
    public string CompactDescriptiveText => "!";

    public TModel Build()
    {
      throw new NotSupportedException("This is a placeholder on top of which other models should be built.");
    }
  }

  class MeanOverTimeStepsPoolingConstructor(BrainBuilder<TBrain, TModel, TDevice> Host, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public string CompactDescriptiveText => "";

    public TModel Build()
    {
      return Host.Factory.CreateMeanOverTimeStepsPooling();
    }
  }

  class AttentionConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    int InputFeatures,
    int Heads,
    int FeaturesPerHead) : ModelConstructor
  {
    public int OutputFeatures => Heads * FeaturesPerHead;

    public string CompactDescriptiveText => $"a{Heads}-{FeaturesPerHead}";

    public TModel Build()
    {
      return Host.Factory.CreateMultiHeadedAttention(InputFeatures, Heads, FeaturesPerHead);
    }
  }

  record LatestTimeStepOfStatePoolingConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor Predecessor) : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public string CompactDescriptiveText => "";

    public TModel Build()
    {
      return Host.Factory.CreateLatestTimeStepInStatePooling();
    }
  }

  sealed record AdaptOutputConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor CoreConstructor,
    bool WithBias) : ModelConstructor
  {
    public int OutputFeatures => Host.OutputFeatures;

    public string CompactDescriptiveText => CoreConstructor.CompactDescriptiveText;

    public TModel Build()
    {
      var Pairwise = Host.IsolationBoundaries.Zip(Host.IsolationBoundaries.Skip(1)).Select(P => P.Second - P.First).ToImmutableArray();


      return Host.Factory.CreateSequence(
        CoreConstructor.Build(),
        Host.Factory.CreateParallel(
          Pairwise.Select(Length => Host.Factory.CreateLinear(CoreConstructor.OutputFeatures, Length, WithBias))
        )
      );
    }
  }

  sealed record GRUConstructor(
    BrainBuilder<TBrain, TModel, TDevice> Host,
    ModelConstructor Predecessor,
    int OutputFeatures,
    int Layers) : ModelConstructor
  {
    public string CompactDescriptiveText => $"s{OutputFeatures}";

    public TModel Build()
    {
      return Host.Factory.CreateGRU(Predecessor.OutputFeatures, OutputFeatures, Layers, Host.Device);
    }
  }

  sealed record TanhConstructor(BrainFactory<TBrain, TModel, TDevice> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public string CompactDescriptiveText => "[t]";

    public TModel Build()
    {
      return BrainFactory.CreateTanh();
    }
  }

  record ReLUConstructor(BrainFactory<TBrain, TModel, TDevice> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public string CompactDescriptiveText => "[r]";

    public TModel Build()
    {
      return BrainFactory.CreateReLU();
    }
  }

  sealed record SiLUConstructor(BrainFactory<TBrain, TModel, TDevice> BrainFactory, ModelConstructor Predecessor)
    : ModelConstructor
  {
    public int OutputFeatures => Predecessor.OutputFeatures;

    public string CompactDescriptiveText => "[s]";

    public TModel Build()
    {
      return BrainFactory.CreateSiLU();
    }
  }

  public BrainBuilder<TBrain, TModel, TDevice> WithIsolationBoundaries(params ImmutableArray<int> Boundaries)
  {
    return this with { IsolationBoundaries = Boundaries };
  }
}