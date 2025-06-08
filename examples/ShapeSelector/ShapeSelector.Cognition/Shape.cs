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
public partial class Shape
{
  public const byte PointCount = 50;
  [CognitiveDataCount(PointCount)] public Point[] Points { get; set; } = new Point[PointCount];

  public Shape Normalize()
  {
    var HotPoints = Points.Where(P => P.IsHot).ToArray();
    var ColdPoints = Points.Where(P => !P.IsHot).ToArray();

    if (!HotPoints.Any())
      return this;

    var MinX = HotPoints.Select(P => P.X).Min();
    var MinY = HotPoints.Select(P => P.Y).Min();
    var MaxX = HotPoints.Select(P => P.X).Max();
    var MaxY = HotPoints.Select(P => P.Y).Max();
    var ScaleX = MaxX - MinX;
    var ScaleY = MaxY - MinY;
    var Scale = 2 / MathF.Max(ScaleX, ScaleY);
    var CenterX = (MinX + MaxX) / 2;
    var CenterY = (MinY + MaxY) / 2;

    if (Scale <= float.Epsilon)
      return this;

    return new()
    {
      Points =
      [
        ..HotPoints.Select(P => P with
        {
          X = (P.X - CenterX) * Scale,
          Y = (P.Y - CenterY) * Scale
        }),
        ..ColdPoints.Select(_ => new Point())
      ]
    };
  }
}