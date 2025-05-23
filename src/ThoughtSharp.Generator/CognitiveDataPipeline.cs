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
    RenderExplicitDataClasses(Context);
    RenderActionsClasses(Context);
    RenderCategories(Context);
  }

  static void RenderCategories(IncrementalGeneratorInitializationContext Context)
  {
    var RawSource = Context.SyntaxProvider.ForAttributeWithMetadataName(CognitiveAttributeNames.FullCategoryAttribute,
      (Node, _) => Node is TypeDeclarationSyntax,
      (C, _) => CognitiveCategoryModelFactory.ConvertToCognitiveCategoryAndAssociatedData(C));
    BindRenderingOfCognitiveCategories(Context, RawSource.Select((Pair, _) => Pair.Category));
    BindRenderingOfCognitiveDataClasses(Context, RawSource.SelectMany((Pair, _) => Pair.AssociatedData));
  }

  static void BindRenderingOfCognitiveCategories(IncrementalGeneratorInitializationContext Context, IncrementalValuesProvider<CognitiveCategoryModel> ModelsProvider)
  {
    Context.RegisterSourceOutput(ModelsProvider, (C, M) =>
    {
      C.AddSource(
        GeneratedTypeFormatter.GetFilename(M.CategoryType), CognitiveCategoryRenderer.GenerateCategoryType(M)
      );
    });
  }

  static void RenderExplicitDataClasses(IncrementalGeneratorInitializationContext Context)
  {
    BindRenderingOfCognitiveDataClasses(Context, GetExplicitCognitiveDataClasses(Context));
  }

  static void RenderActionsClasses(IncrementalGeneratorInitializationContext Context)
  {
    var RawProvider = Context.SyntaxProvider.ForAttributeWithMetadataName(
        CognitiveAttributeNames.FullActionsAttribute,
        (Node, _) => Node is InterfaceDeclarationSyntax,
        (InnerContext, _) => InnerContext
      ).Select((C, _) => CognitiveActionsModelFactory.MakeModelsForCognitiveActions(C));

    BindRenderingOfCognitiveDataClasses(Context, RawProvider.SelectMany((Items, _) => Items.CognitiveDataClasses));
    BindRenderingOfCognitiveDataInterpreters(Context,
      RawProvider.Select((Items, _) => Items.CognitiveInterpreterClass));
  }

  static IncrementalValuesProvider<CognitiveDataClass> GetExplicitCognitiveDataClasses(
    IncrementalGeneratorInitializationContext Context)
  {
    var FindCognitiveDataClasses = Context.SyntaxProvider.ForAttributeWithMetadataName(
      CognitiveAttributeNames.FullDataAttribute,
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

  static void BindRenderingOfCognitiveDataInterpreters(
    IncrementalGeneratorInitializationContext Context,
    IncrementalValuesProvider<CognitiveDataInterpreter> Interpreters)
  {
    Context.RegisterSourceOutput(Interpreters, (Target, Interpreter) =>
    {
      Target.AddSource(
        GeneratedTypeFormatter.GetFilename(Interpreter.DataClass.Address, "__interpreters__"),
        CognitiveDataInterpreterRenderer.RenderSourceFor(Interpreter));
    });
  }
}