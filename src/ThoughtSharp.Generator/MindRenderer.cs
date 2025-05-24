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

        RenderStateCopyMethods(W, M);

        foreach (var MakeOperation in M.MakeOperations)
          RenderMakeMethod(W, MakeOperation, OperationCode++);
      }
    });
  }

  static void RenderStateCopyMethods(IndentedTextWriter W, MindModel Model)
  {
    W.WriteLine("void CopyStateTo(ref Input InputObject)");
    W.WriteLine("{");
    W.Indent++;
    foreach (var State in Model.States)
      W.WriteLine($"InputObject.Parameters.{State.Name}.Value = {State.Name};");
    W.Indent--;
    W.WriteLine("}");

    W.WriteLine("void CopyStateFrom(ref readonly Output OutputObject)");
    W.WriteLine("{");
    W.Indent++;
    foreach (var State in Model.States)
      W.WriteLine($"{State.Name} = OutputObject.Parameters.{State.Name}.Value;");
    W.Indent--;
    W.WriteLine("}");

    W.WriteLine("static readonly IReadOnlyList<Range> StateRanges = [");
    W.Indent++;
    foreach (var State in Model.States) 
      W.WriteLine($"(Output.ParametersIndex + Output.OutputParameters.{State.Name}Index)..(Output.ParametersIndex + Output.OutputParameters.{State.Name}Index + Output.OutputParameters.{State.Name}Parameters.Length)");
    W.WriteLine("];");
    W.Indent--;
  }

  static void RenderMakeMethod(IndentedTextWriter W, MindMakeOperationModel MakeOperation, ushort OperationCode)
  {
    W.WriteLine(
      $"public partial Thought<{MakeOperation.ReturnType}> {MakeOperation.Name}({string.Join(", ", MakeOperation.Parameters.Select(P => $"{P.Type} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("var InputObject = new Input();");
    W.WriteLine($"InputObject.OperationCode = {OperationCode.ToLiteralExpression()};");

    foreach (var Parameter in MakeOperation.Parameters)
      W.WriteLine($"InputObject.Parameters.{MakeOperation.Name}.{Parameter.Name} = {Parameter.Name};");

    W.WriteLine("CopyStateTo(ref InputObject);");
    W.WriteLine("var InputBuffer = new float[Input.Length];");
    W.WriteLine("InputObject.MarshalTo(InputBuffer);");
    W.WriteLine();
    W.WriteLine("var Inference = Brain.MakeInference(InputBuffer);");
    W.WriteLine("var OutputObject = Output.UnmarshalFrom(Inference.Result);");
    W.WriteLine("CopyStateFrom(ref OutputObject);");
    W.WriteLine();

    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{MakeOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{MakeOperation.Name}Parameters.Length;");

    W.WriteLine("var TrainingPolicy = new ApplyTrainingToInference(this, Inference, [OutputStart..OutputEnd], StateRanges);");

    W.WriteLine($"return Thought.Capture(OutputObject.Parameters.{MakeOperation.Name}.Value, TrainingPolicy);");
    W.Indent--;
    W.WriteLine("}");
  }
}