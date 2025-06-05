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
using System.CommandLine;
using System.Reflection;
using ThoughtSharp.Scenarios.Model;

var RootCommand = new RootCommand("Train models with ThoughtSharp");

var AssemblyArgument = new Argument<FileInfo>
{
  Name = "assembly",
  Description = "The .net assembly to train",
  Arity = ArgumentArity.ExactlyOne
};
  
AssemblyArgument.AddValidator(R =>
{
  var File = R.GetValueOrDefault<FileInfo?>();

  if (File is null)
    R.ErrorMessage = "The assembly argument is required";
  else if (!File.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
    R.ErrorMessage = "The specified assembly must be a .net assembly (with an extension '.dll')";
  else if (!File.Exists)
    R.ErrorMessage = "The specified assembly does not exist";
});

RootCommand.AddArgument(AssemblyArgument);

RootCommand.SetHandler(ToTrain =>
{
  Console.WriteLine($"Training {ToTrain.FullName}:");
  var Assembly = System.Reflection.Assembly.LoadFrom(ToTrain.FullName);
  var Parser = new AssemblyParser();

  var Model = Parser.Parse(Assembly);
  Console.WriteLine(Model.Query(new Scanner()));
}, AssemblyArgument);

Environment.ExitCode = RootCommand.Invoke(args);

class Scanner : ScenariosModelNodeVisitor<string>
{
  string Scan(Action<IndentedTextWriter> Describe, IEnumerable<ScenariosModelNode> ChildNodes)
  {
    using StringWriter Core = new();
    using IndentedTextWriter Writer = new IndentedTextWriter(Core, "  ");

    Describe(Writer);

    Writer.Indent++;

    foreach (var Child in ChildNodes) 
      Writer.Write(Child.Query(this));

    return Core.ToString();
  }

  public string Visit(ScenariosModel Model)
  {
    return Scan(W => W.WriteLine("model:"), Model.ChildNodes);
  }

  public string Visit(DirectoryNode Directory)
  {
    return Scan(W => W.WriteLine($"directory: {Directory.Name}"), Directory.ChildNodes);
  }

  public string Visit(CurriculumNode Curriculum)
  {
    return Scan(W => W.WriteLine($"curriculum: {Curriculum.Name}"), Curriculum.ChildNodes);
  }

  public string Visit(CapabilityNode Capability)
  {
    return Scan(W => W.WriteLine($"capability: {Capability.Name}"), Capability.ChildNodes);
  }

  public string Visit(MindPlaceNode MindPlace)
  {
    return Scan(W => W.WriteLine($"mind place: {MindPlace.Name}"), MindPlace.ChildNodes);
  }

  public string Visit(BehaviorNode Behavior)
  {
    return Scan(W => W.WriteLine($"behavior: {Behavior.Name}"), Behavior.ChildNodes);
  }

  public string Visit(CurriculumPhaseNode CurriculumPhase)
  {
    return Scan(W => W.WriteLine($"curriculum phase: {CurriculumPhase.Name}"), CurriculumPhase.ChildNodes);
  }
}