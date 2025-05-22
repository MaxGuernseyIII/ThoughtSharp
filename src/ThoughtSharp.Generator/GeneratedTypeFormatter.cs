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

namespace ThoughtSharp.Generator;

static class GeneratedTypeFormatter
{
  public static string GetFilename(TypeAddress Address, string Suffix = "")
  {
    return string.Join(".",
      Address.ContainingNamespaces.SelectMany(N => N.Split('.'))
        .Concat(Address.ContainingTypes.Select(T => T.Name)
          .Concat([Address.TypeName.Name]))) + Suffix + ".g.cs";
  }

  public class TypeGenerationRequest(TypeAddress Type, Action<IndentedTextWriter> WriteBody)
  {
    public TypeAddress Type { get; } = Type;
    public Action<IndentedTextWriter> WriteHeader { get; set; } = Ignore;
    public Action<IndentedTextWriter> WriteBeforeTypeDeclaration { get; set; } = Ignore;
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

    Request.WriteBeforeTypeDeclaration(Target);
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
}