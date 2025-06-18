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

using ThoughtSharp.Runtime;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace ThoughtSharp.Adapters.TorchSharp;

public class TorchBrain(
  Module<TorchInferenceParts, TorchInferenceParts> Model, Device Device) : Brain
{
  Module<TorchInferenceParts, TorchInferenceParts> Model { get; } = Model;
  Device Device { get; } = Device;

  public void Save(string Path)
  {
    Model.save(Path);
  }

  public void Load(string Path)
  {
    Model.load(Path);
  }

  public IDisposable EnterTrainingMode(bool NewTrainingMode)
  {
    var CurrentlyTraining = Model.training;

    SetTrainingMode(NewTrainingMode);

    return new RestoreMode(this, CurrentlyTraining);
  }

  void SetTrainingMode(bool Training)
  {
    if (Training)
      Model.train();
    else
      Model.eval();
  }

  class RestoreMode(TorchBrain TorchBrain, bool OriginalTrainingMode) : IDisposable
  {
    public void Dispose()
    {
      TorchBrain.SetTrainingMode(OriginalTrainingMode);
    }
  }

  internal TorchInferenceStateNode EmptyState => new();
  readonly Lazy<optim.Optimizer> OptimizerLazy = new(() => optim.Adam(Model.parameters()));
  optim.Optimizer Optimizer => OptimizerLazy.Value;

  public void Dispose()
  {
    Model.Dispose();
    Optimizer.Dispose();
  }

  internal Tensor ConvertFloatsToTensor(float[][] Batches)
  {
    var TensorShaped = new float[Batches.Length, Batches.Max(B => B.Length)];

    foreach (var BatchNumber in Enumerable.Range(0, TensorShaped.GetLength(0)))
    foreach (var FeatureNumber in Enumerable.Range(0, Batches[BatchNumber].Length))
      TensorShaped[BatchNumber, FeatureNumber] = Batches[BatchNumber][FeatureNumber];

    return tensor(TensorShaped, ScalarType.Float32).to(Device);
  }

  internal TorchInferenceParts Forward(float[][] Batches, TorchInferenceStateNode? State)
  {
    return Model.forward(new()
    {
      Payload = ConvertFloatsToTensor(Batches),
      State = State
    });
  }

  public virtual Inference MakeInference(float[][] Batches)
  {
    return ExecuteInference(null, EmptyState, Batches);
  }

  internal Inference ExecuteInference(
    TorchInference? Predecessor,
    TorchInferenceStateNode? StateInputTensor,
    float[][] Batches)
  {
    using var Mode = EnterTrainingMode(false);

    var Tensors = Forward(Batches, StateInputTensor);

    return new TorchInference(this, Predecessor, Batches, Tensors.State, Tensors.Payload);
  }

  public void ApplyLoss(Tensor Loss)
  {
    Model.zero_grad();
    Loss.backward();
    Optimizer.step();
  }

  public Tensor GetInt64ScalarTensor(long RuleIndex)
  {
    return tensor([RuleIndex], ScalarType.Int64).to(Device);
  }
}