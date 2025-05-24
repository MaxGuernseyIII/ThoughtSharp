// MIT License
// 
// Copyright (c) 2024-2024 Hexagon Software LLC
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

namespace ThoughtSharp.Runtime;

partial class Thought
{
  class ThoughtGraph
  {
    ThoughtGraph(ImmutableArray<Thought> Unrewarded, ImmutableArray<RewardNode> RewardedWithoutMind,
      ImmutableDictionary<Mind, ImmutableArray<RewardNode>> RewardedWithMind)
    {
      this.Unrewarded = Unrewarded;
      this.RewardedWithoutMind = RewardedWithoutMind;
      this.RewardedWithMind = RewardedWithMind;
    }

    public ImmutableArray<Thought> Unrewarded { get; }
    public ImmutableArray<RewardNode> RewardedWithoutMind { get; }
    public ImmutableDictionary<Mind, ImmutableArray<RewardNode>> RewardedWithMind { get; }

    public static ThoughtGraph For(Thought CompleteThought)
    {
      var Unrewarded = new List<Thought>();
      var RewardedWithoutMind = new List<RewardNode>();
      var RewardedWithMind = new Dictionary<Mind, List<RewardNode>>();

      BuildFor(CompleteThought, 1f, Unrewarded, RewardedWithoutMind, RewardedWithMind);

      return new(
        [..Unrewarded],
        [..RewardedWithoutMind],
        RewardedWithMind.ToImmutableDictionary(Pair => Pair.Key, Pair => Pair.Value.ToImmutableArray()));
    }

    static void BuildFor(Thought CurrentThought, float Weight, List<Thought> Unrewarded,
      List<RewardNode> RewardedWithoutMind,
      Dictionary<Mind, List<RewardNode>> RewardedWithMind)
    {
      foreach (var ChildThought in CurrentThought.Children)
        BuildFor(ChildThought, CurrentThought.Weights[ChildThought], Unrewarded, RewardedWithoutMind, RewardedWithMind);

      var TrainingPolicy = CurrentThought.TrainingPolicy;

      switch (TrainingPolicy)
      {
        case null:
          Unrewarded.Add(CurrentThought);
          break;
        case {Mind: null}:
          RewardedWithoutMind.Add(new(CurrentThought, TrainingPolicy, Weight));
          break;
        case {Mind: { } Mind}:
          if (!RewardedWithMind.TryGetValue(Mind, out var PerMindThoughts))
          {
            PerMindThoughts = new();
            RewardedWithMind.Add(Mind, PerMindThoughts);
          }

          PerMindThoughts.Add(new(CurrentThought, TrainingPolicy, Weight));
          break;
      }
    }

    public readonly struct RewardNode(Thought Thought, TrainingPolicy TrainingPolicy, float Weight)
    {
      public Thought Thought { get; } = Thought;

      public void IncentivizeOutput(float Reward)
      {
        TrainingPolicy.IncentivizeOutput(Reward * Weight);
      }

      public void IncentivizeOutputAndState(float Reward)
      {
        TrainingPolicy.IncentivizeOutputAndState(Reward * Weight);
      }
    }
  }
}