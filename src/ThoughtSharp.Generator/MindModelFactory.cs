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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ThoughtSharp.Generator;

static class MindModelFactory
{
  public static (MindModel Result, List<CognitiveDataClass> AssociatedDataTypes) MakeMindModel(
    GeneratorAttributeSyntaxContext C)
  {
    var Type = (INamedTypeSymbol) C.TargetSymbol;
    var PossibleGenerationTargets = Type.GetMembers().ToImmutableArray();
    var TypeName = TypeAddress.ForSymbol(Type);

    var MindModelBuilder = Generator.MindModelBuilder.Create(TypeName);

    foreach (var Member in PossibleGenerationTargets)
    {
      //if (!Debugger.IsAttached && Member.Name == "AsynchronousUseSomeInterface")
      //  Debugger.Launch();


      if (TryGetMakeMethod(Member, out var MakeMethod))
        MindModelBuilder.AddMakeMethodFor(MakeMethod);
      else if (TryGetUseMethod(Member, out var UseMethod))
        MindModelBuilder.AddUseMethodFor(UseMethod);
      else if (TryGetChooseMethod(Member, out var ChooseMethod))
        MindModelBuilder.AddChooseMethodFor(ChooseMethod);
    }

    var Result = MindModelBuilder.Build();
    return (Result, MindModelBuilder.AssociatedDataTypes);
  }

  static bool TryGetUseMethod(ISymbol S, [NotNullWhen(true)] out IMethodSymbol? Result)
  {
    if (
      S is IMethodSymbol {IsPartialDefinition: true, IsStatic: false, DeclaredAccessibility: Accessibility.Public} M &&
      M.HasAttribute(CognitiveAttributeNames.UseAttributeName) &&
      CognitiveActionRules.IsValidCognitiveResult(M))
    {
      Result = M;
      return true;
    }

    Result = null;
    return false;
  }

  static bool TryGetMakeMethod(ISymbol S, [NotNullWhen(true)] out IMethodSymbol? Method)
  {
    if (
      S is IMethodSymbol {IsPartialDefinition: true, IsStatic: false, DeclaredAccessibility: Accessibility.Public} M &&
      M.HasAttribute(CognitiveAttributeNames.MakeAttributeName) &&
      M.ReturnType.IsCognitiveResultOf(_ => true, _ => true))
    {
      Method = M;
      return true;
    }

    Method = null;
    return false;
  }

  static bool TryGetChooseMethod(ISymbol S, [NotNullWhen(true)] out IMethodSymbol? Method)
  {
    if (
      S is IMethodSymbol {IsPartialDefinition: true, IsStatic: false, DeclaredAccessibility: Accessibility.Public} M &&
      M.HasAttribute(CognitiveAttributeNames.ChooseAttributeName)
    )
    {
      var CognitiveCategories =
        M.Parameters.Select(P => (Hit: P.Type.TryGetCognitiveCategoryData(out var CategoryData), CategoryData))
          .Where(P => P.Hit);
      if (CognitiveCategories.Count() == 1)
      {
        var PayloadType = CognitiveCategories.Single().CategoryData!.Value.PayloadType;
        if (M.ReturnType.IsCognitiveResultOf(T => T.IsEquivalentTo(PayloadType), T => T.IsEquivalentTo(PayloadType)))
        {
          Method = M;
          return true;
        }
      }
    }

    Method = null;
    return false;
  }
}