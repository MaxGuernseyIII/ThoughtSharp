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

static class CognitiveCategoryModelFactory
{
  public static (CognitiveCategoryModel Category, ImmutableArray<CognitiveDataClass> AssociatedData)
    ConvertToCognitiveCategoryAndAssociatedData(
      GeneratorAttributeSyntaxContext C)
  {
    var Type = (INamedTypeSymbol) C.TargetSymbol;
    var (PayloadType, DescriptorType, Count) = Type.GetCognitiveCategoryData();
    var TypeName = TypeAddress.ForSymbol(Type);

    var DataObjects = new List<CognitiveDataClass>();
    var DescriptorTypeAddress = TypeAddress.ForSymbol(DescriptorType);
    var ItemClassName = "InputItem";
    var ItemType = TypeName.GetNested(TypeIdentifier.Explicit("class", ItemClassName));
    var ItemBuilder = new CognitiveDataClassBuilder(ItemType)
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    ItemBuilder.AddCompilerDefinedBoolParameter("IsHot");
    ItemBuilder.AddCompilerDefinedBoundedIntLikeParameter("ItemNumber", ushort.MinValue, ushort.MaxValue);
    ItemBuilder.AddCompilerDefinedSubDataParameter("Descriptor", DescriptorTypeAddress.FullName);
    DataObjects.Add(ItemBuilder.Build());
    var QuestionBuilder =
      new CognitiveDataClassBuilder(TypeName.GetNested(TypeIdentifier.Explicit("class", "Input")))
      {
        IsPublic = true,
        ExplicitConstructor = true
      };
    QuestionBuilder.AddCompilerDefinedSubDataArrayParameter("Items", ItemClassName, Count, "new()");
    QuestionBuilder.AddCompilerDefinedBoolParameter("IsFinalBatch");
    DataObjects.Add(QuestionBuilder.Build());

    var Answer =
      new CognitiveDataClassBuilder(TypeName.GetNested(TypeIdentifier.Explicit("class", "Output")))
      {
        IsPublic = true,
        ExplicitConstructor = true
      };
    Answer.AddCompilerDefinedBoundedIntLikeParameter("Selection", ushort.MinValue, ushort.MaxValue);
    DataObjects.Add(Answer.Build());

    return (Category: new(TypeName, TypeAddress.ForSymbol(PayloadType), DescriptorTypeAddress, Count),
      AssociatedData: [..DataObjects]);
  }
}