﻿// MIT License
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

static class CognitiveActionsModelFactory
{
  public static (List<CognitiveDataClass> CognitiveDataClasses, CognitiveDataInterpreter CognitiveInterpreterClass)
    MakeModelsForCognitiveActions(
      GeneratorAttributeSyntaxContext C)
  {
    return ConvertToInterpreter((INamedTypeSymbol) C.TargetSymbol);
  }

  public static (List<CognitiveDataClass> CognitiveDataClasses, CognitiveDataInterpreter CognitiveInterpreterClass)
    ConvertToInterpreter(
      INamedTypeSymbol NamedType)
  {
    var TargetType = TypeAddress.ForSymbol(NamedType);
    var Methods = NamedType.GetMembers().OfType<IMethodSymbol>().Where(CognitiveActionRules.IsValidCognitiveAction);
    var CognitiveDataClasses = new List<CognitiveDataClass>();

    var CompleteDataTypeAddress = TargetType.GetNested(TypeIdentifier.Explicit("struct", "Output"));
    var CompleteDataBuilder = new CognitiveDataClassBuilder(CompleteDataTypeAddress)
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    UpdateActionCode(CompleteDataBuilder, Methods);
    var OutputParametersTypeAddress =
      CompleteDataTypeAddress.GetNested(TypeIdentifier.Explicit("struct", "OutputParameters"));
    var CompleteParametersDataBuilder = new CognitiveDataClassBuilder(OutputParametersTypeAddress)
    {
      IsPublic = true,
      ExplicitConstructor = true
    };
    var InterpreterBuilder = new CognitiveDataInterpreterBuilder(TargetType);
    CompleteDataBuilder.AddCompilerDefinedBoolParameter("MoreActions");
    CompleteDataBuilder.AddCompilerDefinedSubDataParameter("Parameters", "OutputParameters");
    foreach (var Method in Methods)
    {
      var MethodType = TargetType.GetNested(TypeIdentifier.Explicit("struct", $"{Method.Name}Parameters"));
      var ThisDataClass = Method.GetParametersDataModel(MethodType);
      CompleteParametersDataBuilder.AddCompilerDefinedSubDataParameter(Method.Name, MethodType.FullName);
      CognitiveDataClasses.Add(ThisDataClass);
      InterpreterBuilder.AssociateMethodWithDataClass(Method, ThisDataClass);
    }

    var CognitiveDataClass = CompleteDataBuilder.Build();
    CognitiveDataClasses.Add(CognitiveDataClass);
    InterpreterBuilder.ParametersClass = CognitiveDataClass;
    CognitiveDataClasses.Add(CompleteParametersDataBuilder.Build());

    return (CognitiveDataClasses, CognitiveInterpreterClass: InterpreterBuilder.Build());
  }

  static void UpdateActionCode(CognitiveDataClassBuilder CompleteDataBuilder, IEnumerable<IMethodSymbol> Methods)
  {
    CompleteDataBuilder.SetCompilerDefinedBoundedOpcodeParameter("ActionCode", (ushort)Methods.Count());
  }
}