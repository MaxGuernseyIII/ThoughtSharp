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
using FluentAssertions.Execution;
using ThoughtSharp.Runtime;

// ReSharper disable NotAccessedPositionalProperty.Local

namespace Tests;

[TestClass]
public class BrainBuilding
{
  BrainBuilder<MockBuiltBrain, MockBuiltModel, MockDevice> BrainBuilder = null!;
  BrainFactory<MockBuiltBrain, MockBuiltModel, MockDevice> Factory = null!;
  int InputFeatures;
  int OutputFeatures;

  [TestInitialize]
  public void Setup()
  {
    InputFeatures = Any.Int(1, 200);
    OutputFeatures = Any.Int(1, 200);
    Factory = new MockFactory();
    BrainBuilder = new(Factory, InputFeatures, OutputFeatures);
  }

  [TestMethod]
  public void GetProperties()
  {
    BrainBuilder.InputFeatures.Should().Be(InputFeatures);
    BrainBuilder.OutputFeatures.Should().Be(OutputFeatures);
  }

  [TestMethod]
  public void Default()
  {
    var Actual = BrainBuilder.Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S).Build());
  }

  [TestMethod]
  public void SequenceWithoutFinalBias()
  {
    var Actual = BrainBuilder.UsingSequence(S => S, false).Build();

    ShouldBeAdaptedContainerFor(Actual, InputFeatures, Factory.GetDefaultOptimumDevice(), [], false);
  }

  [TestMethod]
  public void UsingCPU()
  {
    var Actual = BrainBuilder.UsingCPU().Build();

    ShouldBeAdaptedContainerFor(Actual, InputFeatures, Factory.GetCPUDevice());
  }

  [TestMethod]
  public void UsingCUDA()
  {
    var Actual = BrainBuilder.UsingCUDA().Build();

    ShouldBeAdaptedContainerFor(Actual, InputFeatures, Factory.GetCUDADevice());
  }

  [TestMethod]
  public void AddLogicLayers()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.UsingSequence(S => S.AddLogicLayers(LayerCounts)).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => LayerCounts.Aggregate(S,
      (Previous, Features) => Previous.AddLinear(Features * BrainBuilder.InputFeatures).AddReLU())).Build());
  }

  [TestMethod]
  public void AddDefaultLogicLayers()
  {
    var Actual = BrainBuilder.UsingSequence(S => S.AddLogicLayers()).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S.AddLogicLayers(4, 2)).Build());
  }

  [TestMethod]
  public void AddLogicPath()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.UsingParallel(P => P.AddLogicPath(LayerCounts)).Build();

    Actual.Should().Be(BrainBuilder.UsingParallel(P => P.AddPath(S => S.AddLogicLayers(LayerCounts)))
      .Build());
  }

  [TestMethod]
  public void ForLogic()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.ForLogic(LayerCounts).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S.AddLogicLayers(LayerCounts)).Build());
  }

  [TestMethod]
  public void AddMathLayers()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.UsingSequence(S => S.AddMathLayers(LayerCounts)).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => LayerCounts.Aggregate(S,
      (Previous, Features) => Previous.AddLinear(Features * BrainBuilder.InputFeatures).AddTanh())).Build());
  }

  [TestMethod]
  public void AddDefaultMathLayers()
  {
    var Actual = BrainBuilder.UsingSequence(S => S.AddMathLayers()).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S.AddMathLayers(4, 2)).Build());
  }

  [TestMethod]
  public void AddMathPath()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.UsingParallel(P => P.AddMathPath(LayerCounts)).Build();

    Actual.Should().Be(BrainBuilder.UsingParallel(P => P.AddPath(S => S.AddMathLayers(LayerCounts)))
      .Build());
  }

  [TestMethod]
  public void ForMath()
  {
    var LayerCounts = AnyLayerFeatureCounts(AtLeast: 1);

    var Actual = BrainBuilder.ForMath(LayerCounts).Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S.AddMathLayers(LayerCounts)).Build());
  }

  [TestMethod]
  public void UsingStandard()
  {
    using var Scope = new AssertionScope();

    var Actual = BrainBuilder.UsingStandard().Build();

    Actual.Should().Be(BrainBuilder.UsingSequence(S => S
        .AddLinear(InputFeatures * 20)
        .AddTanh()
        .AddLinear((InputFeatures * 20 + OutputFeatures) / 2)
        .AddTanh())
      .Build());
  }

  [TestMethod]
  public void UsingSequence()
  {
    var (FeatureLayerCounts, ExpectedLayers, ExpectedFinalLayer) = GivenExpectedSequenceAndFinalLayer();
    var Builder =
      BrainBuilder.UsingSequence(Sequence => ApplyFeatureLayerCountsToSequenceBuilder(FeatureLayerCounts, Sequence));

    var Actual = Builder.Build();

    Actual.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateSequence(ExpectedLayers),
          ExpectedFinalLayer),
        Factory.GetDefaultOptimumDevice()
      ));
  }

  [TestMethod]
  public void UsingParallel()
  {
    var Builder = BrainBuilder.UsingParallel(Parallel => Parallel
      .AddPath(Sequence => Sequence.AddLinear(5))
      .AddPath(Sequence => Sequence.AddLinear(10).AddLinear(4))
    );

    var Actual = Builder.Build();

    Actual.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateParallel(
            Factory.CreateSequence(
              Factory.CreateLinear(InputFeatures, 5)),
            Factory.CreateSequence(
              Factory.CreateLinear(InputFeatures, 10),
              Factory.CreateLinear(10, 4)
            )),
          Factory.CreateLinear(9, OutputFeatures)
        ),
        Factory.GetDefaultOptimumDevice()
      ));
  }

  [TestMethod]
  public void UsingParallelWithoutBias()
  {
    var Builder = BrainBuilder.UsingParallel(Parallel => Parallel
        .AddPath(Sequence => Sequence.AddLinear(5))
        .AddPath(Sequence => Sequence.AddLinear(10).AddLinear(4)),
      false
    );

    var Actual = Builder.Build();

    Actual.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateParallel(
            Factory.CreateSequence(
              Factory.CreateLinear(InputFeatures, 5)),
            Factory.CreateSequence(
              Factory.CreateLinear(InputFeatures, 10),
              Factory.CreateLinear(10, 4)
            )),
          Factory.CreateLinear(9, OutputFeatures, false)
        ),
        Factory.GetDefaultOptimumDevice()
      ));
  }

  [TestMethod]
  public void AddChildParallel()
  {
    var Layer1Features = 12;
    var Layer2A1Features = 5;
    var Layer2B1Features = 10;
    var Layer2B2Features = 4;
    var Layer3Features = 19;

    var Builder = BrainBuilder.UsingSequence(Sequence =>
      Sequence
        .AddLinear(Layer1Features)
        .AddParallel(Parallel => Parallel
          .AddPath(S => S.AddLinear(Layer2A1Features))
          .AddPath(S => S.AddLinear(Layer2B1Features).AddLinear(Layer2B2Features))
        )
        .AddLinear(Layer3Features)
    );

    var Actual = Builder.Build();

    Actual.Model.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateSequence(
            Factory.CreateLinear(InputFeatures, Layer1Features),
            Factory.CreateParallel(
              Factory.CreateSequence(
                Factory.CreateLinear(Layer1Features, Layer2A1Features)),
              Factory.CreateSequence(
                Factory.CreateLinear(Layer1Features, Layer2B1Features),
                Factory.CreateLinear(Layer2B1Features, Layer2B2Features)
              )),
            Factory.CreateLinear(Layer2A1Features + Layer2B2Features, Layer3Features)
          ),
          Factory.CreateLinear(Layer3Features, OutputFeatures)),
        Factory.GetDefaultOptimumDevice()
      ).Model);
  }

  MockDevice UpdateBrainBuilderToAnyDevice()
  {
    switch (Any.Int(0, 2))
    {
      case 1:
        BrainBuilder = BrainBuilder.UsingCPU();
        return Factory.GetCPUDevice();
      case 2:
        BrainBuilder = BrainBuilder.UsingCUDA();
        return Factory.GetCUDADevice();
      default:
        return Factory.GetDefaultOptimumDevice();
    }
  }

  [TestMethod]
  public void AddLinear()
  {
    var Count = Any.Int();
    var Actual = BrainBuilder.UsingSequence(S => S.AddLinear(Count)).Build();

    ShouldBeAdaptedContainerFor(Actual, Count, Factory.GetDefaultOptimumDevice(),
      Factory.CreateLinear(InputFeatures, Count, true));
  }

  [TestMethod]
  public void AddLinearWithoutBias()
  {
    var Count = Any.Int();
    var Actual = BrainBuilder.UsingSequence(S => S.AddLinear(Count, false)).Build();

    ShouldBeAdaptedContainerFor(Actual, Count, Factory.GetDefaultOptimumDevice(),
      Factory.CreateLinear(InputFeatures, Count, false));
  }

  [TestMethod]
  public void AddTanh()
  {
    var Actual = BrainBuilder.UsingSequence(S => S.AddLinear(2).AddTanh().AddLinear(1)).Build();

    ShouldBeAdaptedContainerFor(Actual, 1, Factory.GetDefaultOptimumDevice(), Factory.CreateLinear(InputFeatures, 2),
      Factory.CreateTanh(), Factory.CreateLinear(2, 1));
  }

  [TestMethod]
  public void AddReLU()
  {
    var Actual = BrainBuilder.UsingSequence(S => S.AddLinear(2).AddReLU().AddLinear(1)).Build();

    ShouldBeAdaptedContainerFor(Actual, 1, Factory.GetDefaultOptimumDevice(), Factory.CreateLinear(InputFeatures, 2),
      Factory.CreateReLU(), Factory.CreateLinear(2, 1));
  }

  [TestMethod]
  public void AddSiLU()
  {
    var Actual = BrainBuilder.UsingSequence(S => S.AddLinear(2).AddSiLU().AddLinear(1)).Build();

    ShouldBeAdaptedContainerFor(Actual, 1, Factory.GetDefaultOptimumDevice(), Factory.CreateLinear(InputFeatures, 2),
      Factory.CreateSiLU(), Factory.CreateLinear(2, 1));
  }

  [TestMethod]
  public void AddEmptyTimeAware()
  {
    var Device = UpdateBrainBuilderToAnyDevice();
    var Actual = BrainBuilder.UsingSequence(S => S.AddTimeAware(A => A)).Build();

    ShouldBeAdaptedContainerFor(Actual, InputFeatures,
      Device,
      Factory.CreateTimeAware([], Factory.CreateMeanOverTimeStepsPooling()));
  }

  [TestMethod]
  public void AddTimeAwareWithGRU()
  {
    var Device = UpdateBrainBuilderToAnyDevice();
    var GRUFeatures = Any.Int(100, 200);
    var GRULayers = Any.Int(1, 3);
    var Actual = BrainBuilder.UsingSequence(S => S.AddTimeAware(A => A.AddGRU(GRUFeatures, GRULayers))).Build();

    ShouldBeAdaptedContainerFor(Actual, GRUFeatures,
      Device,
      Factory.CreateTimeAware([
          Factory.CreateGRU(InputFeatures, GRUFeatures, GRULayers, Device)
        ],
        Factory.CreateLatestTimeStepInStatePooling()));
  }

  [TestMethod]
  public void AddTimeAwareWithGRUWithDefaultLayers()
  {
    var Device = UpdateBrainBuilderToAnyDevice();
    var GRUFeatures = Any.Int(100, 200);
    var Actual = BrainBuilder.UsingSequence(S => S.AddTimeAware(A => A.AddGRU(GRUFeatures))).Build();

    ShouldBeAdaptedContainerFor(Actual, GRUFeatures,
      Device,
      Factory.CreateTimeAware([
          Factory.CreateGRU(InputFeatures, GRUFeatures, 1, Device)
        ],
        Factory.CreateLatestTimeStepInStatePooling()));
  }

  void ShouldBeAdaptedContainerFor(MockBuiltBrain Actual, int Features, MockDevice Device,
    params IEnumerable<MockBuiltModel> ExpectedModels)
  {
    ShouldBeAdaptedContainerFor(Actual, Features, Device, ExpectedModels, true);
  }

  void ShouldBeAdaptedContainerFor(MockBuiltBrain Actual, int Features, MockDevice Device,
    IEnumerable<MockBuiltModel> ExpectedModels,
    bool WithBias)
  {
    Actual.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateSequence(
            ExpectedModels
          ),
          Factory.CreateLinear(Features, OutputFeatures, WithBias)),
        Device));
  }

  static BrainBuilder<MockBuiltBrain, MockBuiltModel, MockDevice>.SequenceConstructor
    ApplyFeatureLayerCountsToSequenceBuilder(
      List<int> FeatureLayerCounts,
      BrainBuilder<MockBuiltBrain, MockBuiltModel, MockDevice>.SequenceConstructor Sequence)
  {
    return FeatureLayerCounts.Aggregate(Sequence,
      (Previous, Features) => Previous.AddLinear(Features));
  }

  (List<int> FeatureLayerCounts, List<MockBuiltModel> ExpectedLayers, MockBuiltModel ExpectedFinalLayer)
    GivenExpectedSequenceAndFinalLayer()
  {
    var (FeatureLayerCounts, ExpectedLayers) = GetExpectedLayers();

    var LastOutputFeatures = FeatureLayerCounts.Any() ? FeatureLayerCounts[^1] : InputFeatures;

    return (FeatureLayerCounts, ExpectedLayers, Factory.CreateLinear(LastOutputFeatures, OutputFeatures));
  }

  (List<int> FeatureLayerCounts, List<MockBuiltModel> ExpectedLayers) GetExpectedLayers()
  {
    var LayerFeatureCounts = AnyLayerFeatureCounts();
    var Result = new List<(int InputFeatures, int OutputFeatures)>();
    var PreviousFeatureCount = InputFeatures;

    foreach (var LayerFeatureCount in LayerFeatureCounts)
    {
      Result.Add((PreviousFeatureCount, LayerFeatureCount));
      PreviousFeatureCount = LayerFeatureCount;
    }

    var ExpectedLayers = Result.Aggregate(new List<MockBuiltModel>(),
      (Previous, Mapping) => [.. Previous, Factory.CreateLinear(Mapping.InputFeatures, Mapping.OutputFeatures)]);
    return (FeatureLayerCounts: LayerFeatureCounts, ExpectedLayers);
  }

  static List<int> AnyLayerFeatureCounts(bool _ = false, int AtLeast = 1)
  {
    var LayerCount = Any.Int(AtLeast, 4);
    var LayerFeatureCounts = new List<int>();
    foreach (var __ in Enumerable.Range(0, LayerCount))
      LayerFeatureCounts.Add(Any.Int(1, 1000));
    return LayerFeatureCounts;
  }

  public record MockDevice;

  public class MockFactory : BrainFactory<MockBuiltBrain, MockBuiltModel, MockDevice>
  {
    public MockBuiltModel CreateLinear(int InputFeatures, int OutputFeatures, bool HasBias)
    {
      return new MockLinear(InputFeatures, OutputFeatures, HasBias);
    }

    public MockBuiltModel CreateTanh()
    {
      return new MockTanh();
    }

    public MockBuiltModel CreateSequence(params IEnumerable<MockBuiltModel> Children)
    {
      return new MockSequence([..Children]);
    }

    public MockBuiltBrain CreateBrain(MockBuiltModel Model, MockDevice Device)
    {
      return new(Model, Device);
    }

    public MockBuiltModel CreateParallel(params IEnumerable<MockBuiltModel> Children)
    {
      return new MockParallel(Children);
    }

    public MockBuiltModel CreateTimeAware(IEnumerable<MockBuiltModel> Children, MockBuiltModel Pooling)
    {
      return new MockTimeAware(Children, Pooling);
    }

    public MockBuiltModel CreateGRU(int InputFeatures, int OutputFeatures, int GRULayers, MockDevice Device)
    {
      return new MockGRU(InputFeatures, OutputFeatures, GRULayers, Device);
    }

    public MockBuiltModel CreateReLU()
    {
      return new MockReLU();
    }

    public MockBuiltModel CreateSiLU()
    {
      return new MockSiLU();
    }

    public MockBuiltModel CreateLatestTimeStepInStatePooling()
    {
      return new MockLatestTimeStepInStatePooling();
    }

    public MockBuiltModel CreateMeanOverTimeStepsPooling()
    {
      return new MockMeanOverTimeStepsPooling();
    }

    public MockBuiltModel CreateAttentionPooling(int InputFeatures)
    {
      return new MockAttentionPooling(InputFeatures);
    }

    public MockDevice GetDefaultOptimumDevice()
    {
      return new MockDefaultOptimalDevice();
    }

    public MockDevice GetCPUDevice()
    {
      return new MockCPUDevice();
    }

    public MockDevice GetCUDADevice()
    {
      return new MockCUDADevice();
    }

    sealed record MockLatestTimeStepInStatePooling : MockBuiltModel;

    sealed record MockMeanOverTimeStepsPooling : MockBuiltModel;

    sealed record MockAttentionPooling(int InputFeatures) : MockBuiltModel;

    sealed record MockCPUDevice : MockDevice;

    sealed record MockCUDADevice : MockDevice;

    sealed record MockDefaultOptimalDevice : MockDevice;

    sealed record MockSiLU : MockBuiltModel;

    record MockReLU : MockBuiltModel;

    record MockGRU(int InputFeatures, int OutputFeatures, int Layers, MockDevice Device) : MockBuiltModel;

    record MockLinear(int InputFeatures, int OutputFeatures, bool HasBias) : MockBuiltModel;

    record MockTanh : MockBuiltModel;

    record MockSequence(IReadOnlyList<MockBuiltModel> Children) : MockBuiltModel
    {
      public virtual bool Equals(MockSequence? Other)
      {
        if (Other is null) return false;
        if (ReferenceEquals(this, Other)) return true;
        return base.Equals(Other) && Children.SequenceEqual(Other.Children);
      }

      public override int GetHashCode()
      {
        return HashCode.Combine(base.GetHashCode(), Children);
      }
    }

    record MockParallel(IEnumerable<MockBuiltModel> Children) : MockBuiltModel
    {
      public virtual bool Equals(MockParallel? Other)
      {
        if (Other is null) return false;
        if (ReferenceEquals(this, Other)) return true;
        return base.Equals(Other) && Children.SequenceEqual(Other.Children);
      }

      public override int GetHashCode()
      {
        return HashCode.Combine(base.GetHashCode(), Children);
      }
    }

    sealed record MockTimeAware(IEnumerable<MockBuiltModel> Children, MockBuiltModel Pooling) : MockBuiltModel
    {
      public bool Equals(MockTimeAware? Other)
      {
        if (Other is null) return false;
        if (ReferenceEquals(this, Other)) return true;
        return base.Equals(Other) && Children.SequenceEqual(Other.Children) && Equals(Pooling, Other.Pooling);
      }

      public override int GetHashCode()
      {
        return HashCode.Combine(base.GetHashCode(), Children);
      }
    }
  }

  public record MockBuiltModel;

  public record MockBuiltBrain(MockBuiltModel Model, MockDevice Device) : Brain
  {
    public void Dispose()
    {
    }

    public Inference MakeInference(float[][] Parameters)
    {
      Assert.Fail();
      return null!;
    }
  }
}

file static class MockBrainFactoryExtensions
{
  public static BrainBuilding.MockBuiltModel CreateLinear(
    this BrainFactory<BrainBuilding.MockBuiltBrain, BrainBuilding.MockBuiltModel, BrainBuilding.MockDevice> Factory,
    int InputParameters, int OutputParameters)
  {
    return Factory.CreateLinear(InputParameters, OutputParameters, true);
  }
}