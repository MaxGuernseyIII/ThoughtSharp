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

using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class TrainingDataScheme(ScenariosModelNode Node, TrainingMetadata Metadata)
{
  public readonly ScenariosModelNode Node = Node;

  public TrainingMetadata Metadata { get; } = Metadata;
  readonly ConcurrentDictionary<ScenariosModelNode, ConvergenceTracker> Trackers = [];
  readonly ConcurrentDictionary<ScenariosModelNode, TrainingDataScheme> ChildSchemes = [];

  public Counter TimesSinceSaved { get; } = new();
  public Counter Attempts { get; } = new();

  public IEnumerable<ScenariosModelNode> TrackedNodes => [..Trackers.Keys];

  public IEnumerable<ScenariosModelNode> SubSchemeNodes => ChildSchemes.Keys;

  public ConvergenceTracker GetConvergenceTrackerFor(ScenariosModelNode Node)
  {
    return Trackers.GetOrAdd(Node, _ => new(Metadata.SampleSize, Metadata.Metric));
  }

  protected bool Equals(TrainingDataScheme Other)
  {
    return Metadata.Equals(Other.Metadata);
  }

  public override bool Equals(object? Obj)
  {
    if (Obj is null) return false;
    if (ReferenceEquals(this, Obj)) return true;
    if (Obj.GetType() != GetType()) return false;
    return Equals((TrainingDataScheme) Obj);
  }

  public override int GetHashCode()
  {
    return Metadata.GetHashCode();
  }

  public TrainingDataScheme GetChildScheme(ScenariosModelNode Node) => ChildSchemes.GetOrAdd(Node, _ => new(Node, Node.Query(Queries.GetTrainingMetadata).Single()));
}