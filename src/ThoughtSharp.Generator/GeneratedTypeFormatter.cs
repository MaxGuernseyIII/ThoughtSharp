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
using System.Net;
using System.Text;

namespace ThoughtSharp.Generator;

static class GeneratedTypeFormatter
{
  public static string GetFilename(TypeAddress Address)
  {
    return string.Join(".",
      Address.ContainingNamespaces.SelectMany(N => N.Split('.'))
        .Concat(Address.ContainingTypes.Select(T => T.Name)
          .Concat([Address.TypeName.Name]))) + ".g.cs";
  }

  public class TypeGenerationRequest(TypeAddress Type, Action<IndentedTextWriter> WriteBody)
  {
    public TypeAddress Type { get; } = Type;
    public Action<IndentedTextWriter> WriteHeader { get; set; } = Ignore;
    public Action<IndentedTextWriter> WriteAfterTypeName { get; set; } = Ignore;
    public Action<IndentedTextWriter> WriteBody { get; } = WriteBody;

    static void Ignore(IndentedTextWriter _) {}
  }

  public static void GenerateType(IndentedTextWriter Target, TypeGenerationRequest Request)
  {
    Request.WriteHeader(Target);

    foreach (var Namespace in Request.Type.ContainingNamespaces)
    {
      Target.WriteLine($"namespace {Namespace}");
      Target.WriteLine("{");
      Target.Indent++;
    }

    foreach (var Type in Request.Type.ContainingTypes)
    {
      Target.WriteLine($"partial {Type.Keyword} {Type.Name}");
      Target.WriteLine("{");
      Target.Indent++;
    }

    Target.Write($"partial {Request.Type.TypeName.Keyword} {Request.Type.TypeName.Name}");
    Request.WriteAfterTypeName(Target);
    Target.WriteLine();
    Target.WriteLine("{");

    Target.Indent++;

    Request.WriteBody(Target);

    foreach (var _ in Request.Type.ContainingTypes.Concat([Request.Type.TypeName]))
    {
      Target.Indent--;
      Target.WriteLine("}");
    }

    foreach (var _ in Request.Type.ContainingNamespaces)
    {
      Target.Indent--;
      Target.WriteLine("}");
    }
  }

  public static string FrameInPartialType(TypeAddress Address, string Content, string TypeSuffix = "")
  {
    var Lines = Content.Split(["\r\n", "\n"], StringSplitOptions.None);
    var OutputBuilder = new StringBuilder(5 * Content.Length);
    OutputBuilder.AppendLine("using ThoughtSharp.Runtime;");
    OutputBuilder.AppendLine("using ThoughtSharp.Runtime.Codecs;");
    OutputBuilder.AppendLine();

    var IndentLevel = 0;

    foreach (var Namespace in Address.ContainingNamespaces)
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}namespace {Namespace}");
      OutputBuilder.AppendLine($"{Indent}{{");
      IndentLevel += 1;
    }

    foreach (var Type in Address.ContainingTypes)
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}partial {Type.Keyword} {Type.Name}");
      OutputBuilder.AppendLine($"{Indent}{{");

      IndentLevel += 1;
    }

    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}partial {Address.TypeName.Keyword} {Address.TypeName.Name}{TypeSuffix}");
      OutputBuilder.AppendLine($"{Indent}{{");

      IndentLevel += 1;
    }

    {
      var Indent = GetIndentForLevel(IndentLevel);

      foreach (var Line in Lines)
      {
        OutputBuilder.Append(Indent);
        OutputBuilder.AppendLine(Line);
      }
    }

    foreach (var _ in Address.ContainingTypes.Concat([Address.TypeName]))
    {
      IndentLevel -= 1;
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}}}");
    }

    foreach (var _ in Address.ContainingNamespaces)
    {
      IndentLevel -= 1;
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}}}");
    }

    return OutputBuilder.ToString();
  }

  static string GetIndentForLevel(int Level)
  {
    return new(' ', 2 * Level);
  }
}