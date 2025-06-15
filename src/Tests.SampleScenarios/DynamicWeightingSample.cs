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

public class DynamicWeightingSample
{
  [MaximumAttempts(1)]
  [ConvergenceStandard(Fraction = 1, Of = 1)]
  [Curriculum]
  public class Curriculum
  {
    [Phase(1)]
    public class PhaseWithImplicitDynamicWeights;

    [Phase(1)]
    [DynamicWeighting(Minimum = MinimumWeight)]
    public class PhaseWithExplicitMinimumWeight
    {
      public const float MinimumWeight = .394f;
    }

    [Phase(2)]
    [DynamicWeighting(Maximum = MaximumWeight)]
    public class PhaseWithExplicitMaximumWeight
    {
      public const float MaximumWeight = 0.606f;
    }

    [Phase(3)]
    [DynamicWeighting(
      Minimum = HeritableMinimumWeight,
      Maximum = HeritableMaximumWeight)]
    public class PhaseWithHeritableWeights
    {
      public const float HeritableMinimumWeight = 0.109f;
      public const float HeritableMaximumWeight = 0.901f;

      [Phase(1)]
      public class PhaseWithInheritedWeights;

      [Phase(2)]
      [DynamicWeighting(
        Minimum = OverriddenMinimumWeight)]
      public class PhaseWithOverriddenMinimumWeight
      {
        public const float OverriddenMinimumWeight = .213f;
      }

      [Phase(3)]
      [DynamicWeighting(
        Maximum = OverriddenMaximumWeight)]
      public class PhaseWithOverriddenMaximumWeight
      {
        public const float OverriddenMaximumWeight = .930f;
      }
    }

    [Phase(4)]
    [DynamicWeighting(Minimum = 0, Maximum = 0)]
    public class PhaseWithLowMinimumAndMaximumWeights;
  }
}