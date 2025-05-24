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
using System.Xml.Linq;

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
    return T.ToDisplayString(new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters)).Trim();
  }

  public static bool IsThoughtTypeWithPayload(this ITypeSymbol Type)
  {
    var FullPath = Type.GetFullPath();
    return FullPath.StartsWith("ThoughtSharp.Runtime.Thought<");
  }

  public static bool IsThoughtType(this ITypeSymbol Type)
  {
    return IsType(Type, "ThoughtSharp.Runtime.Thought");
  }

  public static bool IsTaskType(this ITypeSymbol Type)
  {
    return IsType(Type, "System.Threading.Tasks.Task");
  }

  public static bool IsTaskOfThoughtType(this ITypeSymbol Type)
  {
    return IsType(Type, "System.Threading.Tasks.Task<ThoughtSharp.Runtime.Thought>");
  }

  public static bool RequiresAwait(this ITypeSymbol Type) => Type.IsTaskType() || Type.IsTaskOfThoughtType();
  public static bool IsThoughtful(this ITypeSymbol Type) => Type.IsThoughtType() || Type.IsTaskOfThoughtType();

  static bool IsType(ITypeSymbol Type, string Name)
  {
    return Type.GetFullPath() == Name;
  }

  public static int? GetExplicitCount(this ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes()
               .Where(A => A.AttributeClass?.Name == CognitiveAttributeNames.DataCountAttributeName))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }
}