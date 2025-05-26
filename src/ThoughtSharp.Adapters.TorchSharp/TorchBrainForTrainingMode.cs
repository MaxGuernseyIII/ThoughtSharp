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
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchBrainForTrainingMode(
  Sequential Model, 
  Device Device, 
  int StateSize,
  Func<Tensor, Tensor, Tensor> LossFunction) : TorchBrain(Model, Device, StateSize), Brain
{
  optim.Optimizer Optimizer { get; } = optim.Adam(Model.parameters());

  public Inference MakeInference(float[] Parameters)
  {
    return ExecuteInference(null, EmptyState, Parameters);
  }

  internal Inference ExecuteInference(
    TorchInferenceForTrainingMode? Predecessor,
    Tensor StateInputTensor,
    float[] Parameters)
  {
    var Tensors = Forward(StateInputTensor, Parameters);

    return new TorchInferenceForTrainingMode(this, Predecessor, Parameters, Tensors.State, Tensors.Product);
  }

  public override void Dispose()
  {
    Optimizer.Dispose();
  }

  public void ApplyLoss(Tensor TensorForBackPropagation, Tensor TensorWithExpectedValues)
  {
    var Loss = LossFunction(TensorForBackPropagation, TensorWithExpectedValues);
    Model.zero_grad();
    //Console.WriteLine($"Before step: {Model.parameters().First().data<float>()[0]}");
    Loss.backward();
    //foreach (var param in Model.parameters())
    //{
    //  var grad = param.grad;
    //  Console.WriteLine($"Requires grad: {param.requires_grad}, grad is null: {param.grad is null}");
    //  Console.WriteLine($"Grad norm: {(grad is null ? "null" : grad.norm().item<float>().ToString("F8"))}");
    //}
    Optimizer.step();
    //Console.WriteLine($"Sample weight: {Model.parameters().First().data<float>()[0]}");
    //Console.WriteLine($"After step: {Model.parameters().First().data<float>()[0]}");

    //foreach (var param in Model.parameters())
    //{
    //  var data = param.data<float>();
    //  Console.WriteLine(string.Join(", ", data.Take(5)));  // limit to a few entries
    //}

    //foreach (var param in Model.parameters())
    //{
    //  var grad = param.grad;
    //  Console.WriteLine($"Grad mean: {grad?.mean().item<float>()}, zero? {grad?.allclose(torch.zeros_like(grad))}");
    //}
  }
}