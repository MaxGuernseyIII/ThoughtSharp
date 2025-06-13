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

using CliWrap;
using CliWrap.Buffered;
using ThoughtSharp.Scenarios.Model;

namespace dotnet_train;

static class DotnetTrain
{
  public static async Task Handle(TargetAssemblyResolutionRequest Request, string[]? CurriculaConstraint)
  {
    if (!Request.NoBuild)
      await Build(Request);

    Predicate<CurriculumNode?> SelectNode = CurriculaConstraint is not null
      ? N => CurriculaConstraint.Any(C => N?.Name == C)
      : delegate { return true; };

    var TargetPath = await GetTargetPath(Request);
    if (TargetPath is null)
      throw new ApplicationException("Could not locate target path for selected project.");
    Directory.SetCurrentDirectory(Path.GetDirectoryName(TargetPath)!);
    var Assembly = new ShapingAssemblyLoadContext(TargetPath).LoadFromAssemblyPath(TargetPath);
    var Parser = new AssemblyParser();

    var Model = Parser.Parse(Assembly);
    var CurriculumNodes = Model.Query(new CrawlingVisitor<CurriculumNode>
      {
        VisitCurriculum = C => [C]
      }
    );

    var Scheme = new TrainingDataScheme(Model, new() {MaximumAttempts = 0, SampleSize = 1, SuccessFraction = 1});
    var ConsoleReporter = new ConsoleReporter(Scheme);
    foreach (var Curriculum in CurriculumNodes)
    {
      if (!SelectNode(Curriculum))
        continue;

      Console.WriteLine($"Training curriculum: {Curriculum.Name}");

      var Pool = new MindPool(Model.MindPlaceIndex);

      var Plan = Model.BuildTrainingPlanFor(Curriculum, Pool, Scheme, ConsoleReporter);
      await Plan.Run();
    }

    await ConsoleReporter.Stop();
  }

  static async Task Build(TargetAssemblyResolutionRequest Request)
  {
    await Cli
      .Wrap("dotnet")
      .WithArguments(
      [
        "msbuild",
        Request.ProjectPath.FullName,
        "-noLogo",
        "--",
        ..Request.ExtraArguments
      ])
      .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
      .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
      .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
      .ExecuteAsync();
  }

  static async Task<string?> GetTargetPath(TargetAssemblyResolutionRequest Request)
  {
    var Result = await Cli
      .Wrap("dotnet")
      .WithArguments(
      [
        "msbuild",
        Request.ProjectPath.FullName,
        "-getProperty:TargetPath",
        "-noLogo",
        "--",
        ..Request.ExtraArguments
      ]).ExecuteBufferedAsync();

    return Result.StandardOutput.Trim();
  }
}