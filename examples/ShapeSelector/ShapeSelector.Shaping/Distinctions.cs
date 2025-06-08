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
using ShapeSelector.Cognition;
using ThoughtSharp.Scenarios;
using static ShapeSelector.Shaping.ShapeType;
using static ShapeSelector.Shaping.TestData;

namespace ShapeSelector.Shaping;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Capability]
public class Distinctions(ShapeClassifyingMind Mind)
{
  [Behavior]
  public void Line_Rectangle()
  {
    var Category = GetCategory(Line, Rectangle);

    var Result = Mind.ChooseHandlerFor(SampleLine, Category);

    Assert.That(Result).Is(LineHandler);
  }

  [Behavior]
  public void Rectangle_Line()
  {
    var Category = GetCategory(Rectangle, Line);

    var Result = Mind.ChooseHandlerFor(SampleRectangle, Category);

    Assert.That(Result).Is(RectangleHandler);
  }

  [Behavior]
  public void Circle_Rectangle()
  {
    var Category = GetCategory(Circle, Rectangle);
    var Shape = SampleCircle;

    var Result = Mind.ChooseHandlerFor(Shape, Category);

    try
    {
      Assert.That(Result).Is(CircleHandler);
    }
    catch
    {
      var Svg = Geometry.ShapeToSvg(Shape);
      File.WriteAllText("circle.svg", Svg);
      Console.WriteLine("circle.svg saved");
      throw;
    }
  }

  [Behavior]
  public void Rectangle_Circle()
  {
    var Category = GetCategory(Rectangle, Circle);
    var Shape = SampleRectangle;

    var Result = Mind.ChooseHandlerFor(Shape, Category);

    try
    {
      Assert.That(Result).Is(RectangleHandler);
    }
    catch
    {
      var Svg = Geometry.ShapeToSvg(Shape);
      File.WriteAllText("rectangle.svg", Svg);
      Console.WriteLine("rectangle.svg saved");
      throw;
    }
  }

  [Behavior]
  public void Circle_Arc()
  {
    var Category = GetCategory(Circle, Arc);

    var Result = Mind.ChooseHandlerFor(SampleCircle, Category);

    Assert.That(Result).Is(CircleHandler);
  }

  [Behavior]
  public void Arc_Circle()
  {
    var Category = GetCategory(Arc, Circle);

    var Result = Mind.ChooseHandlerFor(SampleArc, Category);

    Assert.That(Result).Is(ArcHandler);
  }

  [Behavior]
  public void Star_Plus()
  {
    var Category = GetCategory(Star, Plus);

    var Result = Mind.ChooseHandlerFor(SampleStar, Category);

    Assert.That(Result).Is(StarHandler);
  }

  [Behavior]
  public void Plus_Star()
  {
    var Category = GetCategory(Plus, Star);

    var Result = Mind.ChooseHandlerFor(SamplePlus, Category);

    Assert.That(Result).Is(PlusHandler);
  }

  [Behavior]
  public void Irregular_Circle()
  {
    var Category = GetCategory(Irregular, Circle);

    var Result = Mind.ChooseHandlerFor(SampleIrregular, Category);

    Assert.That(Result).Is(IrregularHandler);
  }

  [Behavior]
  public void Circle_Irregular()
  {
    var Category = GetCategory(Circle, Irregular);

    var Result = Mind.ChooseHandlerFor(SampleCircle, Category);

    Assert.That(Result).Is(CircleHandler);
  }

  [Behavior]
  public void Irregular_Plus()
  {
    var Category = GetCategory(Irregular, Plus);

    var Result = Mind.ChooseHandlerFor(SampleIrregular, Category);

    Assert.That(Result).Is(IrregularHandler);
  }

  [Behavior]
  public void Plus_Irregular()
  {
    var Category = GetCategory(Plus, Irregular);

    var Result = Mind.ChooseHandlerFor(SamplePlus, Category);

    Assert.That(Result).Is(PlusHandler);
  }
}