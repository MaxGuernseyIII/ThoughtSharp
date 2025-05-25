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

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchInferenceForTraining(
  TorchBrainForTrainingMode Brain,
  TorchInferenceForTraining? Predecessor,
  float[] OriginalParameters,
  Tensor StateOutputTensor,
  Tensor ProductOutputTensor) : Inference
{
  public ReadOnlySpan<float> Result => ProductOutputTensor.squeeze(0).to(CPU).data<float>().ToArray();

  public void Incentivize(float Reward, params IReadOnlyList<Range> Ranges)
  {
    //if (Reward == 0)
    //  return;

    //using var TrainingTarget = PreviousOutput.detach().clone();

    //var Indices = new List<long>();
    //foreach (var Range in Ranges)
    //  for (var I = Range.Start.GetOffset(OutputLength); I < Range.End.GetOffset(OutputLength); ++I)
    //    Indices.Add(I);

    //if (Indices.Count == 0)
    //  return;

    //using var CachedOutput = PreviousOutput.clone();
    //var IndicesTensor = torch.tensor(Indices.ToArray(), torch.ScalarType.Int64, CachedOutput.device);
    //var CachedSlice = CachedOutput.index_select(1, IndicesTensor);
    //var OutputWithGradients = Model.forward(PreviousInput.detach().clone().requires_grad_());
    //var RerunSlice = OutputWithGradients.index_select(1, IndicesTensor);

    //var Target = RerunSlice.detach().clone(); // this is not part of the graph
    //var Similarity = (CachedSlice * Target).sum() / (CachedSlice.norm() * Target.norm() + 1e-8);

    //using var Loss = -Reward * CachedOutput.mean();
    ////using var Loss = Reward * Similarity;

    //var p = Model.parameters().First();
    //Console.WriteLine($"Before: {p.data<float>()[0]}");
    //Model.zero_grad();
    //Loss.backward();
    //foreach (var param in Model.parameters())
    //{
    //  var grad = param.grad;
    //  Console.WriteLine($"Grad norm: {(grad is null ? "null" : grad.norm().item<float>().ToString())}");
    //}
    //Optimizer.step();
    //Console.WriteLine($"After: {p.data<float>()[0]}");

    //Console.WriteLine($"Loss: {Loss.item<float>()}");
  }

  public void Train(ReadOnlySpan<float> Expected)
  {
    var TensorForBackPropagation = Replay().Product;
    var TensorWithExpectedValues = Brain.ConvertFloatsToTensor(Expected.ToArray());

    var Loss = nn.functional.mse_loss(TensorForBackPropagation, TensorWithExpectedValues);
    Brain.ApplyLoss(Loss);
  }

  TorchInferenceParts Replay()
  {
    var StateTensor = Predecessor?.Replay().State ?? Brain.EmptyState;

    return Brain.Forward(StateTensor, OriginalParameters);
  }

  public void Dispose()
  {
    StateOutputTensor.Dispose();
    ProductOutputTensor.Dispose();
  }

  public Inference MakeInference(float[] Parameters)
  {
    return Brain.ExecuteInference(this, StateOutputTensor, Parameters);
  }

  internal Tensor StateOutputTensor { get; } = StateOutputTensor;
}