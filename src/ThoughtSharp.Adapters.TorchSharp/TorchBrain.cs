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

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchBrain(Sequential Model, int OutputLength, torch.Device? Device = null) : Brain
{
  readonly torch.Device Device = Device ?? torch.CPU;
  readonly torch.optim.Optimizer Optimizer = torch.optim.Adam(Model.parameters(), 0.001);

  public Inference MakeInference(float[] Parameters)
  {
    using var Input = torch.tensor(Parameters, torch.ScalarType.Float32).unsqueeze(0).to(Device);
    var Output = Model.forward(Input);

    return new TorchInference(Model, Optimizer, Input, Output, OutputLength);
  }

  public void Dispose()
  {
    Optimizer.Dispose();
    Model.Dispose();
  }
}