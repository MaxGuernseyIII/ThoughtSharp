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
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Google.Protobuf.Reflection;
using Tests.SampleScenarios;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

[TestClass]
public class AssemblyParsing
{
  public enum NodeType
  {
    Model,
    Directory,
    Curriculum,
    Capability,
    MindPlace,
    Behavior,
    CurriculumPhase
  }

  static readonly string RootNamespace = typeof(Anchor).Namespace!;
  Assembly LoadedAssembly = null!;

  ScenariosModel Model = null!;

  static readonly FetchDataFromNode<TrainingMetadata> FetchTrainingMetadata = new()
  {
    VisitCurriculumPhase = CurriculumPhase => CurriculumPhase.TrainingMetadata,
    VisitCurriculum = Curriculum => Curriculum.TrainingMetadata,
    VisitBehavior = _ => AssemblyParser.StandardTrainingMetadata,
    VisitCapability = _ => AssemblyParser.StandardTrainingMetadata,
    VisitDirectory = _ => AssemblyParser.StandardTrainingMetadata,
    VisitMindPlace = _ => AssemblyParser.StandardTrainingMetadata,
    VisitModel = _ => AssemblyParser.StandardTrainingMetadata,
  };

  [TestInitialize]
  public void SetUp()
  {
    Model = null!;
    var Path = typeof(Anchor).Assembly.Location;
    var Context = new ShapingAssemblyLoadContext(Path, []);
    LoadedAssembly = Context.LoadFromAssemblyPath(Path);
  }

