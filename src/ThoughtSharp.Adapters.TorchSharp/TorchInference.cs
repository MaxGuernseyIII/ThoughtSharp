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

namespace ThoughtSharp.Adapters.TorchSharp;

public class TorchInference(
  TorchBrain Brain,
  TorchInference? Predecessor,
  TorchInferenceParts OriginalInput,
  TorchInferenceParts Output) : Inference
{
  internal TorchInferenceParts Output { get; } = Output;
  internal TorchInferenceParts OriginalInput { get; } = OriginalInput;

  public float[][] Result
  {
    get
    {
      var OutputTensor = Output.Payload;
      var LastIndices = (Output.SequenceLengths - 1).unsqueeze(1).unsqueeze(2).to(OutputTensor.device);
      var BatchSize = LastIndices.shape[0];

      var ExpandedIndices = LastIndices.expand([BatchSize, 1, OutputTensor.shape[2]]);

      var FinalItems = OutputTensor.gather(1, ExpandedIndices);

      return FinalItems
        .to(torch.CPU)
        .data<float>()
        .ToArray()
        .Chunk((int)OutputTensor.shape[2])
        .Select(Chunk => Chunk.ToArray())
        .ToArray();
    }
  }

  protected TorchBrain Brain { get; } = Brain;

  public void Dispose()
  {
    OriginalInput.State?.Dispose();
    Output.Payload.Dispose();
  }

  public void Train(params IReadOnlyList<(int, int, LossRule)> LossRules)
  {
    var Visitor = new TorchLossRuleVisitor(Brain);
    var TensorForBackPropagation = Replay().Payload;

    var CumulativeLoss = torch.tensor(0.0f, requires_grad: true);

    foreach (var (BatchNumber, At, Rule) in LossRules)
    {
      var AffectedSlice = TensorForBackPropagation
        .slice(0, BatchNumber, BatchNumber + 1, 1)
        .slice(2, At, At + Rule.Length, 1);
      var SliceLoss = Rule.Accept(AffectedSlice, Visitor);

      CumulativeLoss += SliceLoss;
    }

    Brain.ApplyLoss(CumulativeLoss);
  }

  TorchInferenceParts Replay()
  {
    var StateTensor = Predecessor?.Replay().State ?? Brain.EmptyState;

    using var Mode = Brain.EnterTrainingMode(true);

    return Brain.Forward(OriginalInput with
    {
      State = StateTensor
    });
  }

  public Inference MakeInference(Batch<float[]> Features, Batch<long[]> Tokens)
  {
    return Brain.ExecuteInference(this, 
      Brain.ConvertFloatsToInput(Features) with
      {
        State = Output.State
      });
  }
}