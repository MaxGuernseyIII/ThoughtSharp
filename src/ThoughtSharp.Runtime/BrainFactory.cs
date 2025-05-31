namespace ThoughtSharp.Runtime;

public interface BrainFactory<
  out TBrain, 
  TModel,
  TDevice>
{
  TBrain CreateBrain(TModel Model, TDevice Device);

  TModel CreateSequence(params IEnumerable<TModel> Children);
  TModel CreateParallel(params IEnumerable<TModel> Children);

  TModel CreateLinear(int InputFeatures, int OutputFeatures);
  TModel CreateGRU(int InputFeatures, int OutputFeatures, TDevice Device);

  TModel CreateTanh();
  TModel CreateReLU();

  TDevice GetDefaultOptimumDevice();
  TDevice GetCPUDevice();
  TDevice GetCUDADevice();
}