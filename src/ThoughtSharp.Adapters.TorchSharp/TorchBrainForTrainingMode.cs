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
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchBrainForTrainingMode(Sequential Model, Device Device, int StateSize) : TorchBrain(Model, Device, StateSize), Brain
{
  internal optim.Optimizer Optimizer { get; } = optim.Adam(Model.parameters());

  public Inference MakeInference(float[] Parameters)
  {
    return ExecuteInference(null, EmptyState, Parameters);
  }

  internal Inference ExecuteInference(
    TorchInferenceForTraining? Predecessor,
    Tensor StateInputTensor,
    float[] Parameters)
  {
    var Tensors = Forward(StateInputTensor, Parameters);

    return new TorchInferenceForTraining(this, Predecessor, Parameters, Tensors.State, Tensors.Product);
  }

  internal TorchInferenceParts Forward(Tensor StateInputTensor, float[] Parameters)
  {
    var ParametersInputTensor = ConvertFloatsToTensor(Parameters);
    var NewInput = cat([StateInputTensor, ParametersInputTensor], 1);
    var NewOutput = Model.forward(NewInput);
    var NewStateTensor = NewOutput.slice(1, 0, StateSize, 1);
    var NewProductTensor = NewOutput.slice(1, StateSize, NewOutput.size(1) - StateSize, 1);

    return new()
    {
      State = NewStateTensor,
      Product = NewProductTensor
    };
  }

  public void ApplyLoss(Tensor Loss)
  {
    Model.zero_grad();
    Loss.backward();
    Optimizer.step();
  }

  public override void Dispose()
  {
    Optimizer.Dispose();
  }
}