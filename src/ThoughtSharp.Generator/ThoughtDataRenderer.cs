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

using System.CodeDom.Compiler;

namespace ThoughtSharp.Generator;

class ThoughtDataRenderer
{
  public static string GenerateThoughtDataContentToWriter(ThoughtDataClass ThoughtDataClass, IndentedTextWriter Target)
  {
    var CodecDictionary = ThoughtDataClass.Codecs.ToDictionary(C => C.Name);

    foreach (var Parameter in ThoughtDataClass.Parameters)
    {
      var ParameterCodec = GetCodecFieldNameFor(Parameter);
      if (CodecDictionary.ContainsKey(ParameterCodec))
        continue;

      var Arguments = "";
      if (Parameter.ExplicitLength is not null)
        Arguments = $"Length: {Parameter.ExplicitLength}";

      Target.WriteLine($"static {Parameter.CodecType} {ParameterCodec} = new({Arguments});");
    }

    var LastValue = "0";
    ThoughtParameter? LastParameter = null;

    foreach (var Parameter in ThoughtDataClass.Parameters)
    {
      var ParameterIndexField = GetIndexFieldNameFor(Parameter);
      Target.Write($"static readonly int {ParameterIndexField} = ");
      WriteIndexValue(Target, LastValue, LastParameter);

      LastValue = ParameterIndexField;
      LastParameter = Parameter;
    }

    Target.Write("public static int Length { get; } = ");
    WriteIndexValue(Target, LastValue, LastParameter);

    Target.WriteLine("public void MarshalTo(Span<float> Target)");
    Target.WriteLine("{");
    foreach (var Parameter in ThoughtDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.Length" : "0") + ")";
      Target.WriteLine(
        $"  {GetCodecFieldNameFor(Parameter)}.EncodeTo({Parameter.Name}{Subscript}, Target[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.Length)]);");
    }

    Target.WriteLine("}");
    Target.WriteLine();
    Target.WriteLine("public void MarshalFrom(ReadOnlySpan<float> Target)");
    Target.WriteLine("{");

    foreach (var Parameter in ThoughtDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.Length" : "0") + ")";
      Target.WriteLine(
        $"  {Parameter.Name}{Subscript} = {GetCodecFieldNameFor(Parameter)}.DecodeFrom(Target[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.Length)]);");
    }

    Target.WriteLine("}");

    return Target.ToString();
  }

  static void WriteIndexValue(IndentedTextWriter Target, string LastValue, ThoughtParameter? LastParameter)
  {
    Target.Write($"{LastValue}");
    if (LastParameter is not null)
      Target.Write($"+ {GetCodecFieldNameFor(LastParameter)}.Length * {LastParameter.EffectiveCount}");
    Target.WriteLine(";");
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
    using var Underlying = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(Underlying, "  ");
      GeneratedTypeFormatter.GenerateType(Writer,
        new(ThoughtDataObject.Address, W => { GenerateThoughtDataContentToWriter(ThoughtDataObject, W); })
        {
          WriteHeader = W =>
          {
            W.WriteLine("using ThoughtSharp.Runtime;");
            W.WriteLine("using ThoughtSharp.Runtime.Codecs;");
            W.WriteLine();
          },
          WriteAfterTypeName = W => { W.Write(" : ThoughtData"); }
        });
    }

    Underlying.Close();

    return Underlying.GetStringBuilder().ToString();
  }
}