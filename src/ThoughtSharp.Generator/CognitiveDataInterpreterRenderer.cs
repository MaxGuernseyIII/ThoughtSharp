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

static class CognitiveDataInterpreterRenderer
{
  public static string RenderSourceFor(CognitiveDataInterpreter Interpreter)
  {
    using var StringWriter = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(StringWriter, "  ");
      GeneratedTypeFormatter.GenerateType(Writer, new(Interpreter.DataClass.Address)
      {
        WriteBody = W =>
        {
          var MethodIsAsync = Interpreter.RequiresAwait;
          var ReturnValue = MethodIsAsync ? "Task<Thought<bool>>" : "Thought<bool>";

          W.WriteLine($"public {ReturnValue} InterpretFor({Interpreter.ToInterpretType.FullName} ToInterpret)");
          W.WriteLine("{");
          W.Indent++;
          W.WriteLine("var ActionCode = this.ActionCode;");
          W.WriteLine("var MoreActions = this.MoreActions;");
          W.WriteLine("var Parameters = this.Parameters;");
          W.WriteLine();
          if (MethodIsAsync)
            W.WriteLine("return Thought.ThinkAsync(async R =>");
          else
            W.WriteLine("return Thought.Think(R =>");
          W.Indent++;
          W.WriteLine("{");
          W.Indent++;
          W.WriteLine("switch (ActionCode)");
          W.WriteLine("{");
          W.Indent++;

          W.WriteLine("case 0: break;");
          var PathId = 1;
          foreach (var Path in Interpreter.Paths)
          {
            W.WriteLine($"case {PathId}:");
            W.Indent++;

            if (Path.IsThoughtful)
              W.Write("R.Incorporate(");

            if (Path.RequiresAwait)
              W.Write("await ");

            W.Write($"ToInterpret.{Path.MethodName}({string.Join(", ",
              Path.ParametersClass.Parameters.Select(P => $"Parameters.{Path.MethodName}.{P.Name}"))})");
            if (Path.IsThoughtful)
              W.Write(")");

            W.WriteLine(";");

            W.WriteLine("break;");
            W.Indent--;

            PathId++;
          }

          W.WriteLine(@"default: throw new InvalidOperationException($""Unknown action code: {ActionCode}"");");

          W.Indent--;
          W.WriteLine("}");
          W.WriteLine();
          W.WriteLine("return MoreActions;");
          W.Indent--;
          W.WriteLine("}");
          W.Indent--;
          W.WriteLine(");");
          W.Indent--;
          W.WriteLine("}");
        },
        WriteHeader = W =>
        {
          W.WriteLine("using ThoughtSharp.Runtime;");
          W.WriteLine();
        }
      });
    }

    StringWriter.Close();

    var Source = StringWriter.GetStringBuilder().ToString();
    return Source;
  }
}