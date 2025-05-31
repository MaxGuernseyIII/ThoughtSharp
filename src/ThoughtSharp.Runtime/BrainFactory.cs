namespace ThoughtSharp.Runtime;

public interface BrainFactory<out TBrain, TModel>
{
  TModel CreateLinear(int InputFeatures, int OutputFeatures);
  TModel CreateTanh();
  TModel CreateSequence(params IEnumerable<TModel> Children);
  TBrain CreateBrain(TModel Model);
  TModel CreateParallel(params IEnumerable<TModel> Children);
}