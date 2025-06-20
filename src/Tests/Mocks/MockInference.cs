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
using FluentAssertions;
using ThoughtSharp.Runtime;

namespace Tests.Mocks;

class MockInference<TInput, TOutput>(params ImmutableArray<TOutput> ResultOutputs)
  : MockInferenceSource<TInput, TOutput>, Inference
  where TInput : CognitiveData<TInput>
  where TOutput : CognitiveData<TOutput>, new()
{
  public ImmutableArray<TOutput> ResultOutputs { get; } = ResultOutputs;
  IReadOnlyList<(int, int, LossRule)>? TrainedLossRules;

  public float[][] Result
  {
    get
    {
      var Result = new List<float[]>();
      foreach (var ResultOutput in ResultOutputs)
      {
        var Buffer = new float[TOutput.Length];
        ResultOutput.MarshalTo(Buffer);
        Result.Add(Buffer);
      }

      return Result.ToArray();
    }
  }

  public void Train(params IReadOnlyList<(int, int, LossRule)> LossRules)
  {
    TrainedLossRules = LossRules;
  }

  public void ShouldHaveBeenTrainedWith(IReadOnlyList<TOutput> Outputs)
  {
    var AllLossRules = new List<(int, int, LossRule)>();

    foreach (var (Output, I) in Outputs.Select((Output, I) => (Output, I)))
    {
      var Stream = new LossRuleStream();
      var Writer = new LossRuleWriter(Stream);
      Output.WriteAsLossRules(Writer);
      var Expected = Stream.PositionRulePairs;
      AllLossRules.AddRange(Expected.Select(O => (I, O.At, O.Rule)));
    }

    ShouldHaveBeenTrainedWith(AllLossRules);
  }

  public void ShouldHaveBeenTrainedWith(params IReadOnlyList<(int BatchNumber, int At, LossRule Rule)> Expected)
  {
    TrainedLossRules.Should().Equal(Expected);
  }

  public void ShouldNotHaveBeenTrained()
  {
    TrainedLossRules.Should().BeNull();
  }
}