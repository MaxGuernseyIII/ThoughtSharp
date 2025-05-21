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

namespace ThoughtSharp.Generator;

static class ThoughtDataModelBuilder
{
  const string ThoughtDataCountAttribute = "ThoughtDataCountAttribute";

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
    var EncodedType = ExplicitCount.HasValue
      ? TryGetArrayType(Member.Type) ?? TryGetIndexableType(Member.Type) ?? Member.Type
      : Member.Type;

    var CodecType = GetCodecType(EncodedType);

    return new(Member.Name, TypeAddress.ForSymbol(EncodedType), ExplicitCount, CodecType);
  }

  static string GetCodecType(ITypeSymbol EncodedType)
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
          SpecialType.System_Int64 or SpecialType.System_UInt64
        } => $"OneHotEncodeCodec<{EncodedType.Name}>",
      INamedTypeSymbol {TypeKind: TypeKind.Enum} Enum => $"OneHotEncodeEnumCodec<{Enum.Name}, {Enum.EnumUnderlyingType?.Name ?? "int"}>",
      _ => "UnknownCodec"
    };
  }

  static int? GetExplicitCount(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes().Where(A => A.AttributeClass?.Name == ThoughtDataCountAttribute))
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