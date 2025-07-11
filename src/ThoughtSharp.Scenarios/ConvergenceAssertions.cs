﻿// MIT License
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

namespace ThoughtSharp.Scenarios;

public static class ConvergenceAssertions
{
  public const float TotalSuccess = 1f;

  public const float TotalFailure = 0f;
}

public readonly ref struct ConvergenceAssertions<T>(T Subject)
  where T : IFloatingPoint<T>
{
  public Grade Approximately(T Target, T TotalSuccessRadius, T TotalFailureRadius)
  {
    var DifferenceMagnitude = T.Abs(Target - Subject);

    return new()
    {
        Score = ComputeSuccessFraction(DifferenceMagnitude, TotalFailureRadius, TotalSuccessRadius),
        Annotations =
          [$"Expected {Target}±{TotalSuccessRadius} (total failure at ±{TotalFailureRadius}) and got {Subject}"]
      };
  }

  public Grade AtLeast(T Target, T TotalFailureRadius)
  {
    return new()
    {
        Score = ComputeSuccessFraction(Subject, Target - TotalFailureRadius, Target),
        Annotations = [$"Expected >= {Target} (total failure at <= {Target - TotalFailureRadius}) and found {Subject}"]
      };
  }

  public Grade AtMost(T Target, T TotalFailureRadius)
  {
    return
      new()
      {
        Score = ComputeSuccessFraction(Subject, Target + TotalFailureRadius, Target),
        Annotations = [$"Expected <= {Target} (total failure at >= {Target + TotalFailureRadius}) and found {Subject}"]
      };
  }

  static float ComputeSuccessFraction(T Actual, T TotalFailure, T TotalSuccess)
  {
    var Offset = Actual - TotalFailure;
    var Range = TotalSuccess - TotalFailure;
    var TransformedOffset = Offset / Range;
    var Success = T.Clamp(TransformedOffset, T.Zero, T.One);

    return float.CreateChecked(Success);
  }
}