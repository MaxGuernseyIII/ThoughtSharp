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

using ThoughtSharp.Runtime;

namespace SimpleDemo.Arithmetic;

[Mind]
public partial class AlgebraMind
{
  // assumed to always be <= 0 in other calcs
  public const float MinimumInput = -1000f;

  // assumed to always be >= 0 in other calcs
  public const float MaximumInput = 1000f;


  public const float MinimumOutput = MinimumInput * MaximumInput + MinimumInput;
  public const float MaximumOutput = MaximumInput * MaximumInput + MaximumInput;

  [Make]
  public partial CognitiveResult<SingleVariableComputation, SingleVariableComputation> ComputePointOnLine(
    [CognitiveDataBounds<float>(MinimumInput, MaximumInput)]
    float M, 
    [CognitiveDataBounds<float>(MinimumInput, MaximumInput)]
    float B,
    [CognitiveDataBounds<float>(MinimumInput, MaximumInput)]
    float X);
}