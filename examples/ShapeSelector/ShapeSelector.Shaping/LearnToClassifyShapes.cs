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

namespace ShapeSelector.Shaping;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Curriculum]
public class LearnToClassifyShapes
{
  [MaximumAttempts(20000)]
  [ConvergenceStandard(Fraction = .90, Of = 100)]
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  [Phase(1)]
  public class LearnBasicDistinctions
  {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.01)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Line_Rectangle)
      ])]
    public class IdentifyLine;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.02)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Line_Rectangle),
        nameof(Distinctions.Rectangle_Line)
      ])]
    public class DistinguishBetweenRectangleAndLine;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.03)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Circle_Rectangle),
        nameof(Distinctions.Rectangle_Circle)
      ])]
    public class DistinguishBetweenCircleAndRectangle;

    [MaximumAttempts(100000)]
    [ConvergenceStandard(Fraction = .99, Of = 500)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.031)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Circle_Rectangle),
        nameof(Distinctions.Rectangle_Circle)
      ])]
    public class HyperTrainCircleAndRectangle;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.04)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Star_Plus),
        nameof(Distinctions.Plus_Star)
      ])]
    public class DistinguishBetweenStarAndPlus;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.05)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Arc_Circle),
        nameof(Distinctions.Circle_Arc)
      ])]
    public class DistinguishBetweenArcAndCircle;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.06)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Irregular_Circle),
        nameof(Distinctions.Circle_Irregular)
      ])]
    public class DistinguishBetweenIrregularAndCircle;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Phase(1.07)]
    [Include(
      typeof(Distinctions),
      Behaviors =
      [
        nameof(Distinctions.Irregular_Plus),
        nameof(Distinctions.Plus_Irregular)
      ])]
    public class DistinguishBetweenIrregularAndPlus;
  }

  [MaximumAttempts(20000)]
  [ConvergenceStandard(Fraction = .95, Of = 500)]
  [Phase(2)]
  [Include(typeof(Distinctions))]
  public class AllDistinctionsToConfidence;
}