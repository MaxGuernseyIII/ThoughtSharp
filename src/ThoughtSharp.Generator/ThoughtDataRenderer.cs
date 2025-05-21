// MIT License
// 
// Copyright (c) 2024-2024 Hexagon Software LLC
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

using System.Text;

namespace ThoughtSharp.Generator;

class ThoughtDataRenderer
{
  public static string GenerateThoughtDataContent(ThoughtDataClass ThoughtDataClass)
  {
    var ResultBuilder = new StringBuilder();
    var CodecDictionary = ThoughtDataClass.Codecs.ToDictionary(C => C.Name);

    foreach (var Parameter in ThoughtDataClass.Parameters)
    {
      var ParameterCodec = GetCodecFieldNameFor(Parameter);
      if (CodecDictionary.ContainsKey(ParameterCodec))
        continue;

      ResultBuilder.AppendLine($"static FloatCopyCodec {ParameterCodec} = new();");
    }


    var LastValue = "0";
    ThoughtParameter? LastParameter = null;

    foreach (var Parameter in ThoughtDataClass.Parameters)
    {
      var ParameterIndexField = GetIndexFieldNameFor(Parameter);
      ResultBuilder.Append($"static readonly int {ParameterIndexField} = ");
      WriteIndexValue(ResultBuilder, LastValue, LastParameter);

      LastValue = ParameterIndexField;
      LastParameter = Parameter;
    }

    ResultBuilder.Append("public static int Length { get; } = ");
    WriteIndexValue(ResultBuilder, LastValue, LastParameter);

    ResultBuilder.AppendLine("public void MarshalTo(Span<float> Target)");
    ResultBuilder.AppendLine("{");
    foreach (var Parameter in ThoughtDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " + (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.Length" : "0") + ")";
      ResultBuilder.AppendLine(
        $"  {GetCodecFieldNameFor(Parameter)}.EncodeTo({Parameter.Name}{Subscript}, Target[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.Length)]);");

    }

    ResultBuilder.AppendLine("}");
    ResultBuilder.AppendLine();
    ResultBuilder.AppendLine("public void MarshalFrom(ReadOnlySpan<float> Target)");
    ResultBuilder.AppendLine("{");

    foreach (var Parameter in ThoughtDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " + (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.Length" : "0") + ")";
      ResultBuilder.AppendLine(
        $"  {Parameter.Name}{Subscript} = {GetCodecFieldNameFor(Parameter)}.DecodeFrom(Target[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.Length)]);");

    }
    ResultBuilder.AppendLine("}");
    ResultBuilder.AppendLine();

    return ResultBuilder.ToString();
  }

  static void WriteIndexValue(StringBuilder ResultBuilder, string LastValue, ThoughtParameter? LastParameter)
  {
    ResultBuilder.Append($"{LastValue}");
    if (LastParameter is not null)
      ResultBuilder.Append($"+ {GetCodecFieldNameFor(LastParameter)}.Length * {LastParameter.EffectiveCount}");
    ResultBuilder.AppendLine(";");
  }

  static string GetIndexFieldNameFor(ThoughtParameter Parameter)
  {
    return $"{Parameter.Name}Index";
  }

  static string GetCodecFieldNameFor(ThoughtParameter Parameter)
  {
    return $"{Parameter.Name}Codec";
  }

  public static string RenderThoughtDataClass(ThoughtDataClass ThoughtDataObject)
  {
    return GeneratedTypeFormatter.FrameInPartialType(ThoughtDataObject.Address, ThoughtDataRenderer.GenerateThoughtDataContent(ThoughtDataObject), " : ThoughtData");
  }
}