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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ThoughtSharp.Generator;

static class ThoughtDataModelFactory
{
  public static ThoughtDataClass ConvertToModel(GeneratorAttributeSyntaxContext InnerContext)
  {
    var Codecs = new List<ThoughtParameterCodec>();
    var Parameters = new List<ThoughtParameter>();
    var Symbol = (INamedTypeSymbol) InnerContext.TargetSymbol;
    var ValueSymbols = Symbol.GetMembers().Select(M => M.ToValueSymbolOrDefault())
      .OfType<IValueSymbol>()
      .Where(M => !M.IsImplicitlyDeclared)
      .ToImmutableArray();

    foreach (var Member in ValueSymbols.Where(M => !M.IsStatic))
      Parameters.Add(CreateParameterFor(Member));

    foreach (var Member in ValueSymbols.Where(M => M.IsStatic))
      Codecs.Add(CreateCodecFor(Member));

    return new(TypeAddress.ForSymbol(Symbol), [..Parameters], [..Codecs]);
  }

  static ThoughtParameterCodec CreateCodecFor(IValueSymbol Member)
  {
    return new(Member.Name);
  }

  static ThoughtParameter CreateParameterFor(IValueSymbol Member)
  {
    var ExplicitCount = GetExplicitCount(Member.Raw);
    var ExplicitLength = GetExplicitLength(Member.Raw);
    var Bounds = GetExplicitBounds(Member.Raw) ?? GetImplicitBounds(Member.Type);
    var EncodedType = ExplicitCount.HasValue
      ? TryGetArrayType(Member.Type) ?? TryGetIndexableType(Member.Type) ?? Member.Type
      : Member.Type;

    var CodecConstructorArguments = new Dictionary<string, string>();
    if (ExplicitLength is not null)
      CodecConstructorArguments["Length"] = GetLiteralFor(ExplicitLength);

    var CodecType = GetCodecType(EncodedType, Member);

    if (Bounds is var (Minimum, Maximum))
    {
      CodecConstructorArguments["Minimum"] = GetLiteralFor(Minimum);
      CodecConstructorArguments["Maximum"] = GetLiteralFor(Maximum);
      CodecConstructorArguments["Inner"] = "new CopyFloatCodec()";
      CodecType = $"NormalizeNumberCodec<{GetFullPath(EncodedType)}, float>";
    }

    return new(Member.Name, CodecType, ExplicitCount, CodecConstructorArguments);
  }

  static (object Minimum, object Maximum)? GetImplicitBounds(ITypeSymbol MemberType)
  {
    return MemberType.SpecialType switch
    {
      SpecialType.System_SByte => (sbyte.MinValue, sbyte.MaxValue),
      SpecialType.System_Byte => (byte.MinValue, byte.MaxValue),
      SpecialType.System_Int16 => (short.MinValue, short.MaxValue),
      SpecialType.System_UInt16 => (ushort.MinValue, ushort.MaxValue),
      SpecialType.System_Char => (char.MinValue, char.MaxValue),
      _ => null
    };
  }

  static (object Minimum, object Maximum)? GetExplicitBounds(ISymbol Symbol)
  {
    foreach (var Attribute in Symbol.GetAttributes()
               .Where(A => A.AttributeClass?.Name == ThoughtDataAttributeNames.DataBoundsAttributeName))
      return (Attribute.ConstructorArguments[0].Value!, Attribute.ConstructorArguments[1].Value!);

    return null;
  }

  static string GetLiteralFor(object? Value)
  {
    var Expression = Value switch
    {
      null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
      bool B => SyntaxFactory.LiteralExpression(
        B ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
      short S => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(S)),
      ushort S => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(S)),
      int I => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(I)),
      uint I => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(I)),
      long L => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(L)),
      ulong L => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(L)),
      float F => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(F)),
      double D => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(D)),
      decimal M => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(M)),
      byte B => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(B)),
      sbyte B => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(B)),
      string S => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(S)),
      char C => SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(C)),
      Enum E => SyntaxFactory.ParseExpression(E.GetType().FullName + "." + E),
      _ => throw new NotSupportedException($"Literal generation not supported for type: {Value.GetType()}")
    };
    return Expression.NormalizeWhitespace().ToFullString();
  }

  static string GetCodecType(ITypeSymbol EncodedType, IValueSymbol Member)
  {
    return EncodedType switch
    {
      {SpecialType: SpecialType.System_Single} => "CopyFloatCodec",
      {SpecialType: SpecialType.System_Boolean} => "CopyBoolCodec",
      {
        SpecialType:
        SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Char
      } => GetIntLikeNumberCodec(EncodedType, Member),
      INamedTypeSymbol {TypeKind: TypeKind.Enum} Enum =>
        $"BitwiseOneHotEnumCodec<{Enum.Name}, {Enum.EnumUnderlyingType?.Name ?? "int"}>",
      {SpecialType: SpecialType.System_String} => GetStringCodec(Member),
      var T when T.GetAttributes().Any(A => A.AttributeClass?.Name == ThoughtDataAttributeNames.DataAttributeName)
        => "SubDataCodec<" + GetFullPath(T) + ">",
      _ => "UnknownCodec"
    };
  }

  static string GetFullPath(ITypeSymbol T)
  {
    return T.ToDisplayString(new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
  }

  static string GetIntLikeNumberCodec(ITypeSymbol EncodedType, IValueSymbol Member)
  {
    return $"BitwiseOneHotNumberCodec<{GetFullPath(EncodedType)}>";
  }

  static string GetStringCodec(IValueSymbol Member)
  {
    var Length = GetExplicitLength(Member.Raw);

    if (Length is null)
      return "UnknownCodec";

    return "BitwiseOneHotStringCodec";
  }

  static int? GetExplicitLength(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == ThoughtDataAttributeNames.DataLengthAttributeName))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }

  static int? GetExplicitCount(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == ThoughtDataAttributeNames.DataCountAttributeName))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }

  static ITypeSymbol? TryGetArrayType(ITypeSymbol Symbol)
  {
    if (Symbol is not IArrayTypeSymbol ArrayType)
      return null;

    return ArrayType.ElementType;
  }

  static ITypeSymbol? TryGetIndexableType(ITypeSymbol Symbol)
  {
    var Indexer = Symbol
      .GetMembers()
      .OfType<IPropertySymbol>()
      .FirstOrDefault(p =>
        p.IsIndexer &&
        p.Parameters.Length == 1 &&
        p.Parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
        p is {GetMethod: not null, SetMethod: not null});

    if (Indexer is null)
      return null;

    return Indexer.Type;
  }
}