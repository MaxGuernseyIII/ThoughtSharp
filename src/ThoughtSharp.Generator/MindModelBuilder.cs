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

class MindModelBuilder
{
  MindModelBuilder(TypeAddress TypeName)
  {
    this.TypeName = TypeName;
    AssociatedDataTypes = [];
    InputBuilder = new(TypeName.GetNested(TypeIdentifier.Explicit("struct", "Input")))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    InputBuilder.AddCompilerDefinedBoundedIntLikeParameter("OperationCode", ushort.MinValue, ushort.MaxValue);
    InputParametersBuilder = new(
      InputBuilder.TypeAddress.GetNested(TypeIdentifier.Explicit("struct", "InputParameters")))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    InputBuilder.AddCompilerDefinedSubDataParameter("Parameters", InputParametersBuilder);
    OutputBuilder = new(TypeName.GetNested(TypeIdentifier.Explicit("struct", "Output")))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    OutputParametersBuilder = new(
      new CognitiveDataClassBuilder(TypeName.GetNested(TypeIdentifier.Explicit("struct", "Output")))
      {
        IsPublic = true,
        ExplicitConstructor = true
      }.TypeAddress.GetNested(TypeIdentifier.Explicit("struct", "OutputParameters")))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    OutputBuilder.AddCompilerDefinedSubDataParameter("Parameters", OutputParametersBuilder);
    MakeOperations = [];
    StateModels = [];
  }

  public TypeAddress TypeName { get; }
  public List<CognitiveDataClass> AssociatedDataTypes { get; }
  CognitiveDataClassBuilder InputBuilder { get; }
  CognitiveDataClassBuilder InputParametersBuilder { get; }
  CognitiveDataClassBuilder OutputBuilder { get; }
  CognitiveDataClassBuilder OutputParametersBuilder { get; }
  List<MindMakeOperationModel> MakeOperations { get; }
  List<MindStateModel> StateModels { get; }

  public static MindModelBuilder Create(TypeAddress TypeName)
  {
    return new(TypeName);
  }

  public MindModel Build()
  {
    AssociatedDataTypes.Add(InputBuilder.Build());
    AssociatedDataTypes.Add(InputParametersBuilder.Build());
    AssociatedDataTypes.Add(OutputBuilder.Build());
    AssociatedDataTypes.Add(OutputParametersBuilder.Build());

    return new(TypeName, [..MakeOperations], [..StateModels]);
  }

  public void AddMakeMethodFor(IMethodSymbol MakeMethod)
  {
    var ThisDataModel = MakeMethod.GetParametersDataModel(GetInputParametersClassName(MakeMethod));
    AssociatedDataTypes.Add(ThisDataModel);
    InputParametersBuilder.AddCompilerDefinedSubDataParameter(MakeMethod.Name, ThisDataModel.Address.FullName);
    var ProductType = ((INamedTypeSymbol) MakeMethod.ReturnType).TypeArguments[0];
    OutputParametersBuilder.AddCompilerDefinedSubDataParameter(MakeMethod.Name, ProductType.GetFullPath());

    MakeOperations.Add(new(MakeMethod.Name, ProductType.GetFullPath(),
      [..MakeMethod.Parameters.Select(P => (P.Name, P.Type.GetFullPath()))]));
  }

  public void AddStateValueFor(IValueSymbol StateValue)
  {
    var TypeName = GetInputParametersClassName(StateValue.Raw);
    var ThisDataModel = new CognitiveDataClassBuilder(TypeName)
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    ThisDataModel.AddParameterValue(StateValue, true, "Value");
    AssociatedDataTypes.Add(ThisDataModel.Build());
    InputParametersBuilder.AddCompilerDefinedSubDataParameter(StateValue.Raw.Name, ThisDataModel);
    StateModels.Add(new(StateValue.Name));
  }

  TypeAddress GetInputParametersClassName(ISymbol S)
  {
    return GetParametersClassName(S, InputParametersBuilder);
  }

  TypeAddress GetOutputParametersClassName(ISymbol S)
  {
    return GetParametersClassName(S, OutputParametersBuilder);
  }

  static TypeAddress GetParametersClassName(ISymbol S, CognitiveDataClassBuilder Parent)
  {
    return Parent.TypeAddress.GetNested(
      TypeIdentifier.Explicit("struct", $"{S.Name}Parameters"));
  }
}