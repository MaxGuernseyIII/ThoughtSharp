﻿// MIT License
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
using Assert = ThoughtSharp.Scenarios.Assert;

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
    Result.Transcript.Should().Be(new Transcript([
      new() {Score = 1f, Annotations = []}
    ]));
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
    Result.Transcript.Should().Be(new Transcript([
      new() {Score = 0, Annotations = [$"unexpected exception of type {Exception.GetType().Name}"]}
    ]));
    Result.Exception.Should().Be(Exception);
  }

  [TestMethod]
  public async Task RunSynchronousFailureWithGrade()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnGrade))!);
    var Grade = Any.GradeOfAtMost(0.99f);
    SoftFailHost.SetGrade(Grade);
    
    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Transcript.Should().Be(new Transcript([Grade]));
  }

  [TestMethod]
  public async Task RunSynchronousSuccessWithGrade()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnGrade))!);
    var Grade = Any.GradeOfAtLeast(1);
    SoftFailHost.SetGrade(Grade);
    
    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
    Result.Transcript.Should().Be(new Transcript([Grade]));
  }

  [TestMethod]
  public async Task RunSynchronousFailureWithTranscript()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnTranscript))!);
    var Transcript = new Transcript([Any.GradeOfAtMost(0.99f)]); 
    SoftFailHost.SetTranscript(Transcript);

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Transcript.Should().Be(Transcript);
  }

  [TestMethod]
  public async Task RunSynchronousSuccessWithTranscript()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnTranscript))!);
    var Transcript = new Transcript([Any.GradeOfAtLeast(1f)]);
    SoftFailHost.SetTranscript(Transcript);
    
    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
    Result.Transcript.Should().Be(Transcript);
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
    Result.Transcript.Should().Be(new Transcript([
      new() {Score = 1f, Annotations = []}
    ]));
  }

  [TestMethod]
  public async Task CaptureOutput()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintIt))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureOutputWithGrade()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintItAndReturnGrade))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureOutputWithTranscript()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintItAndReturnTranscript))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureError()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintItToError))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureErrorWithGrade()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintItToErrorGrade))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureErrorWithTranscript()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintItToErrorTranscript))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected);
    });
  }

  [TestMethod]
  public async Task CaptureOutputAndErrorOnException()
  {
    await Isolated(async () =>
    {
      var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
      var Runner = new BehaviorRunner(Pool, typeof(ConsoleCatchingHost),
        typeof(ConsoleCatchingHost).GetMethod(nameof(ConsoleCatchingHost.PrintAndFail))!);
      var Expected = Any.NormalString;
      ConsoleCatchingHost.ToPrint = Expected;

      var Result = await Runner.Run();

      Result.Output.Should().Be(Expected + Expected);
    });
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
    Result.Transcript.Should().Be(new Transcript([
      new() {Score = 0, Annotations = [$"unexpected exception of type {Exception.GetType().Name}"]}
    ]));
    Result.Exception.Should().Be(Exception);
  }

  [TestMethod]
  public async Task RunAsynchronousFailureWithGrade()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnGradeAsync))!);

    var Grade = Any.GradeOfAtMost(0.999f);
    SoftFailHost.SetGrade(Grade);

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Transcript.Should().Be(new Transcript([Grade]));
  }

  [TestMethod]
  public async Task RunAsynchronousSuccessWithGrade()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnGradeAsync))!);

    var Grade = Any.GradeOfAtLeast(1f);
    SoftFailHost.SetGrade(Grade);

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
    Result.Transcript.Should().Be(new Transcript([Grade]));
  }

  [TestMethod]
  public async Task RunAsynchronousFailureWithTranscript()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnTranscriptAsync))!);

    var Transcript = new Transcript([Any.GradeOfAtMost(.999f)]);
    SoftFailHost.SetTranscript(Transcript);

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Failure);
    Result.Transcript.Should().Be(Transcript);
  }

  [TestMethod]
  public async Task RunAsynchronousSuccessWithTranscript()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(SoftFailHost),
      typeof(SoftFailHost).GetMethod(nameof(SoftFailHost.ReturnTranscriptAsync))!);
    var Transcript = new Transcript([Any.GradeOfAtLeast(1f)]);
    SoftFailHost.SetTranscript(Transcript);

    var Result = await Runner.Run();

    Result.Status.Should().Be(BehaviorRunStatus.Success);
    Result.Transcript.Should().Be(Transcript);
  }

  [TestMethod]
  public async Task FatalErrorInInitializer()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(CannotInitializeHost),
      typeof(CannotInitializeHost).GetMethod(nameof(CannotInitializeHost.Action))!);

    (await Runner.Invoking(R => R.Run()).Should()
      .ThrowAsync<FatalErrorException>())
      .WithMessage(CannotInitializeHost.ErrorMessage);
  }

  [TestMethod]
  public async Task FatalErrorInConstructor()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(CannotInstantiateHost),
      typeof(CannotInstantiateHost).GetMethod(nameof(CannotInstantiateHost.Action))!);

    (await Runner.Invoking(R => R.Run()).Should()
      .ThrowAsync<FatalErrorException>())
      .WithMessage(CannotInstantiateHost.ErrorMessage);
  }

  [TestMethod]
  public async Task FatalError()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.ThrowFatal))!);

    (await Runner.Invoking(R => R.Run()).Should()
      .ThrowAsync<FatalErrorException>())
      .WithMessage(FatalErrorHost.ErrorMessage);
  }

  [TestMethod]
  public async Task FatalErrorAsync()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.ThrowFatalAsync))!);

    (await Runner.Invoking(R => R.Run()).Should()
      .ThrowAsync<FatalErrorException>())
      .WithMessage(FatalErrorHost.ErrorMessage);
  }

  [TestMethod]
  public async Task CriticalFailure()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.CriticalFailure))!);

    (await Runner.Invoking(R => R.Run()).Should()
      .ThrowAsync<FatalErrorException>())
      .WithMessage($"Critical condition not met: {FatalErrorHost.CriticalCondition}");
  }

  [TestMethod]
  public async Task CriticalFailureAsync()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.CriticalFailureAsync))!);

    (await Runner.Invoking(R => R.Run()).Should()
        .ThrowAsync<FatalErrorException>())
      .WithMessage($"Critical condition not met: {FatalErrorHost.CriticalCondition}");
  }

  [TestMethod]
  public async Task CriticalSuccess()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.CriticalSuccess))!);

    await Runner.Invoking(R => R.Run()).Should().NotThrowAsync<FatalErrorException>();
  }

  [TestMethod]
  public async Task CriticalSuccessAsync()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var Runner = new BehaviorRunner(Pool, typeof(FatalErrorHost),
      typeof(FatalErrorHost).GetMethod(nameof(FatalErrorHost.CriticalSuccessAsync))!);

    await Runner.Invoking(R => R.Run()).Should().NotThrowAsync<FatalErrorException>();
  }

  [TestMethod]
  public void GetRunnerFromNode()
  {
    var Pool = new MindPool(ImmutableDictionary<Type, MindPlace>.Empty);
    var HostType = typeof(BehaviorTestingHost);
    var MethodInfo = HostType.GetMethod(nameof(BehaviorTestingHost.Behavior))!;
    var Node = new BehaviorNode(HostType, MethodInfo);

    ImmutableArray<(ScenariosModelNode Node, Runnable Runner)> Runners = [.. Node.GetBehaviorRunners(Pool)];

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

    ImmutableArray<(ScenariosModelNode Node, Runnable Runner)> Runners = [.. Node.GetBehaviorRunners(Pool)];

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

    ImmutableArray<(ScenariosModelNode Node, Runnable Runner)> Runners = [.. Nodes.GetBehaviorRunners(Pool)];

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
    var SaveGate = new MockGate();
    var Scheme = new TrainingDataScheme(new MockNode(), Any.TrainingMetadata());

    var Job = Model.GetTestPassFor(Pool, Scheme, SaveGate, Pool, Reporter, Nodes);

    Job.Should().BeEquivalentTo(
      new AutomationPass(
        [.. Nodes.GetBehaviorRunners(Pool)
          .Select(Tuple => Tuple with
          {
            Runner = new DynamicWeightedRunnable(
              Tuple.Runner, 
              Scheme.Metadata.MinimumDynamicWeight, 
              Scheme.Metadata.MaximumDynamicWeight, 
              Scheme.GetConvergenceTrackerFor(Tuple.Node), 
              Scheme.Metadata.SuccessFraction,
              new CompoundIncrementable(Scheme.TimesSinceSaved, Scheme.Attempts))
          })],
        SaveGate,
        Pool,
        Scheme,
        Reporter));
  }

  public record Mind1(Brain Brain);

  public record Mind2(Brain Brain);

  public class ConsoleCatchingHost
  {
    static readonly AsyncLocal<string?> ToPrintContainer = new();

    public static string? ToPrint
    {
      get => ToPrintContainer.Value;
      set => ToPrintContainer.Value = value;
    }

    public void PrintIt()
    {
      Console.Write(ToPrint);
    }

    public Grade PrintItAndReturnGrade()
    {
      Console.Write(ToPrint);

      return Any.Grade;
    }

    public Transcript PrintItAndReturnTranscript()
    {
      Console.Write(ToPrint);

      return new([Any.Grade]);
    }

    public Task PrintItToError()
    {
      return Console.Error.WriteAsync(ToPrint);
    }

    public async Task<Grade> PrintItToErrorGrade()
    {
      await Console.Error.WriteAsync(ToPrint);

      return Any.Grade;
    }

    public async Task<Transcript> PrintItToErrorTranscript()
    {
      await Console.Error.WriteAsync(ToPrint);

      return new([Any.Grade]);
    }

    public void PrintAndFail()
    {
      Console.Write(ToPrint);
      Console.Error.WriteAsync(ToPrint);
      throw new InvalidOperationException("Expected failure");
    }
  }

  public class CannotInitializeHost
  {
    public const string ErrorMessage = "condition Y";

    static CannotInitializeHost()
    {
      Assert.Fatal(ErrorMessage);
    }

    public void Action()
    {

    }
  }

  public class CannotInstantiateHost
  {
    public const string ErrorMessage = "condition Z";

    public CannotInstantiateHost()
    {
      Assert.Fatal(ErrorMessage);
    }

    public void Action()
    {

    }
  }

  public class FatalErrorHost
  {
    public const string ErrorMessage = "Something critical to the training infrastructure has failed.";
    public const string CriticalCondition = "condition X";

    public void ThrowFatal()
    {
      Assert.Fatal(ErrorMessage);
    }

    public Task ThrowFatalAsync()
    {
      Assert.Fatal(ErrorMessage);
      return Task.CompletedTask;
    }

    public void CriticalSuccess()
    {
      Assert.Critical(true, CriticalCondition);
    }

    public Task CriticalSuccessAsync()
    {
      Assert.Critical(true, CriticalCondition);
      return Task.CompletedTask;
    }

    public void CriticalFailure()
    {
      Assert.Critical(false, CriticalCondition);
    }

    public Task CriticalFailureAsync()
    {
      Assert.Critical(false, CriticalCondition);
      return Task.CompletedTask;
    }
  }

  public class SoftFailHost
  {

    static readonly AsyncLocal<Grade> GradeContainer = new();
    static readonly AsyncLocal<Transcript> TranscriptContainer = new();

    public static void SetGrade(Grade Grade)
    {
      GradeContainer.Value = Grade;
    }

    public static void SetTranscript(Transcript Transcript)
    {
      TranscriptContainer.Value = Transcript;
    }

    public Grade ReturnGrade()
    {
      return GradeContainer.Value!;
    }

    public Task<Grade> ReturnGradeAsync()
    {
      return Task.FromResult(GradeContainer.Value!);
    }

    public Transcript ReturnTranscript()
    {
      return TranscriptContainer.Value!;
    }

    public Task<Transcript> ReturnTranscriptAsync()
    {
      return Task.FromResult(TranscriptContainer.Value!);
    }
  }

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

  static Task Isolated(Func<Task> ToDo)
  {
    return ToDo();
  }
}