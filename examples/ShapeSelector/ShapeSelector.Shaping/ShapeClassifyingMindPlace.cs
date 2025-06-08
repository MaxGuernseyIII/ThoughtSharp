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
using ShapeSelector.Cognition;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Scenarios;
using TorchSharp;

namespace ShapeSelector.Shaping;

[UsedImplicitly]
[MindPlace]
public class ShapeClassifyingMindPlace : MindPlace<ShapeClassifyingMind, TorchBrain>
{
  static BrainBuilder<TorchBrain, torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device> BrainBuilder
  {
    get;
  }
    = TorchBrainBuilder.For<ShapeClassifyingMind>()
      .UsingParallel(P => P
        .AddLogicPath(4, 2)
        .AddMathPath(4, 2));

  static string FilePath => $"shape-classifying-mind-{BrainBuilder.CompactDescriptiveText}.pt";

  public override TorchBrain MakeNewBrain()
  {
    return BrainBuilder.Build();
  }

  public override void LoadSavedBrain(TorchBrain ToLoad)
  {
    ToLoad.Load(FilePath);
  }

  public override void SaveBrain(TorchBrain ToSave)
  {
    ToSave.Save(FilePath);
  }
}