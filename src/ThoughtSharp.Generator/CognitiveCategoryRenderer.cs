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

static class CognitiveCategoryRenderer
{
  public static string GenerateCategoryType(CognitiveCategoryModel Model)
  {
    var RenderingOperation = new RenderingOperation(Model);
    using var StringWriter = new StringWriter();

    {
      using var Writer = new IndentedTextWriter(StringWriter, "  ");

      GeneratedTypeFormatter.GenerateType(Writer, new(Model.CategoryType)
      {
        WriteBody = RenderingOperation.GenerateBody,
        WriteHeader = RenderingOperation.GenerateHeader,
        WriteAfterTypeName = RenderingOperation.AdornTypeName
      });
    }

    StringWriter.Close();

    return StringWriter.ToString();
  }

  class RenderingOperation(CognitiveCategoryModel Model)
  {
    CognitiveCategoryModel Model { get; } = Model;
    string TypeParameters { get; } = $"<{Model.PayloadType.FullName}, {Model.DescriptorType.FullName}>";

    public void GenerateBody(IndentedTextWriter W)
    {
      GenerateAllOptionsProperty(W);
      GenerateToInputBatchesMethod(W);
      GenerateInterpretMethod(W);
    }

    void GenerateAllOptionsProperty(IndentedTextWriter W)
    {
      W.WriteLine($"public IReadOnlyList<CognitiveOption{TypeParameters}> AllOptions {{ get; }} = Options;");
      W.WriteLine();
    }

    void GenerateToInputBatchesMethod(IndentedTextWriter W)
    {
      MethodHeader();
      DefineVariables();
      Spacer();
      BatchesGenerationLoop();
      Spacer();
      SetLastBatch();
      MethodFooter();

      void MethodHeader()
      {
        W.WriteLine("public IReadOnlyList<Input> ToInputBatches()");
        Open();
      }

      void SetLastBatch()
      {
        W.WriteLine("if (Batches.Any())");
        Open();
        W.WriteLine("var LastBatch = Batches[^1];");
        W.WriteLine("LastBatch.IsFinalBatch = true;");
        W.WriteLine("Batches[^1] = LastBatch;");
        Close();
      }

      void MethodFooter()
      {
        Spacer();
        W.WriteLine("return Batches;");
        Close();
      }

      void Spacer()
      {
        W.WriteLine();
      }

      void Open()
      {
        W.WriteLine("{");
        W.Indent++;
      }

      void Close(string Suffix = "")
      {
        W.Indent--;
        W.Write("}");
        W.WriteLine(Suffix);
      }

      void DefineVariables()
      {
        W.WriteLine("var Batches = new List<Input>();");
        W.WriteLine("ushort CurrentIndex = 0;");
      }

      void BatchesGenerationLoop()
      {
        W.WriteLine("while(AllOptions.Count > CurrentIndex)");
        Open();
        W.WriteLine("var Batch = new Input();");
        SingleBatchGenerationLoop();
        W.WriteLine("Batches.Add(Batch);");
        Close();
      }

      void SingleBatchGenerationLoop()
      {
        W.WriteLine($"for (ushort I = 0; I < {Model.Count.ToLiteralExpression()}; ++I, ++CurrentIndex)");
        Open();
        W.WriteLine("if (CurrentIndex < AllOptions.Count)");
        Open();
        W.WriteLine("Batch.Items[I] = new()");
        Open();
        W.WriteLine("IsHot = true,");
        W.WriteLine("ItemNumber = CurrentIndex,");
        W.WriteLine("Descriptor = AllOptions[CurrentIndex].Descriptor");
        Close(";");
        Close();
        W.WriteLine("else");
        Open();
        W.WriteLine("Batch.Items[I] = new()");
        Open();
        W.WriteLine("IsHot = false,");
        W.WriteLine("ItemNumber = 0,");
        W.WriteLine("Descriptor = new()");
        Close(";");
        Close();
        Close();
      }
    }

    void GenerateInterpretMethod(IndentedTextWriter W)
    {
      W.WriteLine($"public {Model.PayloadType.FullName} Interpret(Output O)");
      W.WriteLine("{");
      W.Indent++;
      W.WriteLine("return AllOptions[O.Selection % AllOptions.Count].Payload;");
      W.Indent--;
      W.WriteLine("}");
    }

    public void GenerateHeader(IndentedTextWriter W)
    {
      W.WriteLine("using ThoughtSharp.Runtime;");
      W.WriteLine();
    }

    public void AdornTypeName(IndentedTextWriter W)
    {
      W.WriteLine($"(IReadOnlyList<CognitiveOption{TypeParameters}> Options)");
      W.Write($"  : CognitiveCategory{TypeParameters}");
    }
  }
}