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
using ThoughtSharp.Scenarios;

namespace FizzBuzz.Shaping;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Curriculum]
[MaximumAttempts(50000)]
public static class FizzBuzzTrainingPlan
{
  [Phase(1)]
  [ConvergenceStandard(Fraction = .6, Of = 200)]
  [Include(typeof(FizzbuzzScenarios.Calculations))]
  public class InitialSteps;

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  [Phase(2)]
  [MaximumAttempts(20000)]
  [ConvergenceStandard(Fraction = .98, Of = 500)]
  public class FocusedTraining
  {
    [Phase(2.1)]
    [Include(typeof(FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzbuzzScenarios.Calculations.Fizz)])]
    public class FocusOnFizz;

    [Phase(2.2)]
    [Include(typeof(FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzbuzzScenarios.Calculations.Buzz)])]
    public class FocusOnBuzz;

    [Phase(2.3)]
    [Include(typeof(FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzbuzzScenarios.Calculations.FizzBuzz)])]
    public class FocusOnFizzBuzz;

    [Phase(2.4)]
    [ConvergenceStandard(Fraction = .98, Of = 2000)]
    [Include(typeof(FizzbuzzScenarios.Calculations), Behaviors = [nameof(FizzbuzzScenarios.Calculations.WriteValue)])]
    public class FocusOnWriting;
  }

  [Phase(3)]
  [ConvergenceStandard(Fraction = 1, Of = 1000)]
  [Include(typeof(FizzbuzzScenarios.Calculations))]
  public class AllOperationsMustWork;

  [Phase(4)]
  [ConvergenceStandard(Fraction = 1, Of = 50)]
  [Include(typeof(FizzbuzzScenarios.Calculations))]
  [Include(typeof(FizzbuzzScenarios.Solution))]
  public class FinalTraining;
}