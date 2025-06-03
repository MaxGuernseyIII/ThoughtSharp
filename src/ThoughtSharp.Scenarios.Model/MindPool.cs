using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class MindPool(ImmutableDictionary<Type, MindPlace> Places)
{
  public object GetMind(Type MindType)
  {
    var Place = Places[MindType];
    var Brain = Place.MakeNewBrain();

    return Place.MakeNewMind(Brain);
  }
}