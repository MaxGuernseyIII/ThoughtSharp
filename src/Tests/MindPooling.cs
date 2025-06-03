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
using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;
using ThoughtSharp.Scenarios.Model;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests;

[TestClass]
public class MindPooling
{
  public record MockMind(Brain Brain);
  public record MockMind2(Brain Brain);
  public record MockMind3(Brain Brain);

  public class MockMindPlace : MindPlace
  {
    public Queue<Brain> BrainsToMake { get; } = [];

    public Brain MakeNewBrain()
    {
      return BrainsToMake.Dequeue();
    }

    public object MakeNewMind(Brain Brain)
    {
      return new MockMind(Brain);
    }

    public List<Brain> Loaded { get; } = [];

    public void LoadSavedBrain(Brain ToLoad)
    {
      Loaded.Add(ToLoad);
    }

    public void SaveBrain(Brain ToSave)
    {
      Assert.Fail("Not supported, yet.");
    }
  }

  ImmutableDictionary<Type, MindPlace> Places = null!;

  [TestInitialize]
  public void SetUp()
  {
    Places = ImmutableDictionary<Type, MindPlace>.Empty;
  }

  [TestMethod]
  public void GeneratesMindsTheFirstTime()
  {
    var MindType = typeof(MockMind);
    GivenPlaceBinding(typeof(MockMind2));
    var MockPlace = GivenPlaceBinding(MindType);
    GivenPlaceBinding(typeof(MockMind3));
    var Brain = GivenPlaceWillMakeBrain(MockPlace);
    var Pool = GivenPool();

    var Actual = WhenMakeMind(Pool, MindType);

    ThenMindShouldBe(Actual, new MockMind(Brain));
  }

  static void ThenMindShouldBe(object Actual, object Expected)
  {
    Actual.Should().Be(Expected);
  }

  static object WhenMakeMind(MindPool Pool, Type MindType)
  {
    var Actual = Pool.GetMind(MindType);
    return Actual;
  }

  MindPool GivenPool()
  {
    var Pool = new MindPool(Places);
    return Pool;
  }

  static DummyBrain GivenPlaceWillMakeBrain(MockMindPlace MockPlace)
  {
    var Brain = new DummyBrain();
    MockPlace.BrainsToMake.Enqueue(Brain);
    return Brain;
  }

  MockMindPlace GivenPlaceBinding(Type MindType)
  {
    var MockPlace = new MockMindPlace();
    Places = Places.Add(MindType, MockPlace);
    return MockPlace;
  }

  class DummyBrain : Brain
  {
    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public Inference MakeInference(float[] Parameters)
    {
      throw new NotImplementedException();
    }
  }
}