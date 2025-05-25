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

namespace ThoughtSharp.Generator;

static class MindRenderer
{
  public static string Render(MindModel M)
  {
    return GeneratedTypeFormatter.GenerateType(new(M.TypeName)
    {
      WriteHeader = W =>
      {
        W.WriteLine("using ThoughtSharp.Runtime;");
        W.WriteLine();
      },
      WriteAfterTypeName = W => { W.Write("(Brain Brain) : Mind"); },
      WriteBody = W =>
      {
        ushort OperationCode = 1;

        RenderStateCopyMembers(W, M);

        foreach (var MakeOperation in M.MakeOperations)
          RenderMakeMethod(W, M, MakeOperation, OperationCode++);

        foreach (var UseOperation in M.UseOperations) 
          RenderUseMethod(W, M, UseOperation, OperationCode++);

        foreach (var ChooseOperation in M.ChooseOperations)
          RenderChooseMethod(W, M, ChooseOperation, OperationCode++);
      }
    });
  }

  static void RenderStateCopyMembers(IndentedTextWriter W, MindModel Model)
  {
    W.WriteLine("static readonly IReadOnlyList<Range> StateRanges = [");
    W.Indent++;
    foreach (var State in Model.States)
      W.WriteLine($"(Output.ParametersIndex + Output.OutputParameters.{State.Name}Index)..(Output.ParametersIndex + Output.OutputParameters.{State.Name}Index + Output.OutputParameters.{State.Name}Parameters.Length)");
    W.WriteLine("];");
    W.Indent--;
  }

  static void RenderMakeMethod(IndentedTextWriter W, MindModel Model, MindMakeOperationModel MakeOperation, ushort OperationCode)
  {
    W.WriteLine(
      $"public partial Thought<{MakeOperation.ReturnType}> {MakeOperation.Name}({string.Join(", ", MakeOperation.Parameters.Select(P => $"{P.Type} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    RenderInputObjectForOpCode(W, OperationCode);

    foreach (var Parameter in MakeOperation.Parameters)
      W.WriteLine($"InputObject.Parameters.{MakeOperation.Name}.{Parameter.Name} = {Parameter.Name};");

    RenderMakeInference(W, Model);
    W.WriteLine();

    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{MakeOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{MakeOperation.Name}Parameters.Length;");

    W.WriteLine("var TrainingPolicy = new ApplyTrainingToInference(this, Inference, [OutputStart..OutputEnd], StateRanges);");

    W.WriteLine($"return Thought.Capture(OutputObject.Parameters.{MakeOperation.Name}.Value, TrainingPolicy);");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderUseMethod(
    IndentedTextWriter W, 
    MindModel Model, 
    MindUseOperationModel UseOperation,
    ushort OperationCode)
  {
    var InputParameters = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is null);
    var ActionSurfaces = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is not null);

    var ReturnType = "Thought<bool>";
    var MethodIsAsync = ActionSurfaces.Any(A => A.AssociatedInterpreter!.RequiresAwait);
    if (MethodIsAsync)
      ReturnType = $"Task<{ReturnType}>";

    W.WriteLine($"public partial {ReturnType} {UseOperation.Name}({string.Join(", ", UseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    RenderInputObjectForOpCode(W, OperationCode);
    W.WriteLine();

    foreach (var Parameter in InputParameters)
      W.WriteLine($"InputObject.Parameters.{UseOperation.Name}.{Parameter.Name} = {Parameter.Name};");
    W.WriteLine();
    RenderMakeInference(W, Model);
    W.WriteLine();
    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{UseOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{UseOperation.Name}Parameters.Length;");
    W.WriteLine();

    W.WriteLine("var TrainingPolicy = new ApplyTrainingToInference(this, Inference, [OutputStart..OutputEnd], StateRanges);");
    var (ThoughtMethod, Async) = MethodIsAsync ? ("ThinkAsync", "async ") : ("Think", "");
    W.WriteLine($"return Thought.{ThoughtMethod}({Async}R =>");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("var MoreActions = false;");
    W.WriteLine();
    foreach (var Parameter in ActionSurfaces)
    {
      var Unwrap = Parameter.AssociatedInterpreter!.RequiresAwait ? "await " : "";
      W.WriteLine($"MoreActions = MoreActions || R.Consume({Unwrap}OutputObject.Parameters.{UseOperation.Name}.{Parameter.Name}.InterpretFor({Parameter.Name}));");
    }
    W.WriteLine();
    W.WriteLine("return MoreActions;");
    W.Indent--;
    W.WriteLine("}, TrainingPolicy);");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderChooseMethod(
    IndentedTextWriter W, 
    MindModel MindModel, 
    MindChooseOperationModel ChooseOperation,
    ushort OpCode)
  {
    W.WriteLine($"public partial {ChooseOperation.ReturnType} {ChooseOperation.Name}({string.Join(", ", ChooseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"return Thought.Think(R =>");
    W.WriteLine("{");
    W.Indent++;

    W.WriteLine("Output FinalOutputObject = default!;");

    W.WriteLine($"foreach (var Batch in {ChooseOperation.CategoryParameter}.ToInputBatches())");
    W.WriteLine("{");
    W.Indent++;
    RenderInputObjectForOpCode(W, OpCode);
    foreach (var Parameter in ChooseOperation.Parameters)
    {
      W.Write($"InputObject.Parameters.{ChooseOperation.Name}.{Parameter.Name} = ");
      W.Write(Parameter.Name == ChooseOperation.CategoryParameter ? 
        "Batch" : Parameter.Name);

      W.WriteLine(";");
    }

    W.WriteLine();
    RenderMakeInference(W, MindModel);
    W.WriteLine("FinalOutputObject = OutputObject;");
    W.WriteLine();
    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{ChooseOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{ChooseOperation.Name}Parameters.Length;");
    W.WriteLine();
    W.WriteLine("var TrainingPolicy = new ApplyTrainingToInference(this, Inference, [OutputStart..OutputEnd], StateRanges);");
    W.WriteLine("R.Incorporate(Thought.Done(TrainingPolicy));");
    W.WriteLine();
    W.Indent--;
    W.WriteLine("}");

    W.WriteLine($"return {ChooseOperation.CategoryParameter}.Interpret(FinalOutputObject.Parameters.{ChooseOperation.Name}.{ChooseOperation.CategoryParameter});");

    W.Indent--;
    W.WriteLine("});");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderInputObjectForOpCode(IndentedTextWriter W, ushort OperationCode)
  {
    W.WriteLine("var InputObject = new Input();");
    W.WriteLine($"InputObject.OperationCode = {OperationCode.ToLiteralExpression()};");
  }

  static void RenderMakeInference(IndentedTextWriter W, MindModel Model)
  {
    foreach (var State in Model.States)
      W.WriteLine($"InputObject.Parameters.{State.Name}.Value = {State.Name};");

    W.WriteLine("var InputBuffer = new float[Input.Length];");
    W.WriteLine("InputObject.MarshalTo(InputBuffer);");
    W.WriteLine();
    W.WriteLine("var Inference = Brain.MakeInference(InputBuffer);");
    W.WriteLine("var OutputObject = Output.UnmarshalFrom(Inference.Result);");

    foreach (var State in Model.States)
      W.WriteLine($"{State.Name} = OutputObject.Parameters.{State.Name}.Value;");
  }
}