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
      .ToImmutableArray();

    foreach (var Member in ValueSymbols.Where(M => !M.IsStatic))
      Parameters.Add(new(Member.Name, TypeAddress.ForSymbol(Member.Type), GetExplicitLength(Member.Raw)));

    foreach (var Member in Symbol.GetMembers().Where(M => M.IsStatic).OfType<IPropertySymbol>())
      Codecs.Add(new(Member.Name));

    return new(TypeAddress.ForSymbol(Symbol), [..Parameters], [..Codecs]);
  }

  static int? GetExplicitLength(ISymbol Member)
  {
    foreach (var Attribute in Member.GetAttributes().Where(A => A.AttributeClass?.Name == ThoughtDataCountAttribute))
      if (Attribute.ConstructorArguments[0].Value is int Result)
        return Result;

    return null;
  }
}