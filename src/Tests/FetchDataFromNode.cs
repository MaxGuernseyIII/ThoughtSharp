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

using ThoughtSharp.Scenarios.Model;

namespace Tests;

class FetchDataFromNode<T> : ScenariosModelNodeVisitor<T>
{
  public Func<ScenariosModel, T> VisitModel { get; init; } = AssertFail;
  public Func<DirectoryNode, T> VisitDirectory { get; init; } = AssertFail;
  public Func<CurriculumNode, T> VisitCurriculum { get; init; } = AssertFail;
  public Func<CapabilityNode, T> VisitCapability { get; init; } = AssertFail;
  public Func<MindPlaceNode, T> VisitMindPlace { get; init; } = AssertFail;
  public Func<BehaviorNode, T> VisitBehavior { get; init; } = AssertFail;
  public Func<CurriculumPhaseNode, T> VisitCurriculumPhase { get; init; } = AssertFail;

  T ScenariosModelNodeVisitor<T>.Visit(ScenariosModel Model)
  {
    return VisitModel(Model);
  }

  T ScenariosModelNodeVisitor<T>.Visit(DirectoryNode Directory)
  {
    return VisitDirectory(Directory);
  }

  T ScenariosModelNodeVisitor<T>.Visit(CurriculumNode Curriculum)
  {
    return VisitCurriculum(Curriculum);
  }

  T ScenariosModelNodeVisitor<T>.Visit(CapabilityNode Capability)
  {
    return VisitCapability(Capability);
  }

  T ScenariosModelNodeVisitor<T>.Visit(MindPlaceNode MindPlace)
  {
    return VisitMindPlace(MindPlace);
  }

  T ScenariosModelNodeVisitor<T>.Visit(BehaviorNode Behavior)
  {
    return VisitBehavior(Behavior);
  }

  T ScenariosModelNodeVisitor<T>.Visit(CurriculumPhaseNode CurriculumPhase)
  {
    return VisitCurriculumPhase(CurriculumPhase);
  }

  static T AssertFail<U>(U _)
  {
    throw new AssertFailedException("Should not get here.");
  }
}