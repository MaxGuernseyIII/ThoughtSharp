using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class CompoundIncrementable(params ImmutableArray<Incrementable> Counters) : Incrementable
{
  public void Increment()
  {
    foreach (var Incrementable in Counters) 
      Incrementable.Increment();
  }
}