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
  protected Module<TorchInferenceParts, TorchInferenceParts> Model { get; } = Model;
  Device Device { get; } = Device;

  public void Save(string Path)
  {
    Model.save(Path);
  }

  public void Load(string Path)
  {
    Model.load(Path);
  }

  internal TorchInferenceStateNode EmptyState => new();
  protected optim.Optimizer Optimizer { get; } = optim.Adam(Model.parameters());

  public void Dispose()
  {
    Model.Dispose();
    Optimizer.Dispose();
  }

  internal Tensor ConvertFloatsToTensor(float[] Parameters)
  {
    return tensor(Parameters, ScalarType.Float32).unsqueeze(0).to(Device);
  }

  internal TorchInferenceParts Forward(float[] Parameters, TorchInferenceStateNode State)
  {
    return Model.forward(new()
    {
      Payload = ConvertFloatsToTensor(Parameters),
      State = State
    });
  }

  public virtual Inference MakeInference(float[] Parameters)
  {
    return ExecuteInference(null, EmptyState, Parameters);
  }

  internal Inference ExecuteInference(
    TorchInference? Predecessor,
    TorchInferenceStateNode StateInputTensor,
    float[] Parameters)
  {
    var Tensors = Forward(Parameters, StateInputTensor);

    return new TorchInference(this, Predecessor, Parameters, Tensors.State, Tensors.Payload);
  }

  public void ApplyLoss(Tensor Loss)
  {
    Model.zero_grad();
    Loss.backward();
    Optimizer.step();
  }
}