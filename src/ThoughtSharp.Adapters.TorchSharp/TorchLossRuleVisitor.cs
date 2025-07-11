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

using ThoughtSharp.Runtime;
using static TorchSharp.torch;

namespace ThoughtSharp.Adapters.TorchSharp;

class TorchLossRuleVisitor(TorchBrain Brain) : LossRuleVisitor<Tensor, Tensor>
{
  public Tensor Visit(BinaryCrossEntropyWithLogitsLossRule Rule, Tensor Prediction)
  {
    var Target = Brain.ConvertBatchToFeaturesTensor(
      Batch.OfTensorData.Builder.AddSequence(S => S.AddStep(new() { Features = Rule.Target, Tokens = [] })).Build());
    return nn.functional.binary_cross_entropy_with_logits(Prediction, Target);
  }

  public Tensor Visit(MeanSquareErrorLossRule Rule, Tensor Prediction)
  {
    var Target = Brain.ConvertBatchToFeaturesTensor(
      Batch.OfTensorData.Builder.AddSequence(S => S.AddStep(new() { Features = Rule.Target, Tokens = [] })).Build());
    return nn.functional.mse_loss(Prediction, Target);
  }

  public Tensor Visit(CrossEntropyLossRule Rule, Tensor Prediction)
  {
    var Target = Brain.GetInt64ScalarTensor(Rule.Index);
    return nn.functional.cross_entropy(Prediction.squeeze(0), Target);
  }

  public Tensor Visit(HuberLossRule Rule, Tensor Prediction)
  {
    var Target = Brain.ConvertBatchToFeaturesTensor(
      Batch.OfTensorData.Builder.AddSequence(S => S.AddStep(new() { Features = Rule.Target, Tokens = [] })).Build());
    return nn.functional.huber_loss(Prediction, Target);
  }
}