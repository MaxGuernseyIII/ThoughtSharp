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

namespace Tests;

[TestClass]
public class BehaviorRunning
{
  [TestInitialize]
  public void SetUp()
  {
  }

  [TestMethod]
  public async Task MindsAreBoundIn()
  {
    MindCatchingHost.SetBox(new());
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty
      .Add(typeof(Mind1), MockMindPlace.ForBrain(new DummyBrain(), B => new Mind1(B)))
      .Add(typeof(Mind2), MockMindPlace.ForBrain(new DummyBrain(), B => new Mind2(B))));
    var Mind1 = Pool.GetMind(typeof(Mind1));
    var Mind2 = Pool.GetMind(typeof(Mind2));
    var Runner = new BehaviorRunner(Pool, typeof(MindCatchingHost),
      typeof(MindCatchingHost).GetMethod(nameof(MindCatchingHost.AlwaysSucceed))!);

    await Runner.Run();

    (MindCatchingHost.LastInstance?.Mind1).Should().BeSameAs(Mind1);
    (MindCatchingHost.LastInstance?.Mind2).Should().BeSameAs(Mind2);
  }

  [TestMethod]
  public async Task RunSynchronous()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.Behavior))!);
    var WasRun = false;
    BehaviorTestingHost.BehaviorAction = () => { WasRun = true; };

    await Runner.Run();

    WasRun.Should().BeTrue();
  }

  [TestMethod]
  public async Task RunSynchronousSuccess()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.Behavior))!);

    BehaviorTestingHost.BehaviorAction = () => { };

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
  }

  [TestMethod]
  public async Task RunSynchronousFail()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.Behavior))!);

    var Exception = new Exception();
    BehaviorTestingHost.BehaviorAction = () => throw Exception;

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Exception.Should().Be(Exception);
  }

  [TestMethod]
  public async Task RunAsynchronous()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.AsyncBehavior))!);
    var WasRun = false;
    BehaviorTestingHost.AsyncBehaviorAction = () =>
    {
      return Task.Run(() =>
      {
        WasRun = true;
        return Task.CompletedTask;
      });
    };

    await Runner.Run();

    WasRun.Should().BeTrue();
  }

  [TestMethod]
  public async Task RunAsynchronousSuccess()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.AsyncBehavior))!);
    BehaviorTestingHost.AsyncBehaviorAction = () => { return Task.Run(() => Task.CompletedTask); };

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
  }

  [TestMethod]
  public async Task RunAsynchronousFailure()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(BehaviorTestingHost),
      typeof(BehaviorTestingHost).GetMethod(nameof(BehaviorTestingHost.AsyncBehavior))!);
    var Exception = new Exception();
    BehaviorTestingHost.AsyncBehaviorAction = () => { return Task.Run(() => Task.FromException(Exception)); };

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Exception.Should().Be(Exception);
  }

  [TestMethod]
  public void GetRunnerFromNode()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var HostType = typeof(BehaviorTestingHost);
    var MethodInfo = HostType.GetMethod(nameof(BehaviorTestingHost.Behavior))!;
    var Node = new BehaviorNode(HostType, MethodInfo);

    ImmutableArray<(ScenariosModelNode Node, BehaviorRunner Runner)> Runners = [.. Node.GetBehaviorRunners(Pool)];

    Runners.Should().BeEquivalentTo([(Node, new BehaviorRunner(Pool, HostType, MethodInfo))]);
  }

  [TestMethod]
  public void GetRunnersFromChildNodes()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var HostType1 = typeof(BehaviorTestingHost);
    var MethodInfo1 = HostType1.GetMethod(nameof(BehaviorTestingHost.Behavior))!;
    var MethodInfo2 = HostType1.GetMethod(nameof(BehaviorTestingHost.AsyncBehaviorAction))!;
    var HostType2 = typeof(MindCatchingHost);
    var MethodInfo3 = HostType1.GetMethod(nameof(MindCatchingHost.AlwaysSucceed))!;
    var Node1 = new BehaviorNode(HostType1, MethodInfo1);
    var Node2 = new BehaviorNode(HostType1, MethodInfo2);
    var Node3 = new BehaviorNode(HostType2, MethodInfo3);
    var Node =
      new CapabilityNode(null!, [
        new CapabilityNode(HostType1, [
          Node1,
          Node2
        ]),
        new CapabilityNode(HostType2, [
          Node3
        ])
      ]);

    ImmutableArray<(ScenariosModelNode Node, BehaviorRunner Runner)> Runners = [.. Node.GetBehaviorRunners(Pool)];

    Runners.Should().BeEquivalentTo([
      (Node1, new BehaviorRunner(Pool, HostType1, MethodInfo1)),
      (Node2, new(Pool, HostType1, MethodInfo2)),
      (Node3, new(Pool, HostType2, MethodInfo3))
    ]);
  }

  [TestMethod]
  public void DoesNotDuplicateRunners()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var HostType1 = typeof(BehaviorTestingHost);
    var MethodInfo1 = HostType1.GetMethod(nameof(BehaviorTestingHost.Behavior))!;
    var MethodInfo2 = HostType1.GetMethod(nameof(BehaviorTestingHost.AsyncBehaviorAction))!;
    var HostType2 = typeof(MindCatchingHost);
    var MethodInfo3 = HostType1.GetMethod(nameof(MindCatchingHost.AlwaysSucceed))!;
    var Node1 = new BehaviorNode(HostType1, MethodInfo1);
    var Node2 = new BehaviorNode(HostType1, MethodInfo2);
    var Node3 = new BehaviorNode(HostType2, MethodInfo3);
    var Repeated = new CapabilityNode(HostType2, [
      Node3
    ]);
    ImmutableArray<ScenariosModelNode> Nodes =
    [
      new CapabilityNode(null!, [
        new CapabilityNode(HostType1, [
          Node1,
          Node2
        ]),
        Repeated
      ]),
      Repeated
    ];

    ImmutableArray<(ScenariosModelNode Node, BehaviorRunner Runner)> Runners = [.. Nodes.GetBehaviorRunners(Pool)];

    Runners.Should().BeEquivalentTo([
      (Node1, new BehaviorRunner(Pool, HostType1, MethodInfo1)),
      (Node2, new(Pool, HostType1, MethodInfo2)),
      (Node3, new(Pool, HostType2, MethodInfo3))
    ]);
  }

  [TestMethod]
  public void BuildsAutomationJobFromRunners()
  {
    var Model = new ScenariosModel([]);
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Reporter = new MockReporter();
    var HostType1 = typeof(BehaviorTestingHost);
    var MethodInfo1 = HostType1.GetMethod(nameof(BehaviorTestingHost.Behavior))!;
    var MethodInfo2 = HostType1.GetMethod(nameof(BehaviorTestingHost.AsyncBehaviorAction))!;
    var HostType2 = typeof(MindCatchingHost);
    var MethodInfo3 = HostType1.GetMethod(nameof(MindCatchingHost.AlwaysSucceed))!;
    ImmutableArray<ScenariosModelNode> Nodes =
    [
      new CapabilityNode(null!, [
        new CapabilityNode(HostType1, [
          new BehaviorNode(HostType1, MethodInfo1),
          new BehaviorNode(HostType1, MethodInfo2)
        ]),
        new CapabilityNode(HostType2, [
          new BehaviorNode(HostType2, MethodInfo3)
        ])
      ]),
      new CapabilityNode(HostType2, [
        new BehaviorNode(HostType2, MethodInfo3)
      ])
    ];

    var Scheme = new TrainingDataScheme(Any.TrainingMetadata(), Reporter);
    var Job = Model.GetTestPassFor(Pool, Scheme, Nodes);

    Job.Should().BeEquivalentTo(new AutomationPass([.. Nodes.GetBehaviorRunners(Pool)],
      new FalseGate(), Pool, Scheme));
  }

  public record Mind1(Brain Brain);

  public record Mind2(Brain Brain);

  public class MindCatchingHost
  {
    static readonly AsyncLocal<Box<MindCatchingHost?>> LastInstanceContainer = new();

    public MindCatchingHost(Mind1 Mind1, Mind2 Mind2)
    {
      this.Mind1 = Mind1;
      this.Mind2 = Mind2;
      LastInstance = this;
    }

    public Mind1 Mind1 { get; }
    public Mind2 Mind2 { get; }

    public static MindCatchingHost? LastInstance
    {
      get => LastInstanceContainer.Value?.Value;
      set => LastInstanceContainer.Value!.Value = value;
    }

    public void AlwaysSucceed()
    {
    }

    internal static void SetBox(Box<MindCatchingHost?> Box)
    {
      LastInstanceContainer.Value = Box;
    }
  }

  public class BehaviorTestingHost
  {
    static readonly AsyncLocal<Action> BehaviorActionContainer = new();
    static readonly AsyncLocal<Func<Task>> AsyncBehaviorActionContainer = new();

    public static Action BehaviorAction
    {
      get => BehaviorActionContainer.Value!;
      set => BehaviorActionContainer.Value = value;
    }

    public static Func<Task> AsyncBehaviorAction
    {
      get => AsyncBehaviorActionContainer.Value!;
      set => AsyncBehaviorActionContainer.Value = value;
    }

    public void Behavior()
    {
      BehaviorAction();
    }

    public Task AsyncBehavior()
    {
      return AsyncBehaviorAction();
    }
  }

  internal class Box<T>
  {
    public T Value { get; set; } = default!;
  }
}