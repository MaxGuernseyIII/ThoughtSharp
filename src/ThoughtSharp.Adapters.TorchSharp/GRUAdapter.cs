﻿// MIT License
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

using TorchSharp;

namespace ThoughtSharp.Adapters.TorchSharp;

public class GRUAdapter : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly torch.nn.Module<torch.Tensor, torch.Tensor, (torch.Tensor Payload, torch.Tensor State)> Underlying;
  readonly int OutputFeatures;
  readonly int GRULayers;
  readonly torch.Device Device;

  public GRUAdapter(torch.nn.Module<torch.Tensor, torch.Tensor, (torch.Tensor Payload, torch.Tensor State)> Underlying,
    int OutputFeatures,
    int GRULayers,
    torch.Device Device,
    string Name = "_unnamed") : base(Name)
  {
    this.Underlying = Underlying;
    this.OutputFeatures = OutputFeatures;
    this.GRULayers = GRULayers;
    this.Device = Device;
    // ReSharper disable once VirtualMemberCallInConstructor
    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var InputTensor = Input.Payload;

    var Output = Underlying.forward(InputTensor, Input.State?.Value.FirstOrDefault() ?? torch.zeros(new long[] { 1, GRULayers, OutputFeatures, }, torch.ScalarType.Float32, Device));

    return Input with
    {
      Payload = Output.Payload,
      State = new(Output.State)
    };
  }
}