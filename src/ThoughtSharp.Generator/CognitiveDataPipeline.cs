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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThoughtSharp.Generator;

static class CognitiveDataPipeline
{
  public static void Bind(IncrementalGeneratorInitializationContext Context)
  {
    BindRenderingOfCognitiveDataClasses(Context, GetExplicitCognitiveDataClasses(Context));
    BindRenderingOfCognitiveDataClasses(Context, GetCognitiveDataClassesImpliedInActions(Context));
  }

  static IncrementalValuesProvider<CognitiveDataClass> GetCognitiveDataClassesImpliedInActions(IncrementalGeneratorInitializationContext Context)
  {
    return Context.SyntaxProvider.ForAttributeWithMetadataName(
      CognitiveDataAttributeNames.FullActionsAttribute,
      (Node, _) => Node is InterfaceDeclarationSyntax,
      (InnerContext, _) => InnerContext
    ).SelectMany((C, _) =>
    {
      var NamedType = (INamedTypeSymbol)C.TargetSymbol;
      var TargetType = TypeAddress.ForSymbol(NamedType);
      var Methods = NamedType.GetMembers().OfType<IMethodSymbol>().Where(M => M.ReturnsVoid || M.ReturnType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task");
      var Results = new List<CognitiveDataClass>();

      foreach (var Method in Methods)
      {
        var MethodType = TargetType.GetNested(TypeIdentifier.Explicit("class", Method.Name + "Parameters"));
        var Builder = new CognitiveDataClassBuilder(MethodType)
        {
          IsPublic = true
        };

        foreach (var Parameter in Method.Parameters.Select(P => P.ToValueSymbolOrDefault()!)) 
          Builder.AddParameterValue(Parameter, true);
      }

      return Results;
    });
  }

  static IncrementalValuesProvider<CognitiveDataClass> GetExplicitCognitiveDataClasses(IncrementalGeneratorInitializationContext Context)
  {
    var FindCognitiveDataClasses = Context.SyntaxProvider.ForAttributeWithMetadataName(CognitiveDataAttributeNames.FullDataAttribute,
      (Node, _) => Node is TypeDeclarationSyntax,
      (InnerContext, _) => CognitiveDataClassModelFactory.ConvertDataClassToModel(InnerContext));

    
    return FindCognitiveDataClasses;
  }

  static void BindRenderingOfCognitiveDataClasses(
    IncrementalGeneratorInitializationContext Context,
    IncrementalValuesProvider<CognitiveDataClass> Pipeline)
  {
    Context.RegisterSourceOutput(
      Pipeline,
      (InnerContext, CognitiveDataClass) =>
      {
        InnerContext.AddSource(
          GeneratedTypeFormatter.GetFilename(CognitiveDataClass.Address),
          CognitiveDataClassRenderer.RenderCognitiveDataClass(CognitiveDataClass));
      });
  }
}