  [TestMethod]
  public void TraversalWorksAsExpected()
  {
    WhenParseAssembly();

    ThenTraversalWorksTheSameAsByBruteForce(RootNamespace, nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  [TestMethod]
  public void PathWorksAsExpected()
  {
    WhenParseAssembly();

    ThenGettingAPathWorksTheSameAsByBruteForce(RootNamespace, nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  [TestMethod]
  public void ModelProperties()
  {
    WhenParseAssembly();

    ThenModelNameIs("");
    ThenModelTypeIs(NodeType.Model);
  }

  [TestMethod]
  public void FindsDirectories()
  {
    WhenParseAssembly();

    ThenStructureContainsDirectory(
      RootNamespace,
      nameof(FizzBuzzTraining));
  }

  [TestMethod]
  public void FindsRootMindPlaces()
  {
    WhenParseAssembly();

    ThenStructureContainsMindPlace(
      RootNamespace,
      nameof(RootMindPlace));
  }

  [TestMethod]
  public void FindsCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculum(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan));
  }

  [TestMethod]
  public void DoesNotDuplicateCurriculaAtRoot()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan));
  }

  [TestMethod]
  public void DoesNotCreateEmptyNestedDirectories()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.EmptyClass));
  }

  [TestMethod]
  public void FindsCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Solution));
  }

  [TestMethod]
  public void IndexesWithFullPath()
  {
    WhenParseAssembly();

    ThenIndexedPathIsSameAsSelectionPath(RootNamespace, nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  void ThenIndexedPathIsSameAsSelectionPath(params ImmutableArray<string?> Path)
  {
    var Index = Model.FullPathIndex;

    ImmutableArray<ScenariosModelNode> Expected = [..Model.GetStepsAlongPath(Path).Skip(1)];

    var SubjectNode = GetNodeAtPath(Path);

    var Actual = Index[SubjectNode];
    Actual.Select(A => A.Name).Should().BeEquivalentTo(Expected.Select(E => E.Name), O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void FindsBehaviors()
  {
    WhenParseAssembly();

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Fizz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Buzz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.FizzBuzz));

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzbuzzScenarios),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
      nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.WriteValue));
  }

  [TestMethod]
  public void FindsInheritedBehaviors()
  {
    WhenParseAssembly();

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(DirectoryContainingCapabilityWithInheritedBehaviors),
      nameof(DirectoryContainingCapabilityWithInheritedBehaviors.CapabilityWithInheritedBehavior),
      nameof(ContainsInheritableBehavior.Behavior1));
  }

  [TestMethod]
  public void FindsCurriculumPhases()
  {
    WhenParseAssembly();

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.InitialSteps));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnBuzz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizzBuzz));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnWriting));

    ThenStructureContainsCurriculumPhase(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FinalTraining));
  }

  [TestMethod]
  public void IncludesCapabilitiesInPhases()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.InitialSteps)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations)
        ]
      ]);
  }

  // TODO: Decide if including a directory is important.
  //
  //[TestMethod]
  //public void IncludesCapabilitiesInDirectories()
  //{
  //  WhenParseAssembly();

  //  ThenIncludedNodesForPhaseIs(
  //    [
  //      RootNamespace,
  //      nameof(UsesInheritedBehaviors),
  //      nameof(UsesInheritedBehaviors.IncludesInheritorOfBehavior)
  //    ],
  //    [
  //      [
  //        RootNamespace,
  //        nameof(DirectoryContainingCapabilityWithInheritedBehaviors)
  //      ]
  //    ]);
  //}

  [TestMethod]
  public void InheritsDefaultDynamicWeights()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original,
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithImplicitDynamicWeights)
    );
  }

  [TestMethod]
  public void DefineExplicitWeights()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MinimumDynamicWeight = DynamicWeightingSample.CurriculumWithExplicitWeight.MinimumWeight,
        MaximumDynamicWeight = DynamicWeightingSample.CurriculumWithExplicitWeight.MaximumWeight
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.CurriculumWithExplicitWeight)
    );
  }

  [TestMethod]
  public void InheritDefineExplicitWeights()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original,
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.CurriculumWithExplicitWeight),
      nameof(DynamicWeightingSample.CurriculumWithExplicitWeight.PhaseWithImplicitDynamicWeights)
    );
  }

  [TestMethod]
  public void CanOverridePhaseDynamicMinimumWeight()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MinimumDynamicWeight = DynamicWeightingSample.Curriculum.PhaseWithExplicitMinimumWeight.MinimumWeight,
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithExplicitMinimumWeight)
    );
  }

  [TestMethod]
  public void CannotSetDynamicWeightLimitsTooLow()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MinimumDynamicWeight = .01,
        MaximumDynamicWeight = .01
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithLowMinimumAndMaximumWeights)
    );
  }

  [TestMethod]
  public void CanOverridePhaseDynamicMaximumWeight()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MaximumDynamicWeight = DynamicWeightingSample.Curriculum.PhaseWithExplicitMaximumWeight.MaximumWeight,
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithExplicitMaximumWeight)
    );
  }

  [TestMethod]
  public void CanNestedOverridePhaseDynamicMinimumWeight()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MinimumDynamicWeight = DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights.PhaseWithOverriddenMinimumWeight.OverriddenMinimumWeight,
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights.PhaseWithOverriddenMinimumWeight)
    );
  }

  [TestMethod]
  public void CanNestedOverridePhaseDynamicMaximumWeight()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        MaximumDynamicWeight = DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights.PhaseWithOverriddenMaximumWeight.OverriddenMaximumWeight,
      },
      RootNamespace,
      nameof(DynamicWeightingSample),
      nameof(DynamicWeightingSample.Curriculum),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights),
      nameof(DynamicWeightingSample.Curriculum.PhaseWithHeritableWeights.PhaseWithOverriddenMaximumWeight)
    );
  }

  [TestMethod]
  public void GetsPhaseMetadata()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original with
      {
        SuccessFraction = .6,
        SampleSize = 200,
      },
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.InitialSteps)
    );
  }

  [TestMethod]
  public void InheritedTrainingMetadata()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsIsChangedAt(
      Original => Original,
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
      nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizz)
    );
  }

  [TestMethod]
  public void DefaultTrainingMetadata()
  {
    WhenParseAssembly();

    ThenTrainingMetadataIsAt(
      new()
      {
        SuccessFraction = .95,
        SampleSize = 1000,
        MaximumAttempts = 500,
        MinimumDynamicWeight = .75,
        MaximumDynamicWeight = 1,
        Metric = Summarizers.Convergence.PassRate(1f)
      },
      RootNamespace,
      nameof(CurriculumWithDefaultPhase),
      nameof(CurriculumWithDefaultPhase.SomePhase)
    );
  }

  [TestMethod]
  public void PhasesAreOrderedByPhaseNumberRegardlessOfPositionInClassOrName()
  {
    WhenParseAssembly();

    ThenPhasesHaveOrder(
      [
        RootNamespace,
        nameof(CurriculumWithNumerousPhases)
      ],
      [
        nameof(CurriculumWithNumerousPhases.PhaseY),
        nameof(CurriculumWithNumerousPhases.PhaseZ),
        nameof(CurriculumWithNumerousPhases.PhaseX)
      ]
    );

    ThenPhasesHaveOrder(
      [
        RootNamespace,
        nameof(CurriculumWithNumerousPhases),
        nameof(CurriculumWithNumerousPhases.PhaseZ)
      ],
      [
        nameof(CurriculumWithNumerousPhases.PhaseZ.Phase4),
        nameof(CurriculumWithNumerousPhases.PhaseZ.PhaseZA),
        nameof(CurriculumWithNumerousPhases.PhaseZ.Phase0)
      ]
    );
  }

  void ThenPhasesHaveOrder(ImmutableArray<string?> Path, ImmutableArray<string> ExpectedPhaseOrder)
  {
    var Node = GetNodeAtPath(Path);
    var Phases = Node.GetChildPhases();

    Phases.Select(P => P.Name).Should().BeEquivalentTo(ExpectedPhaseOrder, O => O.WithStrictOrdering());
  }

  [TestMethod]
  public void MultipleInclusionsAllowed()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FinalTraining)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations)
        ],
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Solution)
        ]
      ]);
  }

  [TestMethod]
  public void IncludesSpecificBehaviorsInPhases()
  {
    WhenParseAssembly();

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnFizz)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Fizz)
        ]
      ]);

    ThenIncludedNodesForPhaseIs(
      [
        RootNamespace,
        nameof(FizzBuzzTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining),
        nameof(FizzBuzzTraining.FizzBuzzTrainingPlan.FocusedTraining.FocusOnBuzz)
      ],
      [
        [
          RootNamespace,
          nameof(FizzBuzzTraining),
          nameof(FizzBuzzTraining.FizzbuzzScenarios),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations),
          nameof(FizzBuzzTraining.FizzbuzzScenarios.Calculations.Buzz)
        ]
      ]);
  }

  [TestMethod]
  public void DoesNotFindStaticBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.StaticBehavior));
  }

  [TestMethod]
  public void DoesNotFindNonPublicBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.InternalBehavior));
  }

  [TestMethod]
  public void DoesNotFindNonVoidBehaviors()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.IntBehavior));
  }

  [TestMethod]
  public void DoesNotFindMethodsWithoutBehaviorAttribute()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.NonBehaviorMethod));
  }

  [TestMethod]
  public void FindsAsyncTaskBehaviors()
  {
    WhenParseAssembly();

    ThenStructureContainsBehavior(
      RootNamespace,
      nameof(ArbitraryRootCapability),
      nameof(ArbitraryRootCapability.AsyncBehavior));
  }

  [TestMethod]
  public void FindsMindPlace()
  {
    WhenParseAssembly();

    ThenStructureContainsMindPlace(
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzMindPlace));
  }

  [TestMethod]
  public void InfersMindPlaceMindType()
  {
    WhenParseAssembly();

    ThenMindPlaceMindTypeIs(
      typeof(FizzBuzzTraining.FizzBuzzMind),
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzMindPlace));
  }

  [TestMethod]
  public void CapturesMindPlaceType()
  {
    WhenParseAssembly();

    ThenMindPlaceTypeIs(
      typeof(FizzBuzzTraining.FizzBuzzMindPlace),
      RootNamespace,
      nameof(FizzBuzzTraining),
      nameof(FizzBuzzTraining.FizzBuzzMindPlace));
  }

  [TestMethod]
  public void MindPlacesAreIndexedByType()
  {
    WhenParseAssembly();

    ThenMindPlaceInstanceForMindTypeIs<FizzBuzzTraining.FizzBuzzMind, FizzBuzzTraining.FizzBuzzMindPlace>();
  }

  void ThenMindPlaceInstanceForMindTypeIs<
    TMind,
    TMindPlace>()
    where TMind : Mind<TMind>
    where TMindPlace : MindPlace
  {
    var MindType = ReplaceProxyWithLoadedType(typeof(TMind))!;
    var MindPlaceType = typeof(TMindPlace);
    Model.MindPlaceIndex[MindType].Should().BeOfType(ReplaceProxyWithLoadedType(MindPlaceType));
  }

  [TestMethod]
  public void FindsNestedCapabilities()
  {
    WhenParseAssembly();

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability1));

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryDirectory),
      nameof(ArbitraryDirectory.ArbitraryCapability),
      nameof(ArbitraryDirectory.ArbitraryCapability.ArbitrarySubCapability2));
  }

  [TestMethod]
  public void FindsRootCapabilitiesAndCurricula()
  {
    WhenParseAssembly();

    ThenStructureContainsDirectory(
      RootNamespace);

    ThenStructureContainsCapability(
      RootNamespace,
      nameof(ArbitraryRootCapability));

    ThenStructureContainsCurriculum(
      RootNamespace,
      nameof(ArbitraryRootCurriculum));
  }

  [TestMethod]
  public void DoesNotFindClassesWithoutMeaningfulContent()
  {
    WhenParseAssembly();

    ThenStructureDoesNotContainNode(
      RootNamespace,
      nameof(Anchor));
  }

  void WhenParseAssembly()
  {
    Model = new AssemblyParser().Parse(LoadedAssembly);
  }

  void ThenStructureContainsBehavior(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Behavior, Path);
  }

  void ThenStructureContainsCapability(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Capability, Path);
  }

  void ThenStructureContainsCurriculumPhase(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.CurriculumPhase, Path);
  }

  void ThenStructureContainsCurriculum(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Curriculum, Path);
  }

  void ThenStructureHasNodeOfTypeAtPath(NodeType NodeType, params ImmutableArray<string?> Path)
  {
    var (Parent, Name) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().ContainSingle(HasNameAndType(Name, NodeType));
  }

  void ThenStructureContainsDirectory(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.Directory, Path);
  }

  void ThenModelNameIs(string Expected)
  {
    Model.Name.Should().Be(Expected);
  }

  void ThenStructureContainsMindPlace(params ImmutableArray<string?> Path)
  {
    ThenStructureHasNodeOfTypeAtPath(NodeType.MindPlace, Path);
  }

  void ThenModelTypeIs(NodeType Expected)
  {
    GetNodeType(Model).Should().Be(Expected);
  }

  void ThenTrainingMetadataIsAt(TrainingMetadata Expected, params ImmutableArray<string?> Path)
  {
    var Node = GetNodeAtPath(Path);
    var Actual = Node.Query(FetchTrainingMetadata);
    Actual.Should().Be(Expected);
  }

  void ThenTrainingMetadataIsIsChangedAt(Func<TrainingMetadata, TrainingMetadata> GetExpected, params ImmutableArray<string?> Path)
  {
    var ParentNode = GetNodeAtPath(Path[..^1]);
    var ParentMetadata = ParentNode.Query(FetchTrainingMetadata);
    var Node = GetNodeAtPath(Path);
    var Actual = Node.Query(FetchTrainingMetadata);
    var Expected = GetExpected(ParentMetadata);
    Actual.Should().Be(Expected);
  }

  static NodeType GetNodeType(ScenariosModelNode Node)
  {
    return Node.Query(new GetNodeTypeVisitor());
  }

  void ThenTraversalWorksTheSameAsByBruteForce(params ImmutableArray<string?> Path)
  {
    var Expected = GetNodeAtPathByBruteforce(Path);
    var Actual = Model.GetNodeAtEndOfPath(Path);

    Actual.Should().BeSameAs(Expected);
  }

  void ThenGettingAPathWorksTheSameAsByBruteForce(params ImmutableArray<string?> Path)
  {
    var Expected = GetNodePathByBruteforce(Path);
    var Actual = Model.GetStepsAlongPath(Path);

    Actual.Should().BeEquivalentTo(Expected, O => O.WithStrictOrdering());
  }

  static Expression<Func<ScenariosModelNode, bool>> HasNameAndType(string? Name, NodeType NodeType)
  {
    return I => I.Name == Name && GetNodeType(I) == NodeType;
  }

  (ScenariosModelNode Parent, string? FinalName) GetParentNodeAndFinalNodeName(ImmutableArray<string?> Path)
  {
    return (Parent: GetNodeAtPath(Path[..^1]), FinalName: Path[^1]);
  }

  ScenariosModelNode GetNodeAtPath(ImmutableArray<string?> Path)
  {
    return Model.GetNodeAtEndOfPath(Path);
  }

  ScenariosModelNode GetNodeAtPathByBruteforce(ImmutableArray<string?> Path)
  {
    return Path.Aggregate((ScenariosModelNode) Model,
      (Predecessor, Name) => Predecessor.ChildNodes.Single(N => N.Name == Name));
  }

  ImmutableArray<ScenariosModelNode> GetNodePathByBruteforce(ImmutableArray<string?> Path)
  {
    ImmutableArray<ScenariosModelNode> Result = [Model];
    return Path.Aggregate(Result,
      (Predecessor, Current) => [..Predecessor, Predecessor[^1].ChildNodes.Single(N => N.Name == Current)]);
  }

  void ThenStructureDoesNotContainNode(params ImmutableArray<string?> Path)
  {
    var (Parent, FinalName) = GetParentNodeAndFinalNodeName(Path);
    Parent.ChildNodes.Should().NotContain(I => I.Name == FinalName);
  }

  void ThenIncludedNodesForPhaseIs(ImmutableArray<string?> Path, ImmutableArray<ImmutableArray<string?>> ExpectedPaths)
  {
    var Node = GetNodeAtPath(Path);
    var ExpectedNodes = ExpectedPaths.Select(GetNodeAtPath).ToImmutableArray();

    var ActualNodes = Node.Query(
      new FetchDataFromNode<ImmutableArray<ScenariosModelNode?>>
      {
        VisitCurriculumPhase = CurriculumPhase =>
          [.. CurriculumPhase.IncludedTrainingScenarioNodeFinders.Select(Model.Query)]
      });
    ActualNodes.Should().BeEquivalentTo(ExpectedNodes);
  }

  void ThenMindPlaceTypeIs(Type ExpectedProxy, params ImmutableArray<string?> Path)
  {
    var Expected = ReplaceProxyWithLoadedType(ExpectedProxy);
    var MindPlaceNode = GetNodeAtPath(Path);
    var MindType = MindPlaceNode.Query(new FetchDataFromNode<Type>
    {
      VisitMindPlace = MindPlace => MindPlace.MindPlaceType
    });
    MindType.Should().Be(Expected);
  }

  void ThenMindPlaceMindTypeIs(Type ExpectedProxy, params ImmutableArray<string?> Path)
  {
    var Expected = ReplaceProxyWithLoadedType(ExpectedProxy);
    var MindPlaceNode = GetNodeAtPath(Path);
    var MindType = MindPlaceNode.Query(new FetchDataFromNode<Type>
    {
      VisitMindPlace = MindPlace => MindPlace.MindType
    });
    MindType.Should().Be(Expected);
  }

  Type? ReplaceProxyWithLoadedType(Type ExpectedProxy)
  {
    var Expected = LoadedAssembly.GetType(ExpectedProxy.FullName!);
    return Expected;
  }

  class GetNodeTypeVisitor : ScenariosModelNodeVisitor<NodeType>
  {
    public NodeType Visit(ScenariosModel Model)
    {
      return NodeType.Model;
    }

    public NodeType Visit(DirectoryNode Directory)
    {
      return NodeType.Directory;
    }

    public NodeType Visit(CurriculumNode Curriculum)
    {
      return NodeType.Curriculum;
    }

    public NodeType Visit(CapabilityNode Capability)
    {
      return NodeType.Capability;
    }

    public NodeType Visit(MindPlaceNode MindPlace)
    {
      return NodeType.MindPlace;
    }

    public NodeType Visit(BehaviorNode Behavior)
    {
      return NodeType.Behavior;
    }

    public NodeType Visit(CurriculumPhaseNode CurriculumPhase)
    {
      return NodeType.CurriculumPhase;
    }
  }
}