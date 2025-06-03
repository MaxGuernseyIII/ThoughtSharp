using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios.Model;

public class MindPool(ImmutableDictionary<Type, MindPlace> Places)
{
  readonly Dictionary<Type, object> Minds = [];

  public object GetMind(Type MindType)
  {
    if (Minds.TryGetValue(MindType, out var Mind))
      return Mind;
    
    var Place = Places[MindType];
    var Brain = Place.MakeNewBrain();

    Mind = Place.MakeNewMind(Brain);
    Minds.Add(MindType, Mind);

    return Mind;
  }
}