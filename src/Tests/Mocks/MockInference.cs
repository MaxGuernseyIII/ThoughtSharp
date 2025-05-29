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

using FluentAssertions;
using ThoughtSharp.Runtime;

namespace Tests.Mocks;

class MockInference(int InputLength, float[] Floats) : MockInferenceSource(InputLength, Floats.Length), Inference
{
  IReadOnlyList<(int, LossRule)>? TrainedLossRules;
  public readonly List<(float Reward, IReadOnlyList<Range> Ranges)> Incentives = [];

  public ReadOnlySpan<float> Result => Floats;

  public void Train(params IReadOnlyList<(int, LossRule)> LossRules)
  {
    TrainedLossRules = LossRules;
  }

  public void ShouldHaveBeenTrainedWith<T>(T Output)
    where T : CognitiveData<T>
  {
    var Stream = new LossRuleStream();
    var Writer = new LossRuleWriter(Stream, 0);
    Output.WriteAsLossRules(Writer);

    var Expected = Stream.PositionRulePairs;

    ShouldHaveBeenTrainedWith(Expected);
  }

  public void ShouldHaveBeenTrainedWith(params IReadOnlyList<(int At, LossRule Rule)> Expected)
  {
    TrainedLossRules.Should().Equal(Expected);
  }
}