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
using Tests.Mocks;
using ThoughtSharp.Runtime;
// ReSharper disable NotAccessedPositionalProperty.Local

namespace Tests;

[TestClass]
public class BrainBuilding
{
  BrainFactory<MockBuiltBrain, MockBuiltModel> Factory = null!;
  int InputFeatures;
  int OutputFeatures;
  BrainBuilder<MockBuiltBrain, MockBuiltModel> BrainBuilder = null!;

  [TestInitialize]
  public void Setup()
  {
    InputFeatures = Any.Int(1, 200);
    OutputFeatures = Any.Int(1, 200);
    Factory = new MockFactory();
    BrainBuilder = new(Factory, InputFeatures, OutputFeatures);
  }

  [TestMethod]
  public void DefaultBuild()
  {
    var Actual = BrainBuilder.Build();

    var Expected =
      Factory.CreateBrain(
        Factory.CreateLinear(InputFeatures, OutputFeatures)
      );
    Actual.Should().Be(Expected);
  }

  [TestMethod]
  public void StandardBuild()
  {
    var Actual = BrainBuilder.UsingStandard().Build();

    var Expected =
      Factory.CreateBrain(
        Factory.CreateSequence(
          Factory.CreateLinear(InputFeatures, InputFeatures * 20),
          Factory.CreateTanh(),
          Factory.CreateLinear(InputFeatures * 20, (InputFeatures * 20 + OutputFeatures) / 2),
          Factory.CreateTanh(),
          Factory.CreateLinear((InputFeatures * 20 + OutputFeatures) / 2, OutputFeatures)
        )
      );
    Actual.Should().Be(Expected);
  }

  [TestMethod]
  public void UsingSequence()
  {
    var FeatureLayerCounts = AnyFeatureLayerCounts();
    var Mappings = GetExpectedSequenceLayersFromCounts(FeatureLayerCounts);
    var ExpectedLayers = Mappings.Aggregate(new List<MockBuiltModel>(),
      (Previous, Mapping) => [..Previous, Factory.CreateLinear(Mapping.InputFeatures, Mapping.OutputFeatures)]);
    var Builder = BrainBuilder.UsingSequence(Sequence => FeatureLayerCounts.Aggregate(Sequence,
      (Previous, Features) => Previous.AddLinear(Features)));

    var Actual = Builder.Build();

    Actual.Should().Be(
      Factory.CreateBrain(
        Factory.CreateSequence(ExpectedLayers)));
  }

  List<(int InputFeatures, int OutputFeatures)> GetExpectedSequenceLayersFromCounts(List<int> LayerFeatureCounts)
  {
    var Result = new List<(int InputFeatures, int OutputFeatures)>();
    var PreviousFeatureCount = InputFeatures;

    foreach (var LayerFeatureCount in LayerFeatureCounts)
    {
      Result.Add((PreviousFeatureCount, LayerFeatureCount));
      PreviousFeatureCount = LayerFeatureCount;
    }

    Result.Add((PreviousFeatureCount, OutputFeatures));

    return Result;
  }

  static List<int> AnyFeatureLayerCounts()
  {
    var LayerCount = Any.Int(0, 4);
    var LayerFeatureCounts = new List<int>();
    foreach (var I in Enumerable.Range(0, LayerCount)) 
      LayerFeatureCounts.Add(Any.Int(1, 1000));
    return LayerFeatureCounts;
  }

  class MockFactory : BrainFactory<MockBuiltBrain, MockBuiltModel>
  {
    public MockBuiltModel CreateLinear(int InputFeatures, int OutputFeatures)
    {
      return new MockLinear(InputFeatures, OutputFeatures);
    }

    record MockLinear(int InputFeatures, int OutputFeatures) : MockBuiltModel;

    public MockBuiltModel CreateTanh()
    {
      return new MockTanh();
    }

    record MockTanh : MockBuiltModel
    {
    }

    public MockBuiltModel CreateSequence(params IEnumerable<MockBuiltModel> Children)
    {
      return new MockSequence([..Children]);
    }

    record MockSequence(IReadOnlyList<MockBuiltModel> MockBuiltModels) : MockBuiltModel
    {
      public virtual bool Equals(MockSequence? Other)
      {
        if (Other is null) return false;
        if (ReferenceEquals(this, Other)) return true;
        return base.Equals(Other) && MockBuiltModels.SequenceEqual(Other.MockBuiltModels);
      }

      public override int GetHashCode()
      {
        return HashCode.Combine(base.GetHashCode(), MockBuiltModels);
      }
    }

    public MockBuiltBrain CreateBrain(MockBuiltModel Model)
    {
      return new(Model);
    }
  }

  record MockBuiltModel;

  record MockBuiltBrain(MockBuiltModel Model) : Brain
  {
    public void Dispose()
    {
    }

    public Inference MakeInference(float[] Parameters)
    {
      Assert.Fail();
      return new MockInference(0, Parameters);
    }
  }
}