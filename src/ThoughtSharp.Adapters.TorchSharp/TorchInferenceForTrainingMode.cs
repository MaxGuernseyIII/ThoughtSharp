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
using static TorchSharp.torch;

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchInferenceForTrainingMode(
  TorchBrainForTrainingMode Brain,
  TorchInferenceForTrainingMode? Predecessor,
  float[] OriginalParameters,
  TorchInferenceStateNode StateOutput,
  Tensor ProductOutputTensor) 
  : TorchInference(StateOutput, ProductOutputTensor), Inference
{
  protected TorchBrainForTrainingMode Brain { get; } = Brain;

  public void Train(params IReadOnlyList<(int, LossRule)> LossRules)
  {
    var Visitor = new TorchLossRuleVisitor(Brain);
    var TensorForBackPropagation = Replay().Payload;

    var CumulativeLoss = tensor(0.0f, requires_grad: true);

    foreach (var (At, Rule) in LossRules)
    {
      var AffectedSlice = TensorForBackPropagation.slice(1, At, At + Rule.Length, 1);
      CumulativeLoss += Rule.Accept(AffectedSlice, Visitor);
    }

    Brain.ApplyLoss(CumulativeLoss);
  }

  TorchInferenceParts Replay()
  {
    var StateTensor = Predecessor?.Replay().State ?? Brain.EmptyState;

    return Brain.Forward(OriginalParameters, StateTensor);
  }

  public Inference MakeInference(float[] Parameters)
  {
    return Brain.ExecuteInference(this, StateOutput, Parameters);
  }
}