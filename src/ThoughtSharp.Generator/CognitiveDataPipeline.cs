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

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics;
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

  private static void RenderCategories(IncrementalGeneratorInitializationContext Context)
  {
    var RawSource = Context.SyntaxProvider.ForAttributeWithMetadataName(CognitiveAttributeNames.FullCategoryAttribute,
      (Node, _) => Node is TypeDeclarationSyntax,
      (C, _) =>
      {
        var Type = (INamedTypeSymbol)C.TargetSymbol;
        var Attribute = Type.GetAttributes()
          .First(A => A.AttributeClass?.Name == CognitiveAttributeNames.CategoryAttributeName);
        var PayloadType = Attribute.AttributeClass!.TypeArguments[0];
        var DescriptorType = Attribute.AttributeClass!.TypeArguments[1];
        var Count = Convert.ToUInt16(Attribute.ConstructorArguments[0]);
        var TypeName = TypeAddress.ForSymbol(Type);

        var DataObjects = new List<CognitiveDataClass>();
        var DescriptorTypeAddress = TypeAddress.ForSymbol(DescriptorType);
        var ItemType = TypeName.GetNested(TypeIdentifier.Explicit("class", "Item"));
        var ItemBuilder = new CognitiveDataClassBuilder(ItemType);
        ItemBuilder.AddCompilerDefinedParameter("IsHot", "new CopyBoolCodec()", null, "bool");
        ItemBuilder.AddCompilerDefinedParameter("Descriptor", $"new SubDataCodec<{DescriptorTypeAddress.FullName}>()", null, DescriptorTypeAddress.FullName);
        DataObjects.Add(ItemBuilder.Build());
        var QuestionBuilder =
          new CognitiveDataClassBuilder(TypeName.GetNested(TypeIdentifier.Explicit("class", "Question")));
        QuestionBuilder.AddCompilerDefinedParameter("Items", "new SubDataCodec<Item>()", Count, "Item");
        QuestionBuilder.AddCompilerDefinedParameter("IsLastBatch", "new CopyBoolCodec()", null, "bool");
        DataObjects.Add(QuestionBuilder.Build());

        var Answer =
          new CognitiveDataClassBuilder(TypeName.GetNested(TypeIdentifier.Explicit("class", "Answer")));
        Answer.AddCompilerDefinedParameter("Selection", CognitiveDataClassBuilder.GetBoundedIntLikeCodecName("ushort", ushort.MinValue, ushort.MaxValue), null, "ushort");
        DataObjects.Add(Answer.Build());

        return (new CognitiveCategoryModel(TypeName, TypeAddress.ForSymbol(PayloadType), DescriptorTypeAddress, Count), DataObjects.ToImmutableArray());
      }
    );
    Context.RegisterSourceOutput(RawSource.Select((Pair, _) => Pair.Item1), (C, M) =>
    {
      C.AddSource(
        GeneratedTypeFormatter.GetFilename(M.CategoryType),
        GenerateCategoryType(M)
        );
    });
    BindRenderingOfCognitiveDataClasses(Context, RawSource.SelectMany((Pair, _) => Pair.Item2));
  }

  private static string GenerateCategoryType(CognitiveCategoryModel Model)
  {
    var TypeParameters = $"<{Model.PayloadType.FullName}, {Model.DescriptorType.FullName}>";
    using var StringWriter = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(StringWriter, "  ");

      GeneratedTypeFormatter.GenerateType(Writer, new(Model.CategoryType, W =>
      {
        W.WriteLine($"public static int EncodeLength = {Model.DescriptorType.FullName}.Length");
      })
      {
        WriteHeader = W =>
        {
          W.WriteLine("using ThoughtSharp.Runtime;");
          W.WriteLine();
        },
        WriteAfterTypeName = W =>
        {
          W.WriteLine($"(IReadOnlyList<CognitiveOption{TypeParameters}> Options)");
          W.Write($"  : CognitiveCategory{TypeParameters}");
        }
      });
    }

    StringWriter.Close();

    return StringWriter.ToString();
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