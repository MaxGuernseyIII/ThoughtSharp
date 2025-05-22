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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThoughtSharp.Generator;

static class CognitiveDataPipeline
{
  public static void Bind(IncrementalGeneratorInitializationContext Context)
  {
    RenderExplicitDataClasses(Context);
    RenderActionsClasses(Context);
  }

  static void RenderActionsClasses(IncrementalGeneratorInitializationContext Context)
  {
    var RawProvider = Context.SyntaxProvider.ForAttributeWithMetadataName(
      CognitiveDataAttributeNames.FullActionsAttribute,
      (Node, _) => Node is InterfaceDeclarationSyntax,
      (InnerContext, _) => InnerContext
    ).Select((C, _) =>
    {
      var NamedType = (INamedTypeSymbol) C.TargetSymbol;
      var TargetType = TypeAddress.ForSymbol(NamedType);
      var Methods = NamedType.GetMembers().OfType<IMethodSymbol>().Where(M =>
        M.ReturnsVoid || M.ReturnType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task");
      var CognitiveDataClasses = new List<CognitiveDataClass>();

      var CompleteDataTypeAddress = TargetType.GetNested(TypeIdentifier.Explicit("class", "__AllParameters"));
      var CompleteDataBuilder = new CognitiveDataClassBuilder(CompleteDataTypeAddress)
      {
        IsPublic = true
      };
      var InterpreterBuilder = new CognitiveDataInterpreterBuilder(TargetType);
      CompleteDataBuilder.AddCompilerDefinedParameter("__ActionCode", "new BitwiseOneHotNumberCodec<short>()",
        "CognitiveDataCodec<short>", null, "short");
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
        CompleteDataBuilder.AddCompilerDefinedParameter(Method.Name, $"new SubDataCodec<{MethodType.FullName}>()",
          $"CognitiveDataCodec<{MethodType.FullName}>", null, MethodType.FullName);
        InterpreterBuilder.AssociateMethodWithDataClass(Method.Name, ThisDataClass);
      }

      var CognitiveDataClass = CompleteDataBuilder.Build();
      CognitiveDataClasses.Add(CognitiveDataClass);
      InterpreterBuilder.ParametersClass = CognitiveDataClass;

      return (CognitiveDataClasses, CognitiveInterpreterClass: InterpreterBuilder.Build());
    });

    BindRenderingOfCognitiveDataClasses(Context, RawProvider.SelectMany((Items, _) => Items.CognitiveDataClasses));
    BindRenderingOfCognitiveDataInterpreters(Context,
      RawProvider.Select((Items, _) => Items.CognitiveInterpreterClass));
  }

  static void BindRenderingOfCognitiveDataInterpreters(
    IncrementalGeneratorInitializationContext Context,
    IncrementalValuesProvider<CognitiveDataInterpreter> Interpreters)
  {
    Context.RegisterSourceOutput(Interpreters, (Target, Interpreter) =>
    {
      using var StringWriter = new StringWriter();

      {
        using var Writer = new IndentedTextWriter(StringWriter, "  ");
        GeneratedTypeFormatter.GenerateType(Writer, new(Interpreter.DataClass.Address, W =>
        {
          W.WriteLine($"public Thought InterpretFor({Interpreter.ToInterpretType.FullName} ToInterpret)");
          W.WriteLine("{");
          W.Indent++;
          W.WriteLine("return Thought.Do(R =>");
          W.Indent++;
          W.WriteLine("{");
          W.Indent++;
          W.WriteLine("switch (__ActionCode)");
          W.WriteLine("{");
          W.Indent++;

          W.WriteLine("case 0: break;");
          var PathId = 1;
          foreach (var Path in Interpreter.Paths)
          {
            W.WriteLine($"case {PathId}:");
            W.Indent++;

            W.WriteLine($"ToInterpret.{Path.MethodName}({string.Join(", ", 
              Path.ParametersClass.Parameters.Select(P => $"{Path.MethodName}.{P.Name}"))});");

            W.WriteLine("break;");
            W.Indent--;

            PathId++;
          }

          W.WriteLine(@"default: throw new InvalidOperationException($""Unknown action code: {__ActionCode}"");");

          W.Indent--;
          W.WriteLine("}");
          W.Indent--;
          W.WriteLine("}");
          W.Indent--;
          W.WriteLine(");");
          W.Indent--;
          W.WriteLine("}");
        })
        {
          WriteHeader = W =>
          {
            W.WriteLine("using ThoughtSharp.Runtime;");
            W.WriteLine();
          }
        });
      }

      StringWriter.Close();

      Target.AddSource(
        GeneratedTypeFormatter.GetFilename(Interpreter.DataClass.Address, "__interpreters__"),
        StringWriter.GetStringBuilder().ToString());
    });
  }

  static void RenderExplicitDataClasses(IncrementalGeneratorInitializationContext Context)
  {
    BindRenderingOfCognitiveDataClasses(Context, GetExplicitCognitiveDataClasses(Context));
  }

  static IncrementalValuesProvider<CognitiveDataClass> GetExplicitCognitiveDataClasses(
    IncrementalGeneratorInitializationContext Context)
  {
    var FindCognitiveDataClasses = Context.SyntaxProvider.ForAttributeWithMetadataName(
      CognitiveDataAttributeNames.FullDataAttribute,
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