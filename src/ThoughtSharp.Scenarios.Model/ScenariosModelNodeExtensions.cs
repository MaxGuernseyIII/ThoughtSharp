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

public static class ScenariosModelNodeExtensions
{
  public static IEnumerable<CurriculumPhaseNode> GetChildPhases(this ScenariosModelNode Node)
  {
    return Node.ChildNodes.OfType<CurriculumPhaseNode>().OrderBy(N => N.PhaseNumber).ToImmutableArray();
  }

  public static IEnumerable<BehaviorRunner> GetBehaviorRunners(this ScenariosModelNode Node, MindPool Pool)
  {
    return Node.Query(new CrawlingVisitor<BehaviorRunner>() {VisitBehavior = VisitBehavior});

    IEnumerable<BehaviorRunner> VisitBehavior(BehaviorNode BehaviorNode)
    {
      yield return new(Pool, BehaviorNode.HostType, BehaviorNode.Method);
    }
  }

  public static IEnumerable<ScenariosModelNode> GetStepsAlongPath(this ScenariosModelNode Node,
    params ImmutableArray<string?> Steps)
  {
    var Current = Node;

    foreach (var Step in Steps)
    {
      yield return Current;
      Current = Current.ChildNodes.Single(N => N.Name == Step);
    }

    yield return Current;
  }

  public static ScenariosModelNode GetNodeAtEndOfPath(this ScenariosModelNode Node,
    params ImmutableArray<string?> Steps)
  {
    return Node.GetStepsAlongPath(Steps).Last();
  }
}