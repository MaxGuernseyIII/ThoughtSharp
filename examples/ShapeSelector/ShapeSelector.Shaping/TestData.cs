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

using System.Collections.Immutable;
using ShapeSelector.Cognition;
using ShapeSelector.Cognition.ShapeHandlers;
using ThoughtSharp.Runtime;
using static ShapeSelector.Shaping.ShapeType;

namespace ShapeSelector.Shaping;

static class TestData
{
  public static readonly ShapeHandlerDescriptor LineHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Required,
      Closed = Disposition.Prohibited,
      ConcaveDepressions = Disposition.Prohibited,
      ConvexProtrusions = Disposition.Prohibited,
      FlatSurfaces = Disposition.Prohibited,
      RadialFlatSurfaces = Disposition.Prohibited,
      Corners = Disposition.Prohibited,
      RoundedSegments = Disposition.Prohibited,
      StraightSegments = Disposition.Required
    },
    Constraints =
    {
      MaximumVertices = 2,
      MinimumVertices = 2
    }
  };

  public static readonly ShapeHandler LineHandler = new LineHandler();

  public static readonly ShapeHandlerDescriptor RectangleHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Prohibited,
      Closed = Disposition.Required,
      ConcaveDepressions = Disposition.Prohibited,
      ConvexProtrusions = Disposition.Prohibited,
      FlatSurfaces = Disposition.Required,
      RadialFlatSurfaces = Disposition.Required,
      Corners = Disposition.Required,
      RoundedSegments = Disposition.Prohibited,
      StraightSegments = Disposition.Required
    },
    Constraints =
    {
      MaximumVertices = 4,
      MinimumVertices = 4,
      MinimumSymmetricalAxes = 2,
      MinimumVertexAngle = MathF.PI / 2 * .95f,
      MaximumVertexAngle = MathF.PI / 2 * 1.05f
    }
  };

  public static readonly ShapeHandler RectangleHandler = new RectangleHandler();

  public static readonly ShapeHandlerDescriptor IrregularHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Prohibited,
      Closed = Disposition.Required,
      ConcaveDepressions = Disposition.Supported,
      ConvexProtrusions = Disposition.Supported,
      FlatSurfaces = Disposition.Supported,
      RadialFlatSurfaces = Disposition.Supported,
      Corners = Disposition.Supported,
      RoundedSegments = Disposition.Prohibited,
      StraightSegments = Disposition.Supported
    }
  };

  public static readonly ShapeHandler IrregularHandler = new IrregularHandler();

  public static readonly ShapeHandlerDescriptor CircleHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Prohibited,
      Closed = Disposition.Required,
      ConcaveDepressions = Disposition.Prohibited,
      ConvexProtrusions = Disposition.Prohibited,
      FlatSurfaces = Disposition.Prohibited,
      RadialFlatSurfaces = Disposition.Prohibited,
      Corners = Disposition.Prohibited,
      RoundedSegments = Disposition.Required,
      StraightSegments = Disposition.Prohibited
    },
    Constraints =
    {
      MaximumVertices = 0,
      MinimumSymmetricalAxes = 8
    }
  };

  public static readonly ShapeHandler CircleHandler = new CircleHandler();

  public static readonly ShapeHandlerDescriptor StarHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Prohibited,
      Closed = Disposition.Required,
      ConcaveDepressions = Disposition.Required,
      ConvexProtrusions = Disposition.Required,
      FlatSurfaces = Disposition.Required,
      RadialFlatSurfaces = Disposition.Prohibited,
      Corners = Disposition.Required,
      RoundedSegments = Disposition.Prohibited,
      StraightSegments = Disposition.Required
    },
    Constraints =
    {
      MinimumSymmetricalAxes = 3
    }
  };

  public static readonly ShapeHandler StarHandler = new StarHandler();

  public static readonly ShapeHandlerDescriptor ArcHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Required,
      Closed = Disposition.Prohibited,
      ConcaveDepressions = Disposition.Prohibited,
      ConvexProtrusions = Disposition.Prohibited,
      FlatSurfaces = Disposition.Prohibited,
      RadialFlatSurfaces = Disposition.Prohibited,
      Corners = Disposition.Prohibited,
      RoundedSegments = Disposition.Required,
      StraightSegments = Disposition.Prohibited
    },
    Constraints =
    {
      MinimumSymmetricalAxes = 1,
      MaximumVertices = 2,
      MinimumVertices = 2
    }
  };

  public static readonly ShapeHandler ArcHandler = new ArcHandler();

  public static readonly ShapeHandlerDescriptor PlusHandlerDescriptor = new()
  {
    Features =
    {
      Open = Disposition.Prohibited,
      Closed = Disposition.Required,
      ConcaveDepressions = Disposition.Required,
      ConvexProtrusions = Disposition.Required,
      FlatSurfaces = Disposition.Required,
      RadialFlatSurfaces = Disposition.Required,
      Corners = Disposition.Required,
      RoundedSegments = Disposition.Prohibited,
      StraightSegments = Disposition.Required
    },
    Constraints =
    {
      MinimumSymmetricalAxes = 2,
      MinimumVertexAngle = MathF.PI / 2 * .95f,
      MaximumVertexAngle = MathF.PI / 2 * 1.05f
    }
  };

  public static readonly ShapeHandler PlusHandler = new PlusHandler();

  public static readonly ShapeHandlerCategory AllOptionsCategory = new(
    [
      GetOptionFor(Line),
      GetOptionFor(Rectangle),
      GetOptionFor(Star),
      GetOptionFor(Plus),
      GetOptionFor(Circle),
      GetOptionFor(Arc),
      GetOptionFor(Irregular)
    ]
  );

  static readonly Random Random = new();

  public static Shape SampleLine => TestShape(Geometry.MakeLine());
  public static Shape SampleRectangle => TestShape(Geometry.MakeBox());
  public static Shape SampleCircle => TestShape(Geometry.MakeCircle());
  public static Shape SampleArc => TestShape(Geometry.MakeArc());
  public static Shape SamplePlus => TestShape(Geometry.MakePlus());
  public static Shape SampleStar => TestShape(Geometry.MakeStar());
  public static Shape SampleIrregular => TestShape(Geometry.MakeIrregular());

  static Shape TestShape(List<Point> Points)
  {
    var Shape = new Shape
    {
      Points =
      [
        ..Geometry.PrepareForUseInTraining(Points)
      ]
    };
    return Shape.Normalize();
  }

  public static CognitiveOption<ShapeHandler, ShapeHandlerDescriptor> GetOptionFor(ShapeType Type)
  {
    return Type switch
    {
      Line => new(LineHandler, LineHandlerDescriptor),
      Rectangle => new(RectangleHandler, RectangleHandlerDescriptor),
      Circle => new(CircleHandler, CircleHandlerDescriptor),
      Plus => new(PlusHandler, PlusHandlerDescriptor),
      Star => new(StarHandler, StarHandlerDescriptor),
      Arc => new(ArcHandler, ArcHandlerDescriptor),
      Irregular => new(IrregularHandler, IrregularHandlerDescriptor)
    };
  }

  static List<T> RandomizeOrderOf<T>(IReadOnlyList<T> Samples)
  {
    var Copy = new List<T>(Samples);
    var Result = new List<T>();

    while (Copy.Any())
    {
      var Sample = RandomOneOf(Copy);
      Copy.Remove(Sample);
      Result.Add(Sample);
    }

    return Result;
  }

  static T RandomOneOf<T>(IReadOnlyList<T> Samples)
  {
    return Samples[Random.Next(Samples.Count)];
  }

  public static ShapeHandlerCategory GetCategory(
    params ImmutableArray<CognitiveOption<ShapeHandler, ShapeHandlerDescriptor>> Options)
  {
    return new(RandomizeOrderOf(Options));
  }
}