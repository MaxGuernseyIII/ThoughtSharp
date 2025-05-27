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
      WriteAfterTypeName = W => { W.Write($"(Brain Brain) : Mind"); },
      WriteBody = W =>
      {
        ushort OperationCode = 1;

        WriteInterfaceMembers(W, M);
        WriteCognitionModeMembers(W, M);
        RenderDisposeMembers(W);

        RenderModelMembers(W, M, OperationCode);
      }
    });
  }

  static void WriteInterfaceMembers(IndentedTextWriter W, MindModel M)
  {
    W.WriteLine($"public static int InputLength {{ get; }} = { M.TypeName.FullName}.Input.Length;");
    W.WriteLine($"public static int OutputLength {{ get; }} = { M.TypeName.FullName}.Output.Length;");
    W.WriteLine();
  }

  static void WriteCognitionModeMembers(IndentedTextWriter W, MindModel M)
  {
    W.WriteLine($"public struct ChainedReasoningSession({M.TypeName.FullName} Host) : IDisposable");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("public void Dispose()");
    W.WriteLine("{");
    W.Indent++;

    W.WriteLine("Host.CognitionMode = Host.CognitionMode.ExitChainedLineOfReasoning();");
    W.Indent--;
    W.WriteLine("}");
    W.Indent--;
    W.WriteLine("}");
    W.WriteLine();

    W.WriteLine("CognitionMode CognitionMode = new IsolatedCognitionMode(Brain);");
    W.WriteLine();
    W.WriteLine("public ChainedReasoningSession EnterChainedReasoning()");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("CognitionMode = CognitionMode.EnterChainedLineOfReasoning();");
    W.WriteLine("return new ChainedReasoningSession(this);");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderModelMembers(IndentedTextWriter W, MindModel M, ushort OperationCode)
  {
    foreach (var MakeOperation in M.MakeOperations)
      RenderMakeMethod(W, M, MakeOperation, OperationCode++);

    foreach (var UseOperation in M.UseOperations)
      RenderUseMethod(W, M, UseOperation, OperationCode++);

    foreach (var ChooseOperation in M.ChooseOperations)
      RenderChooseMethod(W, M, ChooseOperation, OperationCode++);
  }

  static void RenderDisposeMembers(IndentedTextWriter W)
  {
    W.WriteLine("public void Dispose()");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("Brain.Dispose();");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderMakeMethod(IndentedTextWriter W, MindModel Model, MindMakeOperationModel MakeOperation,
    ushort OperationCode)
  {
    W.WriteLine(
      $"public partial Thought<{MakeOperation.ReturnType}, MakeFeedback<{MakeOperation.ReturnType}>> {MakeOperation.Name}({string.Join(", ", MakeOperation.Parameters.Select(P => $"{P.Type} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    RenderInputObjectForOpCode(W, OperationCode);

    foreach (var Parameter in MakeOperation.Parameters)
      W.WriteLine($"InputObject.Parameters.{MakeOperation.Name}.{Parameter.Name} = {Parameter.Name};");

    RenderMakeInference(W, Model);
    W.WriteLine();

    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{MakeOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{MakeOperation.Name}Parameters.Length;");

    W.WriteLine("return Thought.Capture(");
    W.Indent++;
    W.WriteLine($"OutputObject.Parameters.{MakeOperation.Name}.Value,");
    W.WriteLine($"new MakeFeedback<{MakeOperation.ReturnType}>(");
    W.Indent++;
    W.WriteLine("new(");
    W.Indent++;
    W.WriteLine("Inference");
    W.Indent--;
    W.WriteLine("),");
    W.WriteLine("V => ");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("var O = new Output");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"Parameters = {{ {MakeOperation.Name} = {{ Value = V }} }}");
    W.Indent--;
    W.WriteLine("};");
    W.WriteLine();
    W.WriteLine("var Buffer = new float[Output.Length];");
    W.WriteLine("O.MarshalTo(Buffer);");
    W.WriteLine("return Buffer;");
    W.Indent--;
    W.WriteLine("}");
    W.Indent--;
    W.WriteLine(")");
    W.Indent--;
    W.WriteLine(");");
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
    var ActionSurface = ActionSurfaces.Single();

    var MethodIsAsync = ActionSurface.AssociatedInterpreter!.RequiresAwait;
    var FeedbackType = $"UseFeedback<{ActionSurface.TypeName}>";
    if (MethodIsAsync)
      FeedbackType = "Async" + FeedbackType;

    var ReturnType = $"Thought<bool, {FeedbackType}>";
    if (MethodIsAsync)
      ReturnType = $"Task<{ReturnType}>";

    W.WriteLine();
    W.WriteLine($"class {UseOperation.Name}FeedbackMock(Inference ToTrain) : {ActionSurface.TypeName}");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("bool Conditioned;");
    W.WriteLine("Output Expected = new();");
    W.WriteLine();

    ushort ActionCode = 1;
    foreach (var Path in ActionSurface.AssociatedInterpreter!.Paths)
    {
      W.WriteLine($"public {Path.ReturnType} {Path.MethodName}({string.Join(", ", Path.ParametersClass.Parameters.Select(P => $"{P.FullType} {P.Name}"))})");
      W.WriteLine("{");
      W.Indent++;
      W.WriteLine("if (Conditioned) throw new InvalidOperationException(\"Cannot condition a training mock twice.\");");
      W.WriteLine("Conditioned = true;");
      W.WriteLine($"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.ActionCode = {ActionCode.ToLiteralExpression()};");

      foreach (var Parameter in Path.ParametersClass.Parameters) 
        W.WriteLine($"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.Parameters.{Path.MethodName}.{Parameter.Name} = {Parameter.Name};");

      var ReturnValue = Path.IsThoughtful? "Thought.Done()" : "";
      if (Path.RequiresAwait)
        ReturnValue = $"Task.FromResult({ReturnValue})";

      if (ReturnValue.Length > 0)
        ReturnValue = " " + ReturnValue;
      if (Path.IsThoughtful)
        W.WriteLine($"return{ReturnValue};");

      W.Indent--;
      W.WriteLine("}");
      W.WriteLine();
      ActionCode++;
    }

    W.WriteLine("public void Commit(bool ExpectedMore)");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.MoreActions = ExpectedMore;");
    W.WriteLine("var Buffer = new float[Output.Length];");
    W.WriteLine("Expected.MarshalTo(Buffer);");
    W.WriteLine("ToTrain.Train(Buffer);");
    W.Indent--;
    W.WriteLine("}");
    W.Indent--;
    W.WriteLine("}");
    W.WriteLine();

    W.WriteLine(
      $"public partial {ReturnType} {UseOperation.Name}({string.Join(", ", UseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
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

    var (ThoughtMethod, Async) = MethodIsAsync ? ("ThinkAsync", "async ") : ("Think", "");
    W.WriteLine($"return Thought.WithFeedback<{FeedbackType}>.{ThoughtMethod}({Async}R =>");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"var FeedbackMock = new {UseOperation.Name}FeedbackMock(Inference);");
    W.WriteLine($"var Feedback = new {FeedbackType}(");
    W.Indent++;
    W.WriteLine($"FeedbackMock,");
    W.WriteLine("FeedbackMock.Commit");
    W.Indent--;
    W.WriteLine(");");

    W.WriteLine("R.SetFeedback(Feedback);");
    W.WriteLine();
    var Unwrap = ActionSurface.AssociatedInterpreter!.RequiresAwait ? "await " : "";
    W.WriteLine(
      $"var MoreActions = R.Consume({Unwrap}OutputObject.Parameters.{UseOperation.Name}.{ActionSurface.Name}.InterpretFor({ActionSurface.Name}));");

    W.WriteLine();
    W.WriteLine("return (");
    W.Indent++;
    W.WriteLine("MoreActions,");
    W.WriteLine("Feedback");
    W.Indent--;
    W.WriteLine(");");
    W.Indent--;
    W.WriteLine("});");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderChooseMethod(
    IndentedTextWriter W,
    MindModel MindModel,
    MindChooseOperationModel ChooseOperation,
    ushort OpCode)
  {
    W.WriteLine(
      $"public partial {ChooseOperation.ReturnType} {ChooseOperation.Name}({string.Join(", ", ChooseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("return Thought.Think(R =>");
    W.WriteLine("{");
    W.Indent++;

    W.WriteLine("using var _ = EnterChainedReasoning();");

    W.WriteLine("Output FinalOutputObject = default!;");
    W.WriteLine("Inference FinalInference = default!;");

    W.WriteLine($"foreach (var Batch in {ChooseOperation.CategoryParameter}.ToInputBatches())");
    W.WriteLine("{");
    W.Indent++;
    RenderInputObjectForOpCode(W, OpCode);
    foreach (var Parameter in ChooseOperation.Parameters)
    {
      W.Write($"InputObject.Parameters.{ChooseOperation.Name}.{Parameter.Name} = ");
      W.Write(Parameter.Name == ChooseOperation.CategoryParameter ? "Batch" : Parameter.Name);

      W.WriteLine(";");
    }

    W.WriteLine();
    RenderMakeInference(W, MindModel);
    W.WriteLine("FinalOutputObject = OutputObject;");
    W.WriteLine("FinalInference = Inference;");
    W.WriteLine();
    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{ChooseOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{ChooseOperation.Name}Parameters.Length;");
    W.WriteLine();
    W.Indent--;
    W.WriteLine("}");

    W.WriteLine("return (");
    W.Indent++;
    W.WriteLine(
      $"{ChooseOperation.CategoryParameter}.Interpret(FinalOutputObject.Parameters.{ChooseOperation.Name}.{ChooseOperation.CategoryParameter}),");
    W.WriteLine($"ChooseFeedback<{ChooseOperation.SelectableTypeName}>.Get(");
    W.Indent++;
    W.WriteLine("new(FinalInference),");
    W.WriteLine($"[..{ChooseOperation.CategoryParameter}.AllOptions.Select(O => O.Payload)],");
    W.WriteLine(
      $"I => new Output {{ Parameters = {{ {ChooseOperation.Name} = {{ {ChooseOperation.CategoryParameter} = {{ Selection = I }} }} }} }}");
    W.Indent--;
    W.WriteLine(")");
    W.Indent--;
    W.WriteLine(");");

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
    W.WriteLine("var InputBuffer = new float[Input.Length];");
    W.WriteLine("InputObject.MarshalTo(InputBuffer);");
    W.WriteLine();
    W.WriteLine("var Inference = CognitionMode.CurrentInferenceSource.MakeInference(InputBuffer);");
    W.WriteLine("var OutputObject = Output.UnmarshalFrom(Inference.Result);");
    W.WriteLine("CognitionMode = CognitionMode.RegisterNewInference(Inference);");
  }
}