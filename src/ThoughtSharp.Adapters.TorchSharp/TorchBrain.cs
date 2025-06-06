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

  internal TorchInferenceStateNode EmptyState => new();
  readonly Lazy<optim.Optimizer> OptimizerLazy = new(() => optim.Adam(Model.parameters()));
  optim.Optimizer Optimizer => OptimizerLazy.Value;

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
    //foreach (var (name, param) in Model.named_parameters())
    //{
    //  if (param.grad is not null)
    //    Console.WriteLine($"{name}.grad.device: {param.grad.device}");
    //}

    //var optimizer = optim.Adam(Model.parameters(), lr: 1e-3);
    Loss.backward();
    //foreach (var (Name, Param) in Model.named_parameters())
    //{
    //  Console.WriteLine($"{Name}: {Param.device}");
    //}

    //foreach (var (name, param) in Model.named_parameters())
    //{
    //  if (param.grad is not null)
    //  {
    //    Console.WriteLine($"{name}: param.shape = {param.shape}, grad.shape = {param.grad.shape}, grad.device = {param.grad.device}");
    //  }
    //  else
    //  {
    //    Console.WriteLine($"{name}: grad is null");
    //  }
    //}

    //foreach (var (name, param) in Model.named_parameters())
    //{
    //  if (param is null)
    //  {
    //    Console.WriteLine($"{name}: param is null");
    //    continue;
    //  }

    //  try
    //  {
    //    var pShape = string.Join(", ", param.shape);
    //    var gShape = param.grad is not null ? string.Join(", ", param.grad.shape) : "no grad";
    //    Console.WriteLine($"{name}: param.shape = [{pShape}], grad.shape = [{gShape}], device = {param.device}");
    //  }
    //  catch (Exception ex)
    //  {
    //    Console.WriteLine($"{name}: shape check failed — {ex.Message}");
    //  }
    //}

    //foreach (var (Name, Param) in Model.named_parameters())
    //{
    //  if (Param.grad is not null)
    //  {
    //    Console.WriteLine($"{Name}: Param = [{string.Join(", ", Param.shape)}], Grad = [{string.Join(", ", Param.grad.shape)}]");
    //  }
    //}

    //optimizer.step();

    Optimizer.step();
  }

  public Tensor GetInt64ScalarTensor(long RuleIndex)
  {
    return tensor([RuleIndex], ScalarType.Int64).to(Device);
  }
}