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

using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ThoughtSharp.Generator;

class TypeIdentifier
{
  TypeIdentifier(string Keyword, string Name)
  {
    this.Keyword = Keyword;
    this.Name = Name;
  }

  public string Keyword { get; }
  public string Name { get; }

  public static TypeIdentifier ForSyntaxNode(TypeDeclarationSyntax Node)
  {
    return new(Node.Keyword.Text, Node.Identifier.Text);
  }
}

class TypeAddress
{
  TypeAddress(
    IReadOnlyList<string> ContainingNamespaces,
    IReadOnlyList<TypeIdentifier> ContainingTypes,
    TypeIdentifier TypeName)
  {
    this.ContainingNamespaces = ContainingNamespaces;
    this.ContainingTypes = ContainingTypes;
    this.TypeName = TypeName;
  }

  public IReadOnlyList<string> ContainingNamespaces { get; }
  public IReadOnlyList<TypeIdentifier> ContainingTypes { get; }
  public TypeIdentifier TypeName { get; }

  public static TypeAddress ForSyntaxNode(TypeDeclarationSyntax Node)
  {
    var ContainingNamespaces = new List<string>();
    var ContainingTypes = new List<TypeIdentifier>();
    var TypeName = TypeIdentifier.ForSyntaxNode(Node);

    var Ancestor = Node.Parent;

    while (Ancestor != null)
    {
      switch (Ancestor)
      {
        case TypeDeclarationSyntax AncestorType:
          ContainingTypes.Add(TypeIdentifier.ForSyntaxNode(AncestorType));
          break;

        case NamespaceDeclarationSyntax AncestorNamespace:
          ContainingNamespaces.Add(AncestorNamespace.Name.ToString());
          break;

        case FileScopedNamespaceDeclarationSyntax AncestorNamespace:
          ContainingNamespaces.Add(AncestorNamespace.Name.ToString());
          break;
      }

      Ancestor = Ancestor.Parent;
    }

    return new(ContainingNamespaces, ContainingTypes, TypeName);
  }
}

static class GeneratedTypeFormatter
{
  public static string GetFilename(TypeAddress Address)
  {
    return "thought/" + string.Join(".",
      Address.ContainingNamespaces.SelectMany(N => N.Split('.'))
        .Concat(Address.ContainingTypes.Select(T => T.Name)
          .Concat([Address.TypeName.Name]))) + ".g.cs";
  }

  public static string FrameInPartialType(TypeAddress Address, string Content)
  {
    var Lines = Content.Split(["\r\n", "\n"], StringSplitOptions.None);
    var OutputBuilder = new StringBuilder(5 * Content.Length);
    var IndentLevel = 0;

    foreach (var Namespace in Address.ContainingNamespaces)
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}namespace {Namespace}");
      OutputBuilder.AppendLine($"{Indent}{{");
      IndentLevel += 1;
    }

    foreach (var Type in Address.ContainingTypes.Concat([Address.TypeName]))
    {
      var Indent = GetIndentForLevel(IndentLevel);
      OutputBuilder.AppendLine($"{Indent}partial {Type.Keyword} {Type.Name}");
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

class ThoughtDataObject(
  TypeAddress Address)
{
  public TypeAddress Address { get; } = Address;
}

[Generator]
public class ThoughtSharpGenerator : IIncrementalGenerator
{
  const string ThoughtDataAttribute = "ThoughtSharp.Runtime.ThoughtDataAttribute";

  public void Initialize(IncrementalGeneratorInitializationContext Context)
  {
    var ThoughtDataPipeline = Context.SyntaxProvider.ForAttributeWithMetadataName(
      ThoughtDataAttribute,
      (Node, _) => Node is TypeDeclarationSyntax,
      (InnerContext, _) =>
      {
        var TargetType = (TypeDeclarationSyntax) InnerContext.TargetNode;
        return new ThoughtDataObject(TypeAddress.ForSyntaxNode(TargetType));
      });

    Context.RegisterSourceOutput(
      ThoughtDataPipeline,
      (InnerContext, ThoughtDataObject) =>
      {
        InnerContext.AddSource(
          GeneratedTypeFormatter.GetFilename(ThoughtDataObject.Address),
          GeneratedTypeFormatter.FrameInPartialType(ThoughtDataObject.Address, "public const int Length = 0;"));
      });

    Context.RegisterPostInitializationOutput(ctx =>
    {
      ctx.AddSource("__debug.g.cs", SourceText.From(@"
// Generator Ran at " + DateTime.UtcNow.ToString("O"),
        Encoding.UTF8));
    });
  }
}