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

// ReSharper disable once UnusedMember.Global
public class TorchBrainForTrainingMode(
  Module<TorchInferenceParts, TorchInferenceParts> Model,
  Device Device,
  Func<TorchInferenceStateNode> MakeEmptyState) : TorchBrain(Model, Device, MakeEmptyState), Brain
{
  optim.Optimizer Optimizer { get; } = optim.Adam(Model.parameters());

  public Inference MakeInference(float[] Parameters)
  {
    return ExecuteInference(null, EmptyState, Parameters);
  }

  public override void Dispose()
  {
    Optimizer.Dispose();
  }

  internal Inference ExecuteInference(
    TorchInferenceForTrainingMode? Predecessor,
    TorchInferenceStateNode StateInputTensor,
    float[] Parameters)
  {
    var Tensors = Forward(Parameters, StateInputTensor);

    return new TorchInferenceForTrainingMode(this, Predecessor, Parameters, Tensors.State, Tensors.Payload);
  }

  public void ApplyLoss(Tensor Loss)
  {
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

public class StatePassThroughModule : Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly Module<Tensor, Tensor> Transformer;

  public StatePassThroughModule(Module<Tensor, Tensor> Transformer, string Name = "_unnamed") : base(Name)
  {
    this.Transformer = Transformer;

    // ReSharper disable once VirtualMemberCallInConstructor
    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    return Input with {Payload = Transformer.forward(Input.Payload)};
  }
}

public sealed class ParallelModule : Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly Module<TorchInferenceParts, TorchInferenceParts> Left;
  readonly Module<TorchInferenceParts, TorchInferenceParts> Right;

  public ParallelModule(
    Module<TorchInferenceParts, TorchInferenceParts> Left,
    Module<TorchInferenceParts, TorchInferenceParts> Right,
    string Name = "_unnamed") : base(Name)
  {
    this.Left = Left;
    this.Right = Right;

    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var LeftOutput = Left.forward(Input with { State = Input.State.Left! });
    var RightOutput = Right.forward(Input with{ State = Input.State.Right! });

    return new()
    {
      Payload = cat([LeftOutput.Payload, RightOutput.Payload], 1),
      State = new(null)
      {
        Left = LeftOutput.State,
        Right = RightOutput.State
      }
    };
  }
}

public class CompositeModule : Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly Module<TorchInferenceParts, TorchInferenceParts> First;
  readonly Module<TorchInferenceParts, TorchInferenceParts> Second;

  public CompositeModule(Module<TorchInferenceParts, TorchInferenceParts> First,
    Module<TorchInferenceParts, TorchInferenceParts> Second, string Name = "_unnamed") : base(Name)
  {
    this.First = First;
    this.Second = Second;

    // ReSharper disable once VirtualMemberCallInConstructor
    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var Intermediate = First.forward(Input);
    return Second.forward(Intermediate);
  }
}

public sealed class AdditionalDimensionForSubModule : Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly Module<TorchInferenceParts, TorchInferenceParts> Underlying;

  public AdditionalDimensionForSubModule(Module<TorchInferenceParts, TorchInferenceParts> Underlying,
    string Name = "_unnamed") : base(Name)
  {
    this.Underlying = Underlying;

    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var NewInput = Input.UnSqueeze();
    var Next = Underlying.forward(NewInput);

    return Next.Squeeze();
  }
}

public class DoubleTensorToTorchInferencePartsAdapter : Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly Module<Tensor, Tensor, (Tensor Payload, Tensor State)> Underlying;

  public DoubleTensorToTorchInferencePartsAdapter(Module<Tensor, Tensor, (Tensor Payload, Tensor State)> Underlying,
    string Name = "_unnamed") : base(Name)
  {
    this.Underlying = Underlying;
    // ReSharper disable once VirtualMemberCallInConstructor
    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    //Console.WriteLine($"Input: {string.Join(", ", Input.Payload.shape)}, State: {string.Join(", ", Input.State.State!.shape)}");

    var Output = Underlying.forward(Input.Payload, Input.State.State!);

    return new()
    {
      Payload = Output.Payload,
      State = new(Output.State)
    };
  }
}