using System.Collections.Immutable;

namespace ThoughtSharp.Runtime;

public sealed record BrainBuilder<BuiltBrain, BuiltModel>(BrainFactory<BuiltBrain, BuiltModel> Factory, int InputFeatures, int OutputFeatures)
{
  public interface ModelConstructor
  {
    BuiltModel Build();
  }

  readonly BrainFactory<BuiltBrain, BuiltModel> Factory = Factory;
  ModelConstructor Constructor { get; init; } = new DefaultConstructor(Factory, InputFeatures, OutputFeatures);

  public BuiltBrain Build()
  {
    return Factory.CreateBrain(Constructor.Build());
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingStandard()
  {
    return this with {Constructor = new StandardConstructor(Factory, InputFeatures, OutputFeatures) };
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingSequence(Func<SequenceBuilder, SequenceBuilder> TransformSequenceBuilder)
  {
    return this with {Constructor = TransformSequenceBuilder(new(Factory, InputFeatures, OutputFeatures))};
  }

  record DefaultConstructor(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int InputFeatures, int OutputFeatures) : ModelConstructor
  {
    public BuiltModel Build()
    {
      return BrainFactory.CreateLinear(InputFeatures, OutputFeatures);
    }
  }

  record StandardConstructor(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int InputFeatures, int OutputFeatures) : ModelConstructor
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

  public sealed record SequenceBuilder(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int NextInputFeatures, int FinalOutputFeatures) : ModelConstructor
  {
    ImmutableArray<BuiltModel> Models { get; init; } = [];

    BuiltModel ModelConstructor.Build()
    {
      return BrainFactory.CreateSequence(
        [
          ..Models,
          BrainFactory.CreateLinear(NextInputFeatures, FinalOutputFeatures)
        ]);
    }

    public SequenceBuilder AddLinear(int Features)
    {
      return this with { Models = [..Models, BrainFactory.CreateLinear(NextInputFeatures, Features)], NextInputFeatures = Features};
    }
  }
}