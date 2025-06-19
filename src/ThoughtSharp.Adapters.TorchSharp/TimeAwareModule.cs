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

sealed class TimeAwareModule : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly ImmutableArray<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Submodules;
  readonly torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Pooling;

  public TimeAwareModule(
    ImmutableArray<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Submodules, 
    torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Pooling, 
    string Name = "_unnamed") : base(Name)
  {
    this.Submodules = Submodules;
    this.Pooling = Pooling;
    var I = 0;
    foreach (var Module in Submodules) 
      register_module($"Submodules{I++}", Module);

    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var WithTimeSteps = Input;

    var Transformed = Submodules.Aggregate(WithTimeSteps,
      (Previous, Module) => Module.forward(Previous));

    return Pooling.forward(Transformed);
  }
}