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

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThoughtSharp.Generator;

class TypeIdentifier
{
  TypeIdentifier(string Keyword, string Name, IReadOnlyList<TypeAddress> TypeParameters)
  {
    this.Keyword = Keyword;
    this.Name = Name;
    this.TypeParameters = TypeParameters;
  }

  public string Keyword { get; }
  public string Name { get; }
  public IReadOnlyList<TypeAddress> TypeParameters { get; }

  public static TypeIdentifier ForSymbol(ITypeSymbol Symbol)
  {
    var Arguments = Symbol is INamedTypeSymbol Named
      ? Named.TypeArguments.Select(TypeAddress.ForSymbol).ToImmutableList()
      : [];

    return new(GetTypeKeyword(Symbol), Symbol.Name, Arguments);
  }

  static string GetTypeKeyword(ITypeSymbol symbol)
  {
    return symbol.TypeKind switch
    {
      TypeKind.Class when symbol.IsRecord => "record",
      TypeKind.Class => "class",
      TypeKind.Struct when symbol.IsRecord => "record struct",
      TypeKind.Struct => "struct",
      TypeKind.Interface => "interface",
      TypeKind.Enum => "enum",
      TypeKind.Delegate => "delegate",
      _ => "unknown"
    };
  }
}

class TypeAddress
{
  TypeAddress(
    IReadOnlyList<string> ContainingNamespaces,
    IReadOnlyList<TypeIdentifier> ContainingTypes,
    TypeIdentifier TypeName)
  {
    this.ContainingNamespaces = ContainingNamespaces;
    this.ContainingTypes = ContainingTypes;
    this.TypeName = TypeName;
  }

  public IReadOnlyList<string> ContainingNamespaces { get; }
  public IReadOnlyList<TypeIdentifier> ContainingTypes { get; }
  public TypeIdentifier TypeName { get; }

  public static TypeAddress ForSymbol(ITypeSymbol Symbol)
  {
    var ContainingNamespaces = new List<string>();
    var ContainingTypes = new List<TypeIdentifier>();
    var TypeName = TypeIdentifier.ForSymbol(Symbol);
    var CurrentType = Symbol.ContainingType;

    while (CurrentType != null)
    {
      ContainingTypes.Insert(0, TypeIdentifier.ForSymbol(CurrentType));
      CurrentType = CurrentType.ContainingType;
    }

    var CurrentNamespace = Symbol.ContainingNamespace;

    while (CurrentNamespace is {IsGlobalNamespace: false})
    {
      ContainingNamespaces.Insert(0, CurrentNamespace.Name);
      CurrentNamespace = CurrentNamespace.ContainingNamespace;
    }

    return new(ContainingNamespaces, ContainingTypes, TypeName);
  }
}

static class GeneratedTypeFormatter
{
  public static string GetFilename(TypeAddress Address)
  {
    return "thought/" + string.Join(".",
      Address.ContainingNamespaces.SelectMany(N => N.Split('.'))
        .Concat(Address.ContainingTypes.Select(T => T.Name)
          .Concat([Address.TypeName.Name]))) + ".g.cs";
  }

  public static string FrameInPartialType(TypeAddress Address, string Content, string TypeSuffix = "")
  {
    var Lines = Content.Split(["\r\n", "\n"], StringSplitOptions.None);
    var OutputBuilder = new StringBuilder(5 * Content.Length);
    OutputBuilder.AppendLine("using ThoughtSharp.Runtime;");
    OutputBuilder.AppendLine("using ThoughtSharp.Runtime.Codecs;");
    OutputBuilder.AppendLine();

    var IndentLevel = 0;

    foreach (var Namespace in Address.ContainingNamespaces)
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}namespace {Namespace}");
      OutputBuilder.AppendLine($"{Indent}{{");
      IndentLevel += 1;
    }

    foreach (var Type in Address.ContainingTypes)
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}partial {Type.Keyword} {Type.Name}");
      OutputBuilder.AppendLine($"{Indent}{{");

      IndentLevel += 1;
    }

    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}partial {Address.TypeName.Keyword} {Address.TypeName.Name}{TypeSuffix}");
      OutputBuilder.AppendLine($"{Indent}{{");

      IndentLevel += 1;
    }

    {
      var Indent = GetIndentForLevel(IndentLevel);

      foreach (var Line in Lines)
      {
        OutputBuilder.Append(Indent);
        OutputBuilder.AppendLine(Line);
      }
    }

    foreach (var _ in Address.ContainingTypes.Concat([Address.TypeName]))
    {
      IndentLevel -= 1;
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}}}");
    }

    foreach (var _ in Address.ContainingNamespaces)
    {
      IndentLevel -= 1;
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}}}");
    }

    return OutputBuilder.ToString();
  }

  static string GetIndentForLevel(int Level)
  {
    return new(' ', 2 * Level);
  }
}

