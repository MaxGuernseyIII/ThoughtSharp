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
    var LineShape = SampleLine;
    var LineOption = GetOptionFor(Line);
    var Category = GetCategory(LineOption, GetOptionFor(Rectangle));

    var Result = Mind.ChooseHandlerFor(LineShape, Category);

    Assert.That(Result).Is(LineOption.Payload);
  }

  [Behavior]
  public void Rectangle_Line()
  {
    var RectangleShape = SampleRectangle;
    var RectangleOption = GetOptionFor(Rectangle);
    var Category = GetCategory(RectangleOption, GetOptionFor(Line));

    var Result = Mind.ChooseHandlerFor(RectangleShape, Category);

    Assert.That(Result).Is(RectangleOption.Payload);
  }
  [Behavior]
  public void Circle_Rectangle()
  {
    var CircleShape = SampleCircle;
    var CircleOption = GetOptionFor(Circle);
    var Category = GetCategory(CircleOption, GetOptionFor(Rectangle));

    var Result = Mind.ChooseHandlerFor(CircleShape, Category);

    Assert.That(Result).Is(CircleOption.Payload);
  }

  [Behavior]
  public void Rectangle_Circle()
  {
    var RectangleShape = SampleRectangle;
    var RectangleOption = GetOptionFor(Rectangle);
    var Category = GetCategory(RectangleOption, GetOptionFor(Circle));

    var Result = Mind.ChooseHandlerFor(RectangleShape, Category);

    Assert.That(Result).Is(RectangleOption.Payload);
  }
}