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
public class TorchInference(
  Sequential Model,
  torch.optim.Optimizer Optimizer,
  torch.Tensor Input,
  torch.Tensor Output,
  int OutputLength) : Inference
{
  public ReadOnlySpan<float> Result => Output.squeeze(0).to(torch.CPU).data<float>().ToArray();

  public void Incentivize(float Reward, params IReadOnlyList<Range> Ranges)
  {
    if (Reward == 0)
      return;

    using var TrainingTarget = Output.detach().clone();

    var Indices = new List<long>();
    foreach (var Range in Ranges)
      for (var I = Range.Start.GetOffset(OutputLength); I < Range.End.GetOffset(OutputLength); ++I)
        Indices.Add(I);

    if (Indices.Count == 0)
      return;

    using var CachedOutput = Output.detach().clone();
    var IndicesTensor = torch.tensor(Indices.ToArray(), torch.ScalarType.Int64, CachedOutput.device);
    var CachedSlice = CachedOutput.index_select(1, IndicesTensor);
    var RerunSlice = Model.forward(Input).index_select(1, IndicesTensor);

    var Similarity = (CachedSlice * RerunSlice).sum() / (CachedSlice.norm() * RerunSlice.norm() + 1e-8);

    using var Loss = Reward * (Reward > 0 ? 1 - Similarity : Similarity);

    Model.zero_grad();
    Loss.backward();
    Optimizer.step();
  }

  public void Dispose()
  {
    Input.Dispose();
    Output.Dispose();
  }
}