namespace ThoughtSharp.Runtime;

public sealed record BrainBuilder<BuiltBrain, BuiltModel>(BrainFactory<BuiltBrain, BuiltModel> Factory, int InputFeatures, int OutputFeatures)
{
  public interface ModelConstructor
  {
    BuiltBrain Build();
  }

  readonly BrainFactory<BuiltBrain, BuiltModel> Factory = Factory;
  ModelConstructor Constructor { get; init; } = null!;

  public BuiltBrain Build()
  {
    return Constructor.Build();
  }

  public BrainBuilder<BuiltBrain, BuiltModel> UsingStandard()
  {
    return this with {Constructor = new StandardConstructor(Factory, InputFeatures, OutputFeatures) };
  }

  class StandardConstructor(BrainFactory<BuiltBrain, BuiltModel> BrainFactory, int InputFeatures, int OutputFeatures) : ModelConstructor
  {
    public BuiltBrain Build()
    {
      var Layer1Features = InputFeatures * 20;
      var Layer2Features = (Layer1Features + OutputFeatures) / 2;
      return BrainFactory.CreateBrain(
        BrainFactory.CreateSequence(
          BrainFactory.CreateLinear(InputFeatures, Layer1Features),
          BrainFactory.CreateTanh(),
          BrainFactory.CreateLinear(Layer1Features, Layer2Features),
          BrainFactory.CreateTanh(),
          BrainFactory.CreateLinear(Layer2Features, OutputFeatures)
        ));
    }
  }
}