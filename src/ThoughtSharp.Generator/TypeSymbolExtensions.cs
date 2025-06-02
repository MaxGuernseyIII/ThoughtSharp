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

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace ThoughtSharp.Generator;

public static class TypeSymbolExtensions
{
  //public static bool IsCognitiveData(this ITypeSymbol T)
  //{
  //  if (T.GetAttributes().Any(A => A.AttributeClass?.Name == CognitiveAttributeNames.))
  //    return true;

  //  if (T.Name == "CognitiveData")
  //    return true;

  //  if (T.BaseType?.IsCognitiveData() ?? false)
  //    return true;

  //  if (T.AllInterfaces.Any(I => I.IsCognitiveData()))
  //    return true;

  //  return false;
  //}

  public static string GetFullPath(this ITypeSymbol T)
  {
    if (T.SpecialType == SpecialType.System_Void)
      return "void";

    return T.ToDisplayString(new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters)).Trim();
  }

  public static string GetFullPathWithoutTypeParameters(this ITypeSymbol T)
  {
    return T.ToDisplayString(new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
      genericsOptions: SymbolDisplayGenericsOptions.None)).Trim();
  }

  public static bool IsThoughtTypeWithPayload(this ITypeSymbol Type)
  {
    return Type.IsGenericOf("ThoughtSharp.Runtime.Thought", _ => true, _ => true);
  }

  public static bool IsThoughtType(this ITypeSymbol Type)
  {
    return IsType(Type, "ThoughtSharp.Runtime.Thought");
  }

  public static bool IsCognitiveResultOf(this ITypeSymbol Type, params Func<ITypeSymbol, bool>[] Arguments)
  {
    return IsGenericOf(Type, "ThoughtSharp.Runtime.CognitiveResult", Arguments);
  }

  public static bool IsTaskType(this ITypeSymbol Type)
  {
    return IsType(Type, "System.Threading.Tasks.Task");
  }

  public static bool IsTaskOfThoughtType(this ITypeSymbol Type)
  {
    return Type.IsTaskOf(IsThoughtType);
  }

  public static bool IsBooleanType(this ITypeSymbol Type)
  {
    return Type.SpecialType == SpecialType.System_Boolean;
  }

  public static bool IsThoughtOf(this ITypeSymbol Type, Func<ITypeSymbol, bool> ArgumentConstraint, Func<ITypeSymbol, bool> FeedbackConstraint)
  {
    return Type.IsGenericOf("ThoughtSharp.Runtime.Thought", ArgumentConstraint, FeedbackConstraint);
  }

  public static bool IsGenericOf(this ITypeSymbol Type, string Name, params Func<ITypeSymbol, bool>[] ArgumentConstraints)
  {
    if (Type is not INamedTypeSymbol Named)
      return false;

    var ExpectedLength = ArgumentConstraints.Length;
    if (Named.TypeArguments.Length != ExpectedLength)
      return false;

    for (var I = 0; I < ExpectedLength; ++I)
      if (!ArgumentConstraints[I](Named.TypeArguments[I]))
        return false;

    return true;
  }

  public static bool IsThoughtOfUseReturnType(this ITypeSymbol Type, IMethodSymbol Method)
  {
    var ActionSurfaceTypes = Method.Parameters.Select(P => P.Type)
      .Where(T => T.HasAttribute(CognitiveAttributeNames.ActionsAttributeName));

    if (ActionSurfaceTypes.Take(2).Count() != 1)
      return false;

    var ActionSurfaceType = ActionSurfaceTypes.Single();

    return Type.IsGenericOf("ThoughtSharp.Runtime.Thought", IsBooleanType, T => T.IsGenericOf("UseFeedback", Inner => IsType(Inner, ActionSurfaceType.GetFullPath())));
  }

  public static bool IsTaskOf(this ITypeSymbol Type, Func<ITypeSymbol, bool> PayloadRequirement)
  {
    return Type.IsGenericOf("System.Threading.Tasks.Task", PayloadRequirement);
  }

  public static bool RequiresAwait(this ITypeSymbol Type)
  {
    return Type.IsTaskType() || Type.IsTaskOfThoughtType();
  }

  public static bool IsThoughtful(this ITypeSymbol Type)
  {
    return Type.IsThoughtType() || Type.IsTaskOfThoughtType();
  }

  static bool IsType(ITypeSymbol Type, string Name)
  {
    return Type.GetFullPath() == Name;
  }

  public static bool IsEquivalentTo(this ITypeSymbol Type, ITypeSymbol Other)
  {
    return Type.GetFullPath() == Other.GetFullPath();
  }

  public static int? GetExplicitCount(this ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataCountAttributeName))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }

  public static (ITypeSymbol PayloadType, ITypeSymbol DescriptorType, ushort Count) GetCognitiveCategoryData(
    this ITypeSymbol Type)
  {
    return ConvertAttributeToCognitiveCategoryData(Type.GetAttributes()
      .First(IsCognitiveCategoryAttribute));
  }

  static (ITypeSymbol PayloadType, ITypeSymbol DescriptorType, ushort Count) ConvertAttributeToCognitiveCategoryData(
    AttributeData Attribute)
  {
    var PayloadType = Attribute.AttributeClass!.TypeArguments[0];
    var DescriptorType = Attribute.AttributeClass!.TypeArguments[1];
    var Count = Convert.ToUInt16(Attribute.ConstructorArguments[0].Value);
    return (PayloadType, DescriptorType, Count);
  }

  public static bool TryGetCognitiveCategoryData(
    this ITypeSymbol Type,
    out (ITypeSymbol PayloadType, ITypeSymbol DescriptorType, ushort Count)? Result)
  {
    var Attribute = Type.GetAttributes().FirstOrDefault(IsCognitiveCategoryAttribute);

    if (Attribute != null)
    {
      Result = ConvertAttributeToCognitiveCategoryData(Attribute);
      return true;
    }

    Result = null;
    return false;
  }

  static bool IsCognitiveCategoryAttribute(AttributeData A)
  {
    return A.AttributeClass?.Name == CognitiveAttributeNames.CategoryAttributeName;
  }
}