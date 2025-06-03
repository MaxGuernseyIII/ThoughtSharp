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

namespace ThoughtSharp.Scenarios.Model;

public class ScenariosModel(IEnumerable<ScenariosModelNode> ChildNodes) : ScenariosModelNode
{
  public ImmutableDictionary<ScenariosModelNode, ImmutableArray<ScenariosModelNode>> FullPathIndex { get; } =
    GetFullPaths([], ChildNodes)
      .ToImmutableDictionary(Pair => Pair.Node, Pair => Pair.Path);

  public ImmutableDictionary<Type, MindPlace> MindPlaceIndex { get; } = GetMindPlaces(ChildNodes)
    .ToImmutableDictionary(Pair => Pair.MindType, Pair => Pair.Place);

  public IEnumerable<ScenariosModelNode> ChildNodes { get; } = ChildNodes;

  public string Name => "";

  public TResult Query<TResult>(ScenariosModelNodeVisitor<TResult> Visitor)
  {
    return Visitor.Visit(this);
  }

  static IEnumerable<(ScenariosModelNode Node, ImmutableArray<ScenariosModelNode> Path)> GetFullPaths(
    ImmutableArray<ScenariosModelNode> Base,
    ScenariosModelNode Node)
  {
    ImmutableArray<ScenariosModelNode> NewBase = [..Base, Node];

    yield return (Node, NewBase);

    foreach (var ValueTuple in GetFullPaths(NewBase, Node.ChildNodes)) yield return ValueTuple;
  }

  static IEnumerable<(ScenariosModelNode Node, ImmutableArray<ScenariosModelNode> Path)> GetFullPaths(
    ImmutableArray<ScenariosModelNode> Base, IEnumerable<ScenariosModelNode> Nodes)
  {
    foreach (var Result in Nodes.SelectMany(N => GetFullPaths(Base, N)))
      yield return Result;
  }

  static IEnumerable<(Type MindType, MindPlace Place)> GetMindPlaces(IEnumerable<ScenariosModelNode> Nodes)
  {
    static IEnumerable<(Type MindType, MindPlace Place)> CaptureResultForMindPlace(MindPlaceNode MindPlace)
    {
      yield return (MindPlace.MindType, (MindPlace) Activator.CreateInstance(MindPlace.MindPlaceType)!);
    }

    var Visitor = new CrawlingVisitor<(Type MindType, MindPlace Place)>
    {
      VisitMindPlace = CaptureResultForMindPlace
    };

    return Nodes.SelectMany(N => N.Query(Visitor));
  }
}

class CrawlingVisitor<T> : ScenariosModelNodeVisitor<IEnumerable<T>>
{
  public CrawlingVisitor()
  {
    VisitModel = Crawl;
    VisitDirectory = Crawl;
    VisitCurriculum = Crawl;
    VisitCapability = Crawl;
    VisitMindPlace = Crawl;
    VisitBehavior = Crawl;
    VisitCurriculumPhase = Crawl;
  }

  public Func<ScenariosModel, IEnumerable<T>> VisitModel { get; init; }
  public Func<DirectoryNode, IEnumerable<T>> VisitDirectory { get; init; }
  public Func<CurriculumNode, IEnumerable<T>> VisitCurriculum { get; init; }
  public Func<CapabilityNode, IEnumerable<T>> VisitCapability { get; init; }
  public Func<MindPlaceNode, IEnumerable<T>> VisitMindPlace { get; init; }
  public Func<BehaviorNode, IEnumerable<T>> VisitBehavior { get; init; }
  public Func<CurriculumPhaseNode, IEnumerable<T>> VisitCurriculumPhase { get; init; }

  public IEnumerable<T> Visit(ScenariosModel Model)
  {
    return VisitModel(Model);
  }

  public IEnumerable<T> Visit(DirectoryNode Directory)
  {
    return VisitDirectory(Directory);
  }

  public IEnumerable<T> Visit(CurriculumNode Curriculum)
  {
    return VisitCurriculum(Curriculum);
  }

  public IEnumerable<T> Visit(CapabilityNode Capability)
  {
    return VisitCapability(Capability);
  }

  public IEnumerable<T> Visit(MindPlaceNode MindPlace)
  {
    return VisitMindPlace(MindPlace);
  }

  public IEnumerable<T> Visit(BehaviorNode Behavior)
  {
    return VisitBehavior(Behavior);
  }

  public IEnumerable<T> Visit(CurriculumPhaseNode CurriculumPhase)
  {
    return VisitCurriculumPhase(CurriculumPhase);
  }

  IEnumerable<T> Crawl<U>(U ToCrawl)
    where U : ScenariosModelNode
  {
    return ToCrawl.ChildNodes.SelectMany(N => N.Query(this));
  }
}