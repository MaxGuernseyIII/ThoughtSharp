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

using System.CommandLine;
using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

class Program
{
  public static async Task Main(string[] Args)
  {
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

    RootCommand.SetHandler(async ToTrain =>
    {
      var Assembly = new ShapingAssemblyLoadContext(ToTrain.FullName).LoadFromAssemblyPath(ToTrain.FullName);
      var Parser = new AssemblyParser();

      var Model = Parser.Parse(Assembly);
      var CurriculumNodes = Model.Query(new CrawlingVisitor<CurriculumNode>()
        {
          VisitCurriculum = C => [C]
        }
      );

      var Scheme = new TrainingDataScheme(Model, new() {MaximumAttempts = 0, SampleSize = 1, SuccessFraction = 1}, S => new ConsoleReporter(S));

      foreach (var Curriculum in CurriculumNodes.Where(C => C.Name == "FizzBuzzTrainingPlan"))
      {
        Console.WriteLine($"Training curriculum: {Curriculum.Name}");

        var Pool = new MindPool(Model.MindPlaceIndex);

        var Plan = Model.BuildTrainingPlanFor(Curriculum, Pool, Scheme);
        await Plan.Run();

        //((ConsoleReporter) Scheme.Reporter).Stop();
      }
    }, AssemblyArgument);

    Environment.ExitCode = await RootCommand.InvokeAsync(Args);
  }
}