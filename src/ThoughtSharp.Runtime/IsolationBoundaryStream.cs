using System.Collections.Immutable;

namespace ThoughtSharp.Runtime;

public class IsolationBoundaryStream
{
  readonly List<int> Written = [];

  public void WriteBoundary(int Boundary)
  {
    Written.Add(Boundary);
  }

  public ImmutableArray<int> Boundaries => [..Written];
}