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

  public TypeAddress GetNested(TypeIdentifier NestedType)
  {
    return new(ContainingNamespaces, [..ContainingTypes, TypeName], NestedType);
  }

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