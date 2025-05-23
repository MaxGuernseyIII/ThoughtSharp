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

static class CognitiveActionsModelFactory
{
  public static (List<CognitiveDataClass> CognitiveDataClasses, CognitiveDataInterpreter CognitiveInterpreterClass) MakeModelsForCognitiveActions(
    GeneratorAttributeSyntaxContext C)
  {
    var NamedType = (INamedTypeSymbol) C.TargetSymbol;
    var TargetType = TypeAddress.ForSymbol(NamedType);
    var Methods = NamedType.GetMembers().OfType<IMethodSymbol>().Where(IsValidThoughtAction);
    var CognitiveDataClasses = new List<CognitiveDataClass>();

    var CompleteDataTypeAddress = TargetType.GetNested(TypeIdentifier.Explicit("class", "__AllParameters"));
    var CompleteDataBuilder = new CognitiveDataClassBuilder(CompleteDataTypeAddress)
    {
      IsPublic = true
    };
    var InterpreterBuilder = new CognitiveDataInterpreterBuilder(TargetType);
    CompleteDataBuilder.AddCompilerDefinedBoundedIntLikeParameter("__ActionCode", ushort.MinValue, ushort.MaxValue);
    CompleteDataBuilder.AddCompilerDefinedBoolParameter("__MoreActions");
    foreach (var Method in Methods)
    {
      var MethodType = TargetType.GetNested(TypeIdentifier.Explicit("class", $"{Method.Name}Parameters"));
      var ThisDataClassBuilder = new CognitiveDataClassBuilder(MethodType)
      {
        IsPublic = true
      };

      foreach (var Parameter in Method.Parameters.Select(P => P.ToValueSymbolOrDefault()!))
        ThisDataClassBuilder.AddParameterValue(Parameter, true);

      var ThisDataClass = ThisDataClassBuilder.Build();
      CognitiveDataClasses.Add(ThisDataClass);
      CompleteDataBuilder.AddCompilerDefinedSubDataParameter(Method.Name, MethodType.FullName);
      InterpreterBuilder.AssociateMethodWithDataClass(Method, ThisDataClass);
    }

    var CognitiveDataClass = CompleteDataBuilder.Build();
    CognitiveDataClasses.Add(CognitiveDataClass);
    InterpreterBuilder.ParametersClass = CognitiveDataClass;

    return (CognitiveDataClasses, CognitiveInterpreterClass: InterpreterBuilder.Build());
  }

  static bool IsValidThoughtAction(IMethodSymbol M)
  {
    if (M.ReturnsVoid)
      return true;

    if (M.ReturnType.IsThoughtType())
      return true;

    if (M.ReturnType.IsTaskType())
      return true;

    if (M.ReturnType.IsTaskOfThoughtType())
      return true;

    return false;
  }
}