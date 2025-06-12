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

using System.Numerics;
using JetBrains.Annotations;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers, Reason = "Public API")]
public static class Assert
{
  public static AccumulatedUseAssertionContext<TSurface> That<TSurface>(AccumulatedUseFeedback<TSurface> Subject)
  {
    return new(Subject);
  }

  public static CognitiveResultAssertionContext<TResultFeedback> That<TResultFeedback>(
    CognitiveResult<TResultFeedback, TResultFeedback> Subject)
  {
    return new(Subject);
  }

  public static void ShouldBe<T>(this T Actual, T Expected)
  {
    if (!Equals(Actual, Expected))
      throw new AssertionFailedException($"Expected {Expected} but found {Actual}");
  }

  public static void ShouldBeApproximately<T>(this T Actual, T Expected, T Epsilon)
    where T : IFloatingPoint<T>
  {
    if (Expected < Actual - Epsilon || Expected > Actual + Epsilon)
      throw new AssertionFailedException($"Expected {Expected}±{Epsilon} but found {Actual}");
  }

  public static void ShouldBeGreaterThan<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual <= Expected)
      throw new AssertionFailedException($"Expected >{Expected} but found {Actual}");
  }

  public static void ShouldBeLessThan<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual >= Expected)
      throw new AssertionFailedException($"Expected <{Expected} but found {Actual}");
  }

  public static void ShouldBeGreaterThanOrEqualTo<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual < Expected)
      throw new AssertionFailedException($"Expected >{Expected} but found {Actual}");
  }

  public static void ShouldBeLessThanOrEqualTo<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual > Expected)
      throw new AssertionFailedException($"Expected <{Expected} but found {Actual}");
  }
}
