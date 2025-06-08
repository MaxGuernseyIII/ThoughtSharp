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

using System.Numerics;
using ShapeSelector.Cognition;

namespace ShapeSelector.Shaping;

static class Geometry
{
  static readonly Random Random = new();

  static float RandomScale => Random.NextSingle() * 100f;
  static Vector2 RandomVector => new(RandomScale, RandomScale);

  static Matrix3x2 RandomTransform => CreateTransform(RandomVector, RandomRadians, RandomScale, RandomVector);

  static float RandomRadians => Random.NextSingle() * MathF.PI * 2;

  static Matrix3x2 CreateTransform(Vector2 Center, float RotationRadians, float Scale, Vector2 Translation)
  {
    return
      Matrix3x2.CreateTranslation(-Center) *
      Matrix3x2.CreateRotation(RotationRadians) *
      Matrix3x2.CreateScale(Scale) *
      Matrix3x2.CreateTranslation(Translation);
  }

  static IEnumerable<Point> AddNoise(IEnumerable<Point> Points, float Radius)
  {
    return Points.Select(P => AddNoise(P, Radius));
  }

  static IEnumerable<Point> Transform(IEnumerable<Point> Points, Matrix3x2 Transform)
  {
    return Points.Select(P => TransformPoint(P, Transform));
  }

  static Point AddNoise(Point Point, float Radius)
  {
    var Direction = RandomRadians;
    var Length = Random.NextSingle() * Radius;

    return Point with
    {
      X = Point.X + MathF.Sin(Direction) * Length,
      Y = Point.Y + MathF.Cos(Direction) * Length
    };
  }

  static Point TransformPoint(Point Point, Matrix3x2 Transform)
  {
    var Vector = new Vector2(Point.X, Point.Y);
    var NewVector = Vector2.Transform(Vector, Transform);

    return Point with { X = NewVector.X, Y = NewVector.Y };
  }

  public static IEnumerable<Point> PrepareForUseInTraining(IEnumerable<Point> Points)
  {
    return NormalizePointsArraySize(Transform(AddNoise(Points, 0.01f), RandomTransform));
  }

  static IEnumerable<Point> NormalizePointsArraySize(IEnumerable<Point> Noisy2)
  {
    var PossiblyNoisy = Noisy2.Take(Shape.PointCount);
    List<Point> Result = [.. PossiblyNoisy];
    Result.AddRange(Enumerable.Repeat(new Point(), Shape.PointCount - Result.Count));
    return Result;
  }

  public static List<Point> MakeLine()
  {
    return MakeLine(new(0, 0), new(0, 1), Random.Next(20, Shape.PointCount + 1), false);
  }

  public static List<Point> MakeLine(Vector2 From, Vector2 To, int Samples, bool SkipFrom)
  {
    var Result = new List<Point>();
    var SegmentLength = (To - From) / (Samples - 1);

    for (var I = SkipFrom ? 1 : 0; I < Samples; ++I)
    {
      var Vector = From + SegmentLength * I;

      Result.Add(new() { IsHot = true, X = Vector.X, Y = Vector.Y });
    }

    return Result;
  }

