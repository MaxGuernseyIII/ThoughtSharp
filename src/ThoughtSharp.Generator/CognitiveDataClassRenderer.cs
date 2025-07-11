﻿// MIT License
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

class CognitiveDataClassRenderer
{
  public static void GenerateCognitiveDataClassContentToWriter(
    CognitiveDataClass CognitiveDataClass, IndentedTextWriter W)
  {
    if (CognitiveDataClass.ExplicitConstructor)
      W.WriteLine($"public {CognitiveDataClass.Address.TypeName.Name}() {{ }}");

    var CodecDictionary = CognitiveDataClass.Codecs.ToDictionary(C => C.Name);

    foreach (var Parameter in CognitiveDataClass.Parameters)
    {
      var ParameterCodec = GetCodecFieldNameFor(Parameter);
      if (CodecDictionary.ContainsKey(ParameterCodec))
        continue;

      W.WriteLine(
        $"internal static readonly CognitiveDataCodec<{Parameter.FullType}> {ParameterCodec} = {Parameter.CodecExpression};");
    }

    var LastValue = "0";
    var LastTokenValue = "0";
    CognitiveParameter? LastParameter = null;

    foreach (var Parameter in CognitiveDataClass.Parameters)
    {
      if (Parameter.Implied)
      {
        var Initializer = Parameter.Initializer is not null ? $" = {Parameter.Initializer}" : "";
        var ParameterFullType = Parameter.FullType;
        if (Parameter.ExplicitCount is not null)
          ParameterFullType += "[]";
        W.WriteLine($"public {ParameterFullType} {Parameter.Name}{Initializer};");
      }

      var ParameterIndexField = GetIndexFieldNameFor(Parameter);
      var ParameterTokenIndexField = GetTokenIndexFieldNameFor(Parameter);
      W.Write($"internal static readonly int {ParameterIndexField} = ");
      WriteIndexValue(W, LastValue, LastParameter, "FloatLength");
      W.WriteLine();
      W.Write($"internal static readonly int {ParameterTokenIndexField} = ");
      WriteIndexValue(W, LastTokenValue, LastParameter, "EncodedTokenClassCounts.Length");
      W.WriteLine();

      LastValue = ParameterIndexField;
      LastTokenValue = ParameterTokenIndexField;
      LastParameter = Parameter;
    }

    W.WriteLine("public static ImmutableArray<long> EncodedTokenClassCounts => [");
    foreach (var Parameter in CognitiveDataClass.Parameters)
      W.WriteLine($"..{GetCodecFieldNameFor(Parameter)}.EncodedTokenClassCounts,");
    W.WriteLine("];");

    W.Write("public static int FloatLength { get; } = ");
    WriteIndexValue(W, LastValue, LastParameter, "FloatLength");

    W.WriteLine("public void MarshalTo(Span<float> __TARGET__, Span<long> __TOKENS__)");
    W.WriteLine("{");
    foreach (var Parameter in CognitiveDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.FloatLength" : "0") + ")";
      var TokenOffset = "(" + GetTokenIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.EncodedTokenClassCounts.Length" : "0") + ")";
      W.WriteLine(
        $"  {GetCodecFieldNameFor(Parameter)}.EncodeTo(");
      W.WriteLine(
        $"    {Parameter.Name}{Subscript},");
      W.WriteLine(
        $"    __TARGET__[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.FloatLength)],");
      W.WriteLine(
        $"    __TOKENS__[{TokenOffset}..({TokenOffset}+{GetCodecFieldNameFor(Parameter)}.EncodedTokenClassCounts.Length)]);");
    }

    W.WriteLine("}");
    W.WriteLine();
    W.WriteLine(
      $"public static {CognitiveDataClass.Address.TypeName.FullName} UnmarshalFrom(ReadOnlySpan<float> __SOURCE__, ReadOnlySpan<long> __TOKENS__)");
    W.WriteLine("{");
    W.WriteLine($"  var Result = new {CognitiveDataClass.Address.TypeName.FullName}();");
    W.WriteLine();

    foreach (var Parameter in CognitiveDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I}]" : "";
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.FloatLength" : "0") + ")";
      var TokenOffset = "(" + GetTokenIndexFieldNameFor(Parameter) + " + " +
                        (Parameter.ExplicitCount.HasValue ? $"{I} * {GetCodecFieldNameFor(Parameter)}.EncodedTokenClassCounts.Length" : "0") + ")";
      W.WriteLine(
        $"  Result.{Parameter.Name}{Subscript} = {GetCodecFieldNameFor(Parameter)}.DecodeFrom(");
      W.WriteLine(
        $"    __SOURCE__[{Offset}..({Offset}+{GetCodecFieldNameFor(Parameter)}.FloatLength)],");
      W.WriteLine(
        $"    __TOKENS__[{TokenOffset}..({TokenOffset}+{GetCodecFieldNameFor(Parameter)}.EncodedTokenClassCounts.Length)]);");
    }

    W.WriteLine("  return Result;");
    W.WriteLine("}");

    W.WriteLine();
    W.WriteLine(
      $"public void WriteAsLossRules(LossRuleWriter Writer)");
    W.WriteLine("{");
    W.Indent++;
    foreach (var Parameter in CognitiveDataClass.Parameters)
    foreach (var I in Enumerable.Range(0, Parameter.EffectiveCount))
    {
      var Subscript = Parameter.ExplicitCount.HasValue ? $"[{I.ToLiteralExpression()}]" : "";
      var CodecFieldNameForParameter = GetCodecFieldNameFor(Parameter);
      var Offset = "(" + GetIndexFieldNameFor(Parameter) + " + " +
                   (Parameter.ExplicitCount.HasValue ? $"{I} * {CodecFieldNameForParameter}.FloatLength" : "0") + ")";
      W.WriteLine(
        $"{CodecFieldNameForParameter}.WriteLossRulesFor({Parameter.Name}{Subscript}, Writer.ForOffset({Offset}));");
    }
    W.Indent--;
    W.WriteLine("}");

    W.WriteLine();

    using (W.DeclareWithBlock("public static void WriteIsolationBoundaries(IsolationBoundariesWriter Writer)"))
      foreach (var Parameter in CognitiveDataClass.Parameters) 
        W.WriteLine($"{GetCodecFieldNameFor(Parameter)}.WriteIsolationBoundaries(Writer.AddOffset({GetIndexFieldNameFor(Parameter)}));");
  }

  static void WriteIndexValue(IndentedTextWriter Target, string LastValue, CognitiveParameter? LastParameter, string LengthExpression)
  {
    Target.Write($"{LastValue}");
    if (LastParameter is not null)
      Target.Write($" + {GetCodecFieldNameFor(LastParameter)}.{LengthExpression} * {LastParameter.EffectiveCount}");
    Target.WriteLine(";");
  }

  static string GetIndexFieldNameFor(CognitiveParameter Parameter)
  {
    return $"{Parameter.Name}Index";
  }

  static string GetTokenIndexFieldNameFor(CognitiveParameter Parameter)
  {
    return $"{Parameter.Name}TokenIndex";
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
        new(CognitiveDataObject.Address)
        {
          WriteBody = W =>
          {
            GenerateCognitiveDataClassContentToWriter(CognitiveDataObject, W);
          },
          WriteHeader = W =>
          {
            W.WriteLine("using ThoughtSharp.Runtime;");
            W.WriteLine("using ThoughtSharp.Runtime.Codecs;");
            W.WriteLine("using System.Collections.Immutable;");
            W.WriteLine();
          },
          WriteBeforeTypeDeclaration = W =>
          {
            if (CognitiveDataObject.IsPublic)
              W.Write("public ");
          },
          WriteAfterTypeName = W => { W.Write($" : CognitiveData<{CognitiveDataObject.Address.TypeName.FullName}>"); }
        });
    }

    Underlying.Close();

    return Underlying.GetStringBuilder().ToString();
  }
}