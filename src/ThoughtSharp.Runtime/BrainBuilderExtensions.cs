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

namespace ThoughtSharp.Runtime;

public static class BrainBuilderExtensions
{
  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice> UsingStandard<BuiltBrain, BuiltModel, TDevice>(this BrainBuilder<BuiltBrain, BuiltModel, TDevice> Builder)
  {
    return Builder.UsingSequence(S => S
      .AddLinear(Builder.InputFeatures * 20)
      .AddTanh()
      .AddLinear((Builder.InputFeatures * 20 + Builder.OutputFeatures) / 2)
      .AddTanh());
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice> ForLogic<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice> Builder, params IEnumerable<int> LayerCounts)
  {
    return Builder.UsingSequence(S => S.AddLogicLayers(LayerCounts));
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice>.SequenceConstructor AddLogicLayers<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice>.SequenceConstructor S, params IEnumerable<int> LayerCounts)
  {
    if (!LayerCounts.Any())
      LayerCounts = [4, 2];

    return LayerCounts.Aggregate(S, (Previous, Features) => Previous.AddLinear(S.Host.InputFeatures * Features).AddReLU()) ;
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice>.ParallelConstructor AddLogicPath<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice>.ParallelConstructor P,
    params IEnumerable<int> LayerCounts)
  {
    return P.AddPath(Path => Path.AddLogicLayers(LayerCounts));
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice> ForMath<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice> Builder, params IEnumerable<int> LayerCounts)
  {
    return Builder.UsingSequence(S => S.AddMathLayers(LayerCounts));
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice>.SequenceConstructor AddMathLayers<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice>.SequenceConstructor S, params IEnumerable<int> LayerCounts)
  {
    if (!LayerCounts.Any())
      LayerCounts = [4, 2];

    return LayerCounts.Aggregate(S, (Previous, Features) => Previous.AddLinear(S.Host.InputFeatures * Features).AddTanh());
  }

  public static BrainBuilder<BuiltBrain, BuiltModel, TDevice>.ParallelConstructor AddMathPath<BuiltBrain, BuiltModel, TDevice>(
    this BrainBuilder<BuiltBrain, BuiltModel, TDevice>.ParallelConstructor P,
    params IEnumerable<int> LayerCounts)
  {
    return P.AddPath(S => S.AddMathLayers(LayerCounts));
  }
}