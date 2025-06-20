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

namespace ThoughtSharp.Adapters.TorchSharp;

sealed class AttentionPooling : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly torch.nn.Module<torch.Tensor, torch.Tensor> Weighting;

  public AttentionPooling(int InputFeatures, string Name = "_unnamed") : base(Name)
  {
    this.InputFeatures = InputFeatures;
    Weighting = torch.nn.Linear(InputFeatures, 1);
    RegisterComponents();
  }

  public int InputFeatures { get; }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var Mask = Input.GetMask().unsqueeze(-1);
    var Scores = Weighting.forward(Input.Payload);
    var MinusInfinity = torch.full_like(Scores, float.NegativeInfinity, dtype:torch.ScalarType.Float32);
    var MaskedScores = torch.where(Mask, Scores, MinusInfinity);
    var Weights = MaskedScores.softmax(1);
    var Weighted = Weights * Input.Payload;

    return Input with
    {
      Payload = Weighted.sum(dim: 1).unsqueeze(1),
      SequenceLengths = torch.zeros(Input.SequenceLengths.shape[0], dtype:torch.ScalarType.Int64) + 1
    };
  }
}