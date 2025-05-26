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

using TorchSharp;
using TorchSharp.Modules;

namespace ThoughtSharp.Adapters.TorchSharp;

public class TorchBrain(Sequential Model, torch.Device Device, int StateSize) : IDisposable
{
  internal Sequential Model { get; } = Model;
  internal torch.Device Device { get; } = Device;
  internal int StateSize { get; } = StateSize;

  public torch.Tensor EmptyState => torch.zeros(new long[] { 1, StateSize }, dtype: torch.ScalarType.Float32, device: Device);

  public virtual void Dispose()
  {
    Model.Dispose();
  }

  internal torch.Tensor ConvertFloatsToTensor(float[] Parameters)
  {
    return torch.tensor(Parameters, torch.ScalarType.Float32).unsqueeze(0).to(Device);
  }

  internal TorchInferenceParts Forward(torch.Tensor StateInputTensor, float[] Parameters)
  {
    var ParametersInputTensor = ConvertFloatsToTensor(Parameters);
    var NewInput = torch.cat([StateInputTensor, ParametersInputTensor], 1);
    var NewOutput = Model.forward(NewInput);
    var NewStateTensor = NewOutput.slice(1, 0, StateSize, 1);
    var NewProductTensor = NewOutput.slice(1, StateSize, NewOutput.size(1) - StateSize, 1);

    return new()
    {
      State = NewStateTensor,
      Product = NewProductTensor
    };
  }
}