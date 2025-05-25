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

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ThoughtSharp.Generator;

class TypeIdentifier
{
  TypeIdentifier(string Keyword, string Name, IReadOnlyList<TypeAddress> TypeParameters)
  {
    this.Keyword = Keyword;
    this.Name = Name;
    FullName = ComputeFullName(TypeParameters, Name);
    this.TypeParameters = TypeParameters;
  }

  public string Keyword { get; }
  public string Name { get; }
  public IReadOnlyList<TypeAddress> TypeParameters { get; }

  public string FullName { get; }

  static string ComputeFullName(IReadOnlyList<TypeAddress> IReadOnlyList, string Value)
  {
    var ResultBuilder = new StringBuilder();

    ResultBuilder.Append(Value);

    var Delimiter = "<";
    var Terminator = "";

    foreach (var TypeParameter in IReadOnlyList)
    {
      ResultBuilder.Append(Delimiter);
      ResultBuilder.Append(TypeParameter.FullName);

      Terminator = ">";
      Delimiter = ", ";
    }

    ResultBuilder.Append(Terminator);

    return ResultBuilder.ToString();
  }

  public static TypeIdentifier ForSymbol(ITypeSymbol Symbol)
  {
    var Arguments = Symbol is INamedTypeSymbol Named
      ? Named.TypeArguments.Select(TypeAddress.ForSymbol).ToImmutableList()
      : [];

    return new(GetTypeKeyword(Symbol), Symbol.Name, Arguments);
  }

  public static TypeIdentifier Explicit(
    string Keyword, string Name, params TypeAddress[] TypeParameters)
  {
    return new(Keyword, Name, TypeParameters);
  }

  static string GetTypeKeyword(ITypeSymbol Symbol)
  {
    return Symbol.TypeKind switch
    {
      TypeKind.Class when Symbol.IsRecord => "record",
      TypeKind.Class => "class",
      TypeKind.Struct when Symbol.IsRecord => "record struct",
      TypeKind.Struct => "struct",
      TypeKind.Interface => "interface",
      TypeKind.Enum => "enum",
      TypeKind.Delegate => "delegate",
      _ => "unknown"
    };
  }
}