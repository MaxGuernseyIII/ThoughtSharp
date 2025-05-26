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
    InputBuilder.SetCompilerDefinedBoundedOpcodeParameter("OperationCode", 0);
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
    UseOperations = [];
    ChooseOperations = [];
  }

  public TypeAddress TypeName { get; }
  public List<CognitiveDataClass> AssociatedDataTypes { get; }
  CognitiveDataClassBuilder InputBuilder { get; }
  CognitiveDataClassBuilder InputParametersBuilder { get; }
  CognitiveDataClassBuilder OutputBuilder { get; }
  CognitiveDataClassBuilder OutputParametersBuilder { get; }
  List<MindMakeOperationModel> MakeOperations { get; }
  List<MindUseOperationModel> UseOperations { get; }
  List<MindChooseOperationModel> ChooseOperations { get; }

  public static MindModelBuilder Create(TypeAddress TypeName)
  {
    return new(TypeName);
  }

  public MindModel Build()
  {
    UpdateOperationCode();
    AssociatedDataTypes.Add(InputBuilder.Build());
    AssociatedDataTypes.Add(InputParametersBuilder.Build());
    AssociatedDataTypes.Add(OutputBuilder.Build());
    AssociatedDataTypes.Add(OutputParametersBuilder.Build());

    return new(TypeName, [..MakeOperations], [.. UseOperations], [..ChooseOperations]);
  }

  void UpdateOperationCode()
  {
    InputBuilder.SetCompilerDefinedBoundedOpcodeParameter("OperationCode",
      (ushort) (MakeOperations.Count + UseOperations.Count + ChooseOperations.Count));
  }

  public void AddMakeMethodFor(IMethodSymbol MakeMethod)
  {
    var ThisInputDataModel = MakeMethod.GetParametersDataModel(GetInputParametersClassName(MakeMethod));
    AssociatedDataTypes.Add(ThisInputDataModel);
    InputParametersBuilder.AddCompilerDefinedSubDataParameter(MakeMethod.Name, ThisInputDataModel.Address.FullName);
    var ProductType = ((INamedTypeSymbol) MakeMethod.ReturnType).TypeArguments[0];
    var ThisOutputModelBuilder = new CognitiveDataClassBuilder(GetOutputParametersClassName(MakeMethod))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    OutputParametersBuilder.AddCompilerDefinedSubDataParameter(MakeMethod.Name, ThisOutputModelBuilder);
    ThisOutputModelBuilder.AddCompilerDefinedSubDataParameter("Value", ProductType.GetFullPath());
    AssociatedDataTypes.Add(ThisOutputModelBuilder.Build());

    MakeOperations.Add(new(MakeMethod.Name, ProductType.GetFullPath(),
      [..MakeMethod.Parameters.Select(P => (P.Name, P.Type.GetFullPath()))]));
  }

  public void AddUseMethodFor(IMethodSymbol UseMethod)
  {
    var ThisInputDataModel = UseMethod.GetParametersDataModel(GetInputParametersClassName(UseMethod),
      (Parameter, _) => IsActionSurfaceParameter(Parameter));
    AssociatedDataTypes.Add(ThisInputDataModel);
    InputParametersBuilder.AddCompilerDefinedSubDataParameter(UseMethod.Name, ThisInputDataModel.Address.FullName);

    var ThisOutputModelBuilder = new CognitiveDataClassBuilder(GetOutputParametersClassName(UseMethod))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    OutputParametersBuilder.AddCompilerDefinedSubDataParameter(UseMethod.Name, ThisOutputModelBuilder);
    foreach (var ActionSurface in UseMethod.Parameters.Where(IsActionSurfaceParameter))
      ThisOutputModelBuilder.AddCompilerDefinedSubDataParameter(ActionSurface.Name,
        ActionSurface.Type.GetFullPath() + ".Output");
    AssociatedDataTypes.Add(ThisOutputModelBuilder.Build());

    UseOperations.Add(new(UseMethod.Name, [
      ..UseMethod.Parameters.Select(P => (
        P.Name,
        P.Type.GetFullPath(),
        IsActionSurfaceParameter(P)
          ? CognitiveActionsModelFactory.ConvertToInterpreter((INamedTypeSymbol) P.Type).CognitiveInterpreterClass
          : null))
    ]));

    static bool IsActionSurfaceParameter(IParameterSymbol Parameter)
    {
      return Parameter.Type.HasAttribute(CognitiveAttributeNames.ActionsAttributeName);
    }
  }

  public void AddChooseMethodFor(IMethodSymbol ChooseMethod)
  {
    IParameterSymbol CategoryParameter = null!;

    var ThisInputDataModel = ChooseMethod.GetParametersDataModel(GetInputParametersClassName(ChooseMethod),
      (Parameter, Builder) =>
      {
        if (!IsCategoryParameter(Parameter))
          return false;

        CategoryParameter = Parameter;

        Builder.AddCompilerDefinedSubDataParameter(Parameter.Name, Parameter.Type.GetFullPath() + ".Input");

        return true;
      });
    AssociatedDataTypes.Add(ThisInputDataModel);
    InputParametersBuilder.AddCompilerDefinedSubDataParameter(ChooseMethod.Name, ThisInputDataModel.Address.FullName);
    var CategoryData = CategoryParameter.Type.GetCognitiveCategoryData();

    var ThisOutputModelBuilder = new CognitiveDataClassBuilder(GetOutputParametersClassName(ChooseMethod))
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    OutputParametersBuilder.AddCompilerDefinedSubDataParameter(ChooseMethod.Name, ThisOutputModelBuilder);
    ThisOutputModelBuilder.AddCompilerDefinedSubDataParameter(CategoryParameter.Name,
      CategoryParameter.Type.GetFullPath() + ".Output");
    AssociatedDataTypes.Add(ThisOutputModelBuilder.Build());

    ChooseOperations.Add(new(
      ChooseMethod.Name,
      ChooseMethod.ReturnType.GetFullPath(),
      [..ChooseMethod.Parameters.Select(P => (P.Name, P.Type.GetFullPath()))],
      CategoryParameter.Name,
      CategoryData.PayloadType.GetFullPath()
    ));

    static bool IsCategoryParameter(IParameterSymbol Parameter)
    {
      return Parameter.Type.HasAttribute(CognitiveAttributeNames.CategoryAttributeName);
    }
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