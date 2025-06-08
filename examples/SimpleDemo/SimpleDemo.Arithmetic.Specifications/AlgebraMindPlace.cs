// MIT License
// 
// Copyright (c) 2024-2024 Producore LLC
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

using JetBrains.Annotations;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;
using TorchSharp;

namespace SimpleDemo.Arithmetic.Specifications;

[MindPlace]
[UsedImplicitly]
public class AlgebraMindPlace : MindPlace<AlgebraMind, TorchBrain>
{
  static BrainBuilder<TorchBrain, torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device> Builder
  {
    get
    {
      return TorchBrainBuilder.For<AlgebraMind>()
        .UsingSequence(
          S => S
            .AddLinear(10)
            .AddSiLU()
        );
    }
  }

  public override TorchBrain MakeNewBrain()
  {
    return Builder.Build();
  }

  public override void LoadSavedBrain(TorchBrain ToLoad)
  {
    ToLoad.Load($"algebra-{Builder.CompactDescriptiveText}.pt");
  }

  public override void SaveBrain(TorchBrain ToSave)
  {
    ToSave.Save($"algebra-{Builder.CompactDescriptiveText}.pt");
  }
}