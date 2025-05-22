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
using Microsoft.CodeAnalysis.CSharp;

namespace ThoughtSharp.Generator;

static class SymbolExtensions
{
  public static IValueSymbol? ToValueSymbolOrDefault(this ISymbol ToAdapt)
  {

    if (ToAdapt is IFieldSymbol Field)
      return new FieldValueSymbol(Field);

    if (ToAdapt is IPropertySymbol Property)
      return new PropertyValueSymbolAdapter(Property);

    if (ToAdapt is IParameterSymbol Parameter)
      return new ParameterValueSymbolAdapter(Parameter);

    return null;
  }

  class ParameterValueSymbolAdapter(IParameterSymbol Parameter) : IValueSymbol
  {
    public ISymbol Raw => Parameter;

    public string Name => Parameter.Name;

    public ITypeSymbol Type => Parameter.Type;

    public bool IsStatic => false;

    public bool IsImplicitlyDeclared => false;
  }

  class FieldValueSymbol(IFieldSymbol Field) : IValueSymbol
  {
    public string Name => Field.Name;

    public ITypeSymbol Type => Field.Type;

    public bool IsStatic => Field.IsStatic;

    public bool IsImplicitlyDeclared => Field.IsImplicitlyDeclared;

    public ISymbol Raw => Field;
  }

  class PropertyValueSymbolAdapter(IPropertySymbol Property) : IValueSymbol
  {
    public string Name => Property.Name;

    public ITypeSymbol Type => Property.Type;

    public bool IsStatic => Property.IsStatic;

    public bool IsImplicitlyDeclared => Property.IsImplicitlyDeclared;

    public ISymbol Raw => Property;
  }

  public static string ToLiteralExpression(this object? Value)
  {
    var Expression = Value switch
    {
      null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
      bool B => SyntaxFactory.LiteralExpression(
        B ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
      short S => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(S)),
      ushort S => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(S)),
      int I => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(I)),
      uint I => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(I)),
      long L => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(L)),
      ulong L => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(L)),
      float F => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(F)),
      double D => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(D)),
      decimal M => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(M)),
      byte B => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(B)),
      sbyte B => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(B)),
      string S => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(S)),
      char C => SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(C)),
      Enum E => SyntaxFactory.ParseExpression(E.GetType().FullName + "." + E),
      _ => throw new NotSupportedException($"Literal generation not supported for type: {Value.GetType()}")
    };
    return Expression.NormalizeWhitespace().ToFullString();
  }
}