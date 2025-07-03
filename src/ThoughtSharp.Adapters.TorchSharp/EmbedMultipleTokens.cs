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

using System.Collections.Immutable;
using TorchSharp;

namespace ThoughtSharp.Adapters.TorchSharp;

public sealed class EmbedMultipleTokens : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly ImmutableArray<torch.nn.Module<torch.Tensor, torch.Tensor>> Modules;

  public EmbedMultipleTokens(ImmutableArray<(long ClassCount, int Dimensions)> Configuration, string Name = "_unnamed") : base(Name)
  {
    var Modules = new List<torch.nn.Module<torch.Tensor, torch.Tensor>>();

    foreach (var ((Count, Dimensions), Index) in Configuration.Select((Config, Index) => (Config, Index)))
    {
      var ThisModule = torch.nn.Embedding(Count, Dimensions);
      Modules.Add(ThisModule);
      register_module($"embedding_{Index}", ThisModule);
    }

    this.Modules = [..Modules];
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var Tokens = Input.Tokens;
    var ActualTokenCount = Tokens.shape[2];
    var ExpectedTokenCount = Modules.Length;
    if (ActualTokenCount != ExpectedTokenCount)
      throw new InvalidOperationException($"Expected last dimension of tokens tensor to be {ExpectedTokenCount}, but found {ActualTokenCount}");

    var Results = new torch.Tensor[ExpectedTokenCount];

    foreach (var TokenIndex in Enumerable.Range(0, ExpectedTokenCount))
    {
      var Slice = Tokens.select(2, TokenIndex).contiguous();
      var Embedding = Modules[TokenIndex];
      Results[TokenIndex] = Embedding.forward(Slice);
    }

    return Input with
    {
      Features = torch.concat([Input.Features, ..Results], 2),
      Tokens = torch.empty([Tokens.shape[0], Tokens.shape[1], 0], dtype:torch.ScalarType.Int64, device: Tokens.device)
    };
  }
}