class ThoughtParameter(string Name, TypeAddress Type, int? ExplicitCount)
{
  public string Name { get; } = Name;
  public TypeAddress Type { get; } = Type;
  public int? ExplicitCount { get; } = ExplicitCount;

  public int EffectiveCount => ExplicitCount ?? 1;
}

class ThoughtDataClass(
  TypeAddress Address,
  IReadOnlyList<ThoughtParameter> Parameters,
  ImmutableArray<ThoughtParameterCodec> Codecs)
{
  public TypeAddress Address { get; } = Address;
  public IReadOnlyList<ThoughtParameter> Parameters { get; } = Parameters;
  public ImmutableArray<ThoughtParameterCodec> Codecs { get; } = Codecs;
}

interface IValueSymbol
{
  string Name { get; }
  ITypeSymbol Type { get; }
  bool IsStatic { get; }
  ISymbol Raw { get; }
}

static class SymbolExtensions
{
  public static IValueSymbol? ToValueSymbolOrDefault(this ISymbol ToAdapt)
  {
    if (ToAdapt is IFieldSymbol Field)
      return new FieldValueSymbol(Field);

    if (ToAdapt is IPropertySymbol Property)
      return new PropertyValueSymbolAdapter(Property);

    return null;
  }

  class FieldValueSymbol(IFieldSymbol Field) : IValueSymbol
  {
    public string Name => Field.Name;

    public ITypeSymbol Type => Field.Type;

    public bool IsStatic => Field.IsStatic;

    public ISymbol Raw => Field;
  }

  class PropertyValueSymbolAdapter(IPropertySymbol Property) : IValueSymbol
  {
    public string Name => Property.Name;

    public ITypeSymbol Type => Property.Type;

    public bool IsStatic => Property.IsStatic;

    public ISymbol Raw => Property;
  }
}

[Generator]
public class ThoughtSharpGenerator : IIncrementalGenerator
{
  const string ThoughtDataAttribute = "ThoughtSharp.Runtime.ThoughtDataAttribute";
  const string ThoughtDataLengthAttribute = "ThoughtDataLengthAttribute";

  public void Initialize(IncrementalGeneratorInitializationContext Context)
  {
    var ThoughtDataPipeline = Context.SyntaxProvider.ForAttributeWithMetadataName(
      ThoughtDataAttribute,
      (Node, _) => Node is TypeDeclarationSyntax,
      (InnerContext, _) =>
      {
        var Codecs = new List<ThoughtParameterCodec>();
        var Parameters = new List<ThoughtParameter>();
        var Symbol = (INamedTypeSymbol) InnerContext.TargetSymbol;
        var ValueSymbols = Symbol.GetMembers().Select(M => M.ToValueSymbolOrDefault())
          .OfType<IValueSymbol>()
          .ToImmutableArray();

        foreach (var Member in ValueSymbols.Where(M => !M.IsStatic))
          Parameters.Add(new(Member.Name, TypeAddress.ForSymbol(Member.Type), GetExplicitLength(Member.Raw)));

        foreach (var Member in Symbol.GetMembers().Where(M => M.IsStatic).OfType<IPropertySymbol>())
          Codecs.Add(new(Member.Name));

        return new ThoughtDataClass(TypeAddress.ForSymbol(Symbol), [..Parameters], [..Codecs]);
      });

    Context.RegisterSourceOutput(
      ThoughtDataPipeline,
      (InnerContext, ThoughtDataObject) =>
      {
        InnerContext.AddSource(
          GeneratedTypeFormatter.GetFilename(ThoughtDataObject.Address),
          GeneratedTypeFormatter.FrameInPartialType(ThoughtDataObject.Address,
            GenerateThoughtDataContent(ThoughtDataObject), " : ThoughtData"));
      });
  }

  int? GetExplicitLength(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes().Where(A => A.AttributeClass?.Name == ThoughtDataLengthAttribute))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }

  static string GenerateThoughtDataContent(ThoughtDataClass ThoughtDataClass)
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
}

public class ThoughtParameterCodec(string Name)
{
  public string Name { get; } = Name;
}