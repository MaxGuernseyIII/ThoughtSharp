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

using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;

namespace Tests;

[TestClass]
public class Training
{
  [TestMethod]
  public void CapturedThoughtTraining()
  {
    float TotalRewardApplied = 0;
    var T = Thought.Capture(new MockProduct(), R => TotalRewardApplied += R);
    var RewardToApply = Any.Float;

    T.ApplyReward(RewardToApply);

    TotalRewardApplied.Should().Be(RewardToApply);
  }

  [TestMethod]
  public void DistributiveApplication()
  {
    var RewardToApply = Any.Float;

    var TotalRewardApplied = MeasureDistributedRewardAmount(RewardToApply);

    TotalRewardApplied.Should().Be(RewardToApply);
  }

  [TestMethod]
  public void WeightedDistribution()
  {
    var RewardToApply = Any.Float;
    var UnweightedRewardAmount = MeasureDistributedRewardAmount(RewardToApply);

    float Weight = Any.Float;
    float TotalRewardApplied = 0;
    Thought.Do(R =>
    {
      var Subthought = Thought.Capture(new MockProduct(), Reward => TotalRewardApplied += Reward);
      R.Consume(Subthought);
      R.SetWeight(Subthought, Weight);
    }).ApplyReward(RewardToApply);

    TotalRewardApplied.Should().BeApproximately(UnweightedRewardAmount * Weight, 0.0001f);
  }

  static float MeasureDistributedRewardAmount(float RewardToApply)
  {
    float TotalRewardApplied = 0;
    Thought.Do(R =>
    {
      R.Consume(Thought.Capture(new MockProduct(), Reward => TotalRewardApplied += Reward));
    }).ApplyReward(RewardToApply);
    return TotalRewardApplied;
  }
}