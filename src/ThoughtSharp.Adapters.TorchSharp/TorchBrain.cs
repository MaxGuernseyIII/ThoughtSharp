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

using System.Collections.Immutable;
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

  internal Tensor ConvertFloatsToTensor(Batch<float[]> JaggedTensor)
  {
    return ConvertFloatsToInput(JaggedTensor).Payload;
  }

  internal TorchInferenceParts Forward(TorchInferenceParts Input)
  {
    return Model.forward(Input);
  }

  public Inference MakeInference(Batch<float[]> Features, Batch<long[]> Tokens)
  {
    return ExecuteInference(null, ConvertFloatsToInput(Features));
  }

  internal Inference ExecuteInference(TorchInference? Predecessor, TorchInferenceParts Input)
  {
    using var Mode = EnterTrainingMode(false);

    var Output = Forward(Input);

    return new TorchInference(this, Predecessor, Input, Output);
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

  public TorchInferenceParts ConvertFloatsToInput(Batch<float[]> JaggedTensor)
  {
    var TensorShaped = new float[
      JaggedTensor.Sequences.Length,
      JaggedTensor.Sequences.Max(B => B.Steps.Count),
      JaggedTensor.Sequences.SelectMany(B => B.Steps).Max(R => R.Length)];

    var Sequences = JaggedTensor.Sequences.Select((TimeSequence, TimeSequenceNumber) => (TimeSequence, TimeSequenceNumber)).ToImmutableArray();
    foreach (var (TimeSequence, TimeSequenceNumber) in Sequences)
    foreach (var (TimeStep, TimeStepNumber) in TimeSequence.Steps.Select((TimeStep, TimeStepNumber) => (TimeStep, TimeStepNumber)))
    foreach (var (Feature, FeatureNumber) in TimeStep.Select((Feature, FeatureNumber) => (Feature, FeatureNumber)))
      TensorShaped[TimeSequenceNumber, TimeStepNumber, FeatureNumber] = Feature;

    return new()
    {
      Payload= tensor(TensorShaped, ScalarType.Float32).to(Device),
      SequenceLengths = tensor(Sequences.Select(S => S.TimeSequence.Steps.Count).ToArray(), dtype: int64),
      State = EmptyState
    };
  }
}