﻿namespace ThoughtSharp.Runtime;

public interface BrainFactory<
  out TBrain, 
  TModel,
  TDevice>
{
  TBrain CreateBrain(TModel Model, TDevice Device);

  TModel CreateSequence(params IEnumerable<TModel> Children);
  TModel CreateParallel(params IEnumerable<TModel> Children);
  TModel CreateTimeAware(IEnumerable<TModel> Children, TModel Pooling);

  TModel CreateLinear(int InputFeatures, int OutputFeatures, bool WithBias);
  TModel CreateGRU(int InputFeatures, int OutputFeatures, int GRULayers, TDevice Device);
  TModel CreateMultiHeadedAttention(int InputFeatures, int Heads, int FeaturesPerHead);

  TModel CreateDropout(float Rate);
  TModel CreateLayerNorm(int InputFeatures);

  TModel CreateLastTimeStep();
  TModel CreateMeanOverTimeStepsPooling();
  TModel CreateAttentionPooling(int InputFeatures);

  TModel CreateTanh();
  TModel CreateReLU();
  TModel CreateSiLU();

  TDevice GetDefaultOptimumDevice();
  TDevice GetCPUDevice();
  TDevice GetCUDADevice();
}