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
    var MockTrainingPolicy = new MockTrainingPolicy();
    var T = Thought.Capture(new MockProduct(), MockTrainingPolicy);
    var RewardToApply = Any.Float;

    T.ApplyIncentive(RewardToApply);

    MockTrainingPolicy.OutputReward.Should().Be(RewardToApply);
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

    var Policy = new MockTrainingPolicy();
    float Weight = Any.Float;
    Thought.Do(R =>
    {
      var Subthought = Thought.Capture(new MockProduct(), Policy);
      R.Consume(Subthought);
      R.SetWeight(Subthought, Weight);
    }).ApplyIncentive(RewardToApply);

    Policy.OutputReward.Should().BeApproximately(UnweightedRewardAmount * Weight, 0.0001f);
  }

  static float MeasureDistributedRewardAmount(float RewardToApply)
  {
    var Policy = new MockTrainingPolicy();
    Thought.Do(R =>
    {
      R.Consume(Thought.Capture(new MockProduct(), Policy));
    }).ApplyIncentive(RewardToApply);
    return Policy.OutputReward;
  }
}