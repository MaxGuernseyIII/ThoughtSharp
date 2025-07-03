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

sealed class MultiHeadedAttentionAdapter : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly torch.nn.Module<torch.Tensor, torch.Tensor> Adapter;
  readonly MultiheadAttention Attention;

  public MultiHeadedAttentionAdapter(int InputFeatures, int Heads, int FeaturesPerHead, string Name = "_unnamed") :
    base(Name)
  {
    var HiddenDimensionSize = Heads * FeaturesPerHead;
    Adapter = torch.nn.Linear(InputFeatures, HiddenDimensionSize);
    Attention = torch.nn.MultiheadAttention(HiddenDimensionSize, Heads);

    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var Adapted = Adapter.forward(Input.Features);

    var TimeFirst = Adapted.transpose(0, 1);

    var (Attended, _) = Attention.forward(TimeFirst, TimeFirst, TimeFirst, null, false, null);

    var BatchFirst = Attended.transpose(0, 1);

    return Input with {Features = BatchFirst};
  }
}