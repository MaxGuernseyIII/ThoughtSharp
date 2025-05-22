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

class CognitiveDataClassRenderer
{
  public static string GenerateCognitiveDataClassContentToWriter(
    CognitiveDataClass CognitiveDataClass, IndentedTextWriter Target)
  {
    var CodecDictionary = CognitiveDataClass.Codecs.ToDictionary(C => C.Name);

    foreach (var Parameter in CognitiveDataClass.Parameters)
    {
      var ParameterCodec = GetCodecFieldNameFor(Parameter);
      if (CodecDictionary.ContainsKey(ParameterCodec))
        continue;

      Target.WriteLine($"static readonly {Parameter.CodecType} {ParameterCodec} = {Parameter.CodecExpression};");
    }

    var LastValue = "0";
    CognitiveParameter? LastParameter = null;

    foreach (var Parameter in CognitiveDataClass.Parameters)
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
    foreach (var Parameter in CognitiveDataClass.Parameters)
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

    foreach (var Parameter in CognitiveDataClass.Parameters)
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

  static void WriteIndexValue(IndentedTextWriter Target, string LastValue, CognitiveParameter? LastParameter)
  {
    Target.Write($"{LastValue}");
    if (LastParameter is not null)
      Target.Write($"+ {GetCodecFieldNameFor(LastParameter)}.Length * {LastParameter.EffectiveCount}");
    Target.WriteLine(";");
  }

  static string GetIndexFieldNameFor(CognitiveParameter Parameter)
  {
    return $"{Parameter.Name}Index";
  }

  static string GetCodecFieldNameFor(CognitiveParameter Parameter)
  {
    return $"{Parameter.Name}Codec";
  }

  public static string RenderCognitiveDataClass(CognitiveDataClass CognitiveDataObject)
  {
    using var Underlying = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(Underlying, "  ");
      GeneratedTypeFormatter.GenerateType(Writer,
        new(CognitiveDataObject.Address, W => { GenerateCognitiveDataClassContentToWriter(CognitiveDataObject, W); })
        {
          WriteHeader = W =>
          {
            W.WriteLine("using ThoughtSharp.Runtime;");
            W.WriteLine("using ThoughtSharp.Runtime.Codecs;");
            W.WriteLine();
          },
          WriteAfterTypeName = W => { W.Write($" : CognitiveData<{CognitiveDataObject.Address.TypeName.FullName}>"); }
        });
    }

    Underlying.Close();

    return Underlying.GetStringBuilder().ToString();
  }
}