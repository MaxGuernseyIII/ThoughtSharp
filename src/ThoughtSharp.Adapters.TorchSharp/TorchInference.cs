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

using System;
using ThoughtSharp.Runtime;
using TorchSharp;

namespace ThoughtSharp.Adapters.TorchSharp;

public class TorchInference(
  TorchBrain Brain,
  TorchInference? Predecessor,
  float[][] OriginalBatches,
  TorchInferenceStateNode StateOutput,
  torch.Tensor ProductOutputTensor) : Inference
{
  internal TorchInferenceStateNode StateOutput { get; } = StateOutput;
  internal torch.Tensor ProductOutputTensor { get; } = ProductOutputTensor;

  public ReadOnlySpan<float> Result => ProductOutputTensor[ProductOutputTensor.shape[0] - 1].to(torch.CPU).data<float>().ToArray();

  protected TorchBrain Brain { get; } = Brain;

  static float Sigmoid(float X) =>
    1f / (1f + MathF.Exp(-X));

  public void Dispose()
  {
    StateOutput.Dispose();
    ProductOutputTensor.Dispose();
  }

  public void Train(params IReadOnlyList<(int, LossRule)> LossRules)
  {
    var Visitor = new TorchLossRuleVisitor(Brain);
    var TensorForBackPropagation = Replay().Payload;

    var CumulativeLoss = torch.tensor(0.0f, requires_grad: true);

    foreach (var (At, Rule) in LossRules)
    {
      var AffectedSlice = TensorForBackPropagation.slice(1, At, At + Rule.Length, 1);
      var SliceLoss = Rule.Accept(AffectedSlice, Visitor);
      //Console.WriteLine($"Target: {SliceLoss.device}");

      CumulativeLoss += SliceLoss;
    }

    //Console.WriteLine($"Input: {TensorForBackPropagation.device}");
    //Console.WriteLine($"Loss: {CumulativeLoss.device}");

    Brain.ApplyLoss(CumulativeLoss);
  }

  TorchInferenceParts Replay()
  {
    var StateTensor = Predecessor?.Replay().State ?? Brain.EmptyState;

    return Brain.Forward(OriginalBatches, StateTensor);
  }

  public Inference MakeInference(float[][] Batches)
  {
    return Brain.ExecuteInference(this, StateOutput, Batches);
  }
}