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

static class IndentedWriterExtensions
{
  class IndentCleanup(IndentedTextWriter W, string FinalLine) : IDisposable
  {
    public void Dispose()
    {
      W.Indent--;
      W.WriteLine(FinalLine);
    }
  }

  public static IDisposable DeclareWithBlock(this IndentedTextWriter W, params IEnumerable<string> Declarations)
  {
    foreach (var Declaration in Declarations) 
      W.WriteLine(Declaration);

    return W.EnterBlock();
  }

  public static IDisposable EnterBlock(this IndentedTextWriter W, string StartOfLine = "", string Suffix = "")
  {
    return Bounded(W, $"{StartOfLine}{{", $"}}{Suffix}");
  }

  public static IDisposable EnterMultilineParens(this IndentedTextWriter W, string StartOfLine = "", string Suffix = "")
  {
    return Bounded(W, $"{StartOfLine}(", $"){Suffix}");
  }

  static IDisposable Bounded(IndentedTextWriter W, string Open, string Close)
  {
    W.WriteLine(Open);
    W.Indent++;
    return new IndentCleanup(W, Close);
  }
}