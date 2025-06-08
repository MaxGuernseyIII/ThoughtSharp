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

namespace ShapeSelector.Cognition;

[CognitiveData]
public partial class ShapeLimitations
{
  [CognitiveDataBounds<byte>(0, Shape.PointCount)]
  public byte MinimumVertices { get; set; }

  [CognitiveDataBounds<byte>(0, Shape.PointCount)]
  public byte MaximumVertices { get; set; } = Shape.PointCount;

  [CognitiveDataBounds<byte>(0, 8)]
  public byte MinimumSymmetricalAxes { get; set; } = 0;

  public bool HasMaximumSymmetricalAxes { get; set; } = false;

  [CognitiveDataBounds<byte>(0, 8)]
  public byte MaximumSymmetricalAxes { get; set; } = 8;

  [CognitiveDataBounds<float>(0, 1)]
  public float MinimumRadialSymmetry { get; set; } = 0f;

  [CognitiveDataBounds<float>(0, 1)]
  public float MaximumRadialSymmetry { get; set; } = 1f;

  [CognitiveDataBounds<float>(0, MathF.PI * 2)]
  public float MinimumVertexAngle { get; set; }

  [CognitiveDataBounds<float>(0, MathF.PI * 2)]
  public float MaximumVertexAngle { get; set; } = MathF.PI;
}