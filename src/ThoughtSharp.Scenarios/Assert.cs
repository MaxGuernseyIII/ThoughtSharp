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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios;

[PublicAPI]
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

  public static BatchCognitiveResultAssertionContext<TResultFeedback> That<TResultFeedback>(
    CognitiveResult<IReadOnlyList<TResultFeedback>, IReadOnlyList<TResultFeedback>> Subject)
  {
    return new(Subject);
  }

  public static void ShouldBe<T>(this T Actual, T Expected)
  {
    Actual.ShouldBe(Expected, "Expected {0} but found {1}");
  }

  public static void ShouldBe<T>(this T Actual, T Expected, string Format)
  {
    if (!Equals(Actual, Expected))
      throw new AssertionFailedException(
        string.Format(Format, Expected, Actual));
  }

  public static void ShouldBeApproximately<T>(this T Actual, T Expected, T Epsilon)
    where T : IFloatingPoint<T>
  {
    if (Expected < Actual - Epsilon || Expected > Actual + Epsilon)
      throw new AssertionFailedException($"Expected {Expected}±{Epsilon} but found {Actual}");
  }

  public static void ShouldBeMoreThan<T>(this T Actual, T Expected)
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

  public static void ShouldBeAtLeast<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual < Expected)
      throw new AssertionFailedException($"Expected >={Expected} but found {Actual}");
  }

  public static void ShouldBeAtMost<T>(this T Actual, T Expected)
    where T : IComparisonOperators<T, T, bool>
  {
    if (Actual > Expected)
      throw new AssertionFailedException($"Expected <={Expected} but found {Actual}");
  }

  /// <summary>
  ///   Terminates training or test execution.
  /// </summary>
  /// <param name="Message">The message to be given as the reason for termination</param>
  /// <returns>An exception you can use in a throw clause for clean code-termination analysis</returns>
  /// <exception cref="FatalErrorException">This is always thrown</exception>
  [DoesNotReturn]
  public static FatalErrorException Fatal(string Message)
  {
    throw new FatalErrorException(Message);
  }

  /// <summary>
  ///   Terminates the training or test execution if a condition is not met.
  /// </summary>
  /// <param name="ActualCondition">The condition that must be met</param>
  /// <param name="ConditionDescription">A description of the condition that must be met</param>
  /// <exception cref="FatalErrorException">Thrown when condition is not met</exception>
  public static void Critical([DoesNotReturnIf(false)] bool ActualCondition, string ConditionDescription)
  {
    if (!ActualCondition)
      throw Fatal($"Critical condition not met: {ConditionDescription}");
  }

  public static ConvergenceAssertions<T> ShouldConvergeOn<T>(this T This)
    where T : IFloatingPoint<T>
  {
    return new(This);
  }
}