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

using Microsoft.CodeAnalysis;

namespace ThoughtSharp.Generator;

class CognitiveDataClassBuilder(TypeAddress TypeAddress)
{
  readonly List<CognitiveParameterCodec> Codecs = [];
  readonly List<CognitiveParameter> Parameters = [];
  public bool IsPublic { get; set; }
  public bool ExplicitConstructor { get; set; }

  public CognitiveDataClass Build()
  {
    return new(TypeAddress, [..Parameters], [..Codecs], IsPublic, ExplicitConstructor);
  }

  public void AddParameterValue(IValueSymbol Member, bool Implied = false)
  {
    Parameters.Add(CreateParameterFor(Member, Implied));
  }

  public void AddCompilerDefinedParameter(
    string Name, string CodecExpression, int? ExplicitCount, string FullType, string? Initializer = null)
  {
    Parameters.Add(new(Name, CodecExpression, ExplicitCount, true, FullType, Initializer));
  }

  public static CognitiveParameterCodec CreateCodecFor(IValueSymbol Member)
  {
    return new(Member.Name);
  }

  public static CognitiveParameter CreateParameterFor(IValueSymbol Member, bool Implied)
  {
    var ExplicitCount = GetExplicitCount(Member.Raw);
    var EncodedType = ExplicitCount.HasValue
      ? TryGetArrayType(Member.Type) ?? TryGetIndexableType(Member.Type) ?? Member.Type
      : Member.Type;

    var CodecExpression = GetCodecExpression(EncodedType, Member);

    return new(Member.Name, CodecExpression, ExplicitCount, Implied,
      EncodedType.GetFullPath(), null);
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
               .Where(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataBoundsAttributeName))
      return (Attribute.ConstructorArguments[0].Value!, Attribute.ConstructorArguments[1].Value!);

    return null;
  }

  static string GetCodecExpression(ITypeSymbol EncodedType, IValueSymbol Member)
  {
    return EncodedType switch
    {
      {SpecialType: SpecialType.System_Single} => GetFloatCodecExpression(Member),
      {SpecialType: SpecialType.System_Boolean} => "new CopyBoolCodec()",
      {
        SpecialType:
        SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Char
      } => GetIntLikeNumberCodec(EncodedType, Member),
      INamedTypeSymbol {TypeKind: TypeKind.Enum} Enum =>
        $"new BitwiseOneHotEnumCodec<{Enum.Name}, {Enum.EnumUnderlyingType?.Name ?? "int"}>()",
      {SpecialType: SpecialType.System_String} => GetStringCodec(Member),
      var T when T.GetAttributes().Any(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataAttributeName)
        => "new SubDataCodec<" + T.GetFullPath() + ">()",
      _ => "new UnknownCodec()"
    };
  }

  static string GetFloatCodecExpression(IValueSymbol Member)
  {
    var ExplicitBounds = GetExplicitBounds(Member.Raw);
    if (ExplicitBounds is var (Minimum, Maximum))
      return
        $"new NormalizingCodec<float>(Inner: new CopyFloatCodec(), Minimum: {Minimum.ToLiteralExpression()}, Maximum: {Maximum.ToLiteralExpression()})";

    return "new CopyFloatCodec()";
  }

  static string GetIntLikeNumberCodec(ITypeSymbol EncodedType, IValueSymbol Member)
  {
    var Bounds = GetExplicitBounds(Member.Raw) ?? GetImplicitBounds(EncodedType);

    if (Bounds is var (Minimum, Maximum))
      return
        GetBoundedIntLikeCodecName(EncodedType.GetFullPath(), Minimum, Maximum);

    return $"new BitwiseOneHotNumberCodec<{EncodedType.GetFullPath()}>()";
  }

  public static string GetBoundedIntLikeCodecName(string TypeName, object Minimum, object Maximum)
  {
    return
      $"new NumberToFloatingPointCodec<{TypeName}, float>(Inner: new RoundingCodec<float>(Inner: new NormalizingCodec<float>(Inner: new CopyFloatCodec(),Minimum: {Minimum.ToLiteralExpression()},Maximum: {Maximum.ToLiteralExpression()})))";
  }

  static string GetStringCodec(IValueSymbol Member)
  {
    var Length = GetExplicitLength(Member.Raw);

    if (Length is null)
      return "UnknownCodec";

    return $"new BitwiseOneHotStringCodec({Length.ToLiteralExpression()})";
  }

  static int? GetExplicitLength(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataLengthAttributeName))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }

  static int? GetExplicitCount(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataCountAttributeName))
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

  public void AddCodecValue(IValueSymbol Member)
  {
    Codecs.Add(CreateCodecFor(Member));
  }

  public void AddCompilerDefinedSubDataArrayParameter(string Name, string TypeName, int Count)
  {
    AddCompilerDefinedParameter(Name, $"new SubDataCodec<{TypeName}>()", Count, $"{TypeName}",
      $"new {TypeName}[{Count.ToLiteralExpression()}]");
  }

  public void AddCompilerDefinedSubDataParameter(string Name, string TypeName)
  {
    AddCompilerDefinedParameter(Name, $"new SubDataCodec<{TypeName}>()", null, TypeName, "new()");
  }

  public void AddCompilerDefinedBoolParameter(string Name)
  {
    AddCompilerDefinedParameter(Name, "new CopyBoolCodec()", null, "bool");
  }

  public void AddCompilerDefinedBoundedIntLikeParameter<T>(string Name, T MinValue, T MaxValue)
    where T : notnull
  {
    var TypeName = typeof(T).FullName!;
    AddCompilerDefinedParameter(Name, GetBoundedIntLikeCodecName(TypeName, MinValue, MaxValue), null, TypeName);
  }
}