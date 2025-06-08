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

namespace SimpleDemo.Arithmetic.Specifications;

[Capability]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class LineCapability(AlgebraMind Mind)
{
  const float Scale = AlgebraMind.MaximumOutput - AlgebraMind.MinimumOutput;
  static readonly Random R = new();

  [Behavior]
  public void ProperlyFitsDataToLine()
  {
    var M = R.NextSingle() * .5f;
    var B = R.NextSingle() * .5f;
    var X = R.NextSingle() * .5f;

    var Result = Mind.ComputePointOnLine(M, B, X);

    Assert.That(Result).Is(
      new() {Y = M * X + B},
      C => C.ExpectApproximatelyEqual(R => R.Y, 0.01f * Scale));
  }
}