  public static List<Point> MakeIrregular()
  {
    const int MinPoints = 3;
    const int MaxPoints = 9;
    const float NoiseStrength = 0.5f;
    const float MinimumNoise = 0.1f;
    var VerticesCount = Random.Next(MinPoints, MaxPoints + 1);

    var BaseRadius = 0.5f;

    var Center = new Vector2(0, 0);
    var Points = new List<Point>();

    var VertexBaseStep = MathF.PI * 2 / VerticesCount;

    Vector2 GetVertex(int Index)
    {
      var Angle = Index * VertexBaseStep + VertexBaseStep * (.9f + Random.NextSingle() * .2f);
      var Direction = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle));
      var RawNoise = Random.NextSingle() - 0.5f;
      var Noise = RawNoise * NoiseStrength + MathF.Sign(RawNoise) * MinimumNoise;
      var Radius = BaseRadius + Noise;
      return Center + Direction * Radius;
    }

    var FirstVertex = GetVertex(0);
    var LastVertex = FirstVertex;

    for (var I = 1; I < VerticesCount; I++)
    {
      var ThisVertex = GetVertex(I);

      Points.AddRange(MakeLine(LastVertex, ThisVertex, Random.Next(3, 7), true));
      LastVertex = ThisVertex;
    }

    Points.AddRange(MakeLine(LastVertex, FirstVertex, Random.Next(3, 7), true));

    return Points;
  }

  public static List<Point> MakeBox()
  {
    var AspectRatio = .5f + Random.NextSingle();
    var LinePartCount = 4 + Random.Next(7);
    var Result = new List<Point>();
    Result.AddRange(MakeLine(new(0, 0), new(AspectRatio, 0), LinePartCount, true));
    Result.AddRange(MakeLine(new(AspectRatio, 0), new(AspectRatio, 1), LinePartCount, true));
    Result.AddRange(MakeLine(new(AspectRatio, 1), new(0, 1), LinePartCount, true));
    Result.AddRange(MakeLine(new(0, 1), new(0, 0), LinePartCount, true));

    return Result;
  }

  public static List<Point> MakeStar()
  {
    var Count = 3 + Random.Next(5);
    var SpokeRadianStep = MathF.PI * 2 / Count;
    var SpokeRadianHalfStep = SpokeRadianStep / 2;
    var SpokeLength = 1.1f + Random.NextSingle();
    var PitLength = .5f + Random.NextSingle() * .25f;
    var Result = new List<Point>();
    var SegmentSamples = 4;

    foreach (var I in Enumerable.Range(0, Count))
    {
      var ThisSpokeVertex = GetVertex(I * SpokeRadianStep) * SpokeLength;
      var ThisPitVertex = GetVertex(I * SpokeRadianStep + SpokeRadianHalfStep) * PitLength;
      var NextSpokeVertex = GetVertex((I + 1) * SpokeRadianStep) * SpokeLength;

      Result.AddRange(MakeLine(ThisSpokeVertex, ThisPitVertex, SegmentSamples, true));
      Result.AddRange(MakeLine(ThisPitVertex, NextSpokeVertex, SegmentSamples, true));
    }

    return Result;

    Vector2 GetVertex(float Radians)
    {
      return new(MathF.Sin(Radians), MathF.Cos(Radians));
    }
  }

  public static List<Point> MakePlus()
  {
    var SegmentsPerLine = 3 + Random.Next(2);
    var AspectRatio = .5f + Random.NextSingle();
    var XCutFactor = .1f + Random.NextSingle() * .2f;
    var YCutFactor = .1f + Random.NextSingle() * .2f;
    var XCut = AspectRatio * XCutFactor;
    var YCut = YCutFactor;
    var Result = new List<Point>();
    Result.AddRange(MakeLine(new(XCut, YCut), new(XCut, 0), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 0), new(AspectRatio - XCut, 0), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 0), new(AspectRatio - XCut, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, YCut), new(AspectRatio, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio, YCut), new(AspectRatio, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio, 1 - YCut), new(AspectRatio - XCut, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 1 - YCut), new(AspectRatio - XCut, 1), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 1), new(XCut, 1), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 1), new(XCut, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 1 - YCut), new(0, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(0, 1 - YCut), new(0, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(0, YCut), new(XCut, YCut), SegmentsPerLine, true));
    return Result;
  }

  public static List<Point> MakeArc()
  {
    var MinLength = .25f;
    var Length = MinLength + RandomRadians * (.5f - MinLength);
    return MakeArc(0, Length, Random.Next(20, 50), false);
  }

  public static List<Point> MakeCircle()
  {
    return MakeArc(0f, MathF.PI * 2, Random.Next(Shape.PointCount / 2, Shape.PointCount), true);
  }

  static List<Point> MakeArc(float StartAngleRadians, float EndAngleRadians, int SampleCount, bool SkipFirst)
  {
    var Center = new Vector2(0, 0);
    var Result = new List<Point>();
    var Step = (EndAngleRadians - StartAngleRadians) / (SampleCount - 1);

    for (var I = SkipFirst ? 1 : 0; I < SampleCount; I++)
    {
      var Angle = StartAngleRadians + I * Step;
      var X = Center.X + MathF.Cos(Angle);
      var Y = Center.Y + MathF.Sin(Angle);

      Result.Add(new() { IsHot = true, X = X, Y = Y });
    }

    return Result;
  }

  public static IEnumerable<Point> GetSimplified(IEnumerable<Point> MakeShapePoints)
  {
    return NormalizePointsArraySize(MakeShapePoints);
  }
}