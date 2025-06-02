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

using System.CodeDom.Compiler;

namespace ThoughtSharp.Generator;

static class CognitiveCategoryRenderer
{
  public static string GenerateCategoryType(CognitiveCategoryModel Model)
  {
    var RenderingOperation = new RenderingOperation(Model);
    using var StringWriter = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(StringWriter, "  ");

      GeneratedTypeFormatter.GenerateType(Writer, new(Model.CategoryType)
      {
        WriteBody = RenderingOperation.GenerateBody,
        WriteHeader = RenderingOperation.GenerateHeader,
        WriteAfterTypeName = RenderingOperation.AdornTypeName
      });
    }

    StringWriter.Close();

    return StringWriter.ToString();
  }

  class RenderingOperation(CognitiveCategoryModel Model)
  {
    CognitiveCategoryModel Model { get; } = Model;
    string TypeParameters { get; } = $"<{Model.PayloadType.FullName}, {Model.DescriptorType.FullName}>";

    public void GenerateBody(IndentedTextWriter W)
    {
      GenerateAllOptionsProperty(W);
      GenerateGetContestBetweenMethod(W);
      GenerateInterpretMethod(W);
    }

    void GenerateGetContestBetweenMethod(IndentedTextWriter W)
    {
      using (
        W.DeclareWithBlock($"public Input GetContestBetween({OptionType} Left, {OptionType} Right)"))
      {
        using (W.EnterBlock("return new()", ";"))
        {
          W.WriteLine("Left = Left.Descriptor,");
          W.WriteLine("Right = Right.Descriptor");
        }
      }
    }

    void GenerateInterpretMethod(IndentedTextWriter W)
    {
      var PayloadType = Model.PayloadType.FullName;
      var FeedbackType = $"SingleChoiceFeedbackSink<{PayloadType}, Output>";
      var FeedbackReturnType = $"SingleChoiceFeedbackSink<{PayloadType}>";
      using (W.DeclareWithBlock(
               $"public Thought<{OptionType}, {FeedbackReturnType}> Interpret({OptionType} Left, {OptionType} Right, Output O, Inference I, int Offset)"))
      {
        W.WriteLine($"return Thought.Capture<{OptionType}, {FeedbackReturnType}>(O.RightIsWinner ? Right : Left, new {FeedbackType}(I, Left.Payload, Right.Payload, B => new() {{ RightIsWinner = B }}, Offset));");
      }
    }

    string OptionType => $"CognitiveOption<{Model.PayloadType.FullName}, {Model.DescriptorType.FullName}>";

    void GenerateAllOptionsProperty(IndentedTextWriter W)
    {
      W.WriteLine($"public IReadOnlyList<{OptionType}> AllOptions {{ get; }} = Options;");
      W.WriteLine();
    }

    public void GenerateHeader(IndentedTextWriter W)
    {
      W.WriteLine("using ThoughtSharp.Runtime;");
      W.WriteLine();
    }

    public void AdornTypeName(IndentedTextWriter W)
    {
      W.WriteLine($"(IReadOnlyList<CognitiveOption{TypeParameters}> Options)");
      W.Write($"  : CognitiveCategory{TypeParameters}");
    }
  }
}