using System.Collections.Immutable;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios.Model;

public class MindPool(ImmutableDictionary<Type, MindPlace> Places)
{
  readonly Dictionary<Type, (Brain ToSave, object Mind)> Minds = [];

  public object GetMind(Type MindType)
  {
    if (Minds.TryGetValue(MindType, out var Fragments))
      return Fragments.Mind;
    
    var Place = Places[MindType];
    var Brain = Place.MakeNewBrain();

    Fragments = (Brain, Place.MakeNewMind(Brain));
    Minds.Add(MindType, Fragments);

    return Fragments.Mind;
  }

  public void Save()
  {
    foreach (var (Type,(ToSave, _)) in Minds) 
      Places[Type].SaveBrain(ToSave);
  }
}