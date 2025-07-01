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

using ThoughtSharp.Scenarios;

namespace Tests.SampleScenarios;

public class GradedScenarioExample
{
  [Capability]
  public class Graded
  {
    static readonly Random Source = new();
    static float Center = 0f;

    [Behavior]
    public Grade PseudoTraining()
    {
      Center += 0.001f;
      Thread.Sleep(TimeSpan.FromMilliseconds(1));
      var Actual = Center + Source.NextSingle();
      var Grade = Actual.ShouldConvergeOn().AtLeast(1f, 1f);
      return Grade;
    }

    [Behavior]
    public Transcript PseudoTrainingWithTranscript()
    {
      return new([PseudoTraining(), PseudoTraining()]);
    }

    [Behavior]
    public Task<Grade> PseudoTrainingAsync()
    {
      return Task.FromResult(PseudoTraining());
    }

    [Behavior]
    public Task<Transcript> PseudoTrainingWithTranscriptAsync()
    {
      return Task.FromResult(PseudoTrainingWithTranscript());
    }
  }

  [MaximumAttempts(50000)]
  [Curriculum]
  public class GradedCurriculum
  {
    [Phase(0)]
    [ConvergenceStandard(Fraction = .2, Of = 100)]
    [ConvergenceMetric(typeof(SoftLogic.Metrics.And2))]
    [Include(typeof(Graded))]
    public class GradedPhase0;
  }
}