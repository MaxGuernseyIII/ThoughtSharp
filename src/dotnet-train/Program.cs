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
  var CurriculumNodes = Model.Query(new CrawlingVisitor<CurriculumNode>()
    {
      VisitCurriculum = C => [C]
    }
  );

  //var Scheme = new TrainingDataScheme();
  //var Reporter = new ConsoleReporter()

  foreach (var Curriculum in CurriculumNodes)
  {
    Console.WriteLine($"Training curriculum: {Curriculum.Name}");


    var Pool = new MindPool(Model.MindPlaceIndex);

    //var Plan = Model.BuildTrainingPlanFor(Curriculum, Pool,  )
  }
}, AssemblyArgument);

Environment.ExitCode = RootCommand.Invoke(args);

class ConsoleReporter(TrainingDataScheme Scheme) : Reporter
{
  public void Start()
  {
    Task.Run(async () =>
    {
      while (true)
      {
        await Task.Delay(TimeSpan.FromSeconds(0.5));

        foreach (var Node in Scheme.TrackedNodes)
        {
          var State = Scheme.GetConvergenceTrackerFor(Node);
          var Convergence = State.MeasureConvergence();

          Console.WriteLine($"{Node.Name}: {Convergence:P}");
        }
      }
      // ReSharper disable once FunctionNeverReturns
    });
  }
  public void ReportRunResult(ScenariosModelNode Node, RunResult Result)
  {
  }

  public void ReportEnter(ScenariosModelNode Node)
  {
    Console.WriteLine($"Enter: {Node.Name}");
  }

  public void ReportExit(ScenariosModelNode Node)
  {
    Console.WriteLine($"Exit: {Node.Name}");
  }
}