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
using System.CommandLine.Parsing;

namespace dotnet_train;

static class Commands
{
  static RootCommand GetRootCommand()
  {
    return new()
    {
      Name = "dotnet"
    };
  }

  static Argument<FileInfo> GetProjectArgument()
  {
    var ProjectArgument = new Argument<FileInfo>(
      "project", A => ParseProjectArgumentString(A.Tokens.FirstOrDefault()?.Value ?? ".", A), true, "The project file or folder to train.")
    {
      Arity = ArgumentArity.ZeroOrOne
    };
    ProjectArgument.SetDefaultValueFactory(A => ParseProjectArgumentString(".", A));
    return ProjectArgument;
  }

  static FileInfo ParseProjectArgumentString(string ProjectPath, ArgumentResult A)
  {
    if (File.Exists(ProjectPath))
      return new(ProjectPath);

    if (!Directory.Exists(ProjectPath))
      return Error($"Could not locate the project file for '{ProjectPath}'");

    var ProjectFilePaths = Directory.GetFiles(ProjectPath, "*.csproj");

    if (ProjectFilePaths.Length < 1)
      return Error($"No project files found in '{ProjectPath}'");

    if (ProjectFilePaths.Length > 1)
      return Error($"Too many project files found in '{ProjectPath}':{Environment.NewLine}{string.Join(Environment.NewLine,
        ProjectFilePaths.Select(P => $"  - {P}"))}");

    return new(ProjectFilePaths[0]);

    FileInfo Error(string Message)
    {
      A.ErrorMessage = Message;
      return null!;
    }
  }

  static Option<bool> GetNoBuildOption()
  {
    return new(["--no-build"], "Skip the build.");
  }

  static Option<string[]?> GetExtraArgumentsOption()
  {
    return new(["--"], "Additional arguments passed through to MSBuild.");
  }

  static Option<string[]?> GetCurriculaConstraintOption()
  {
    return new(["--curricula"], "Explicitly-selected curricula")
    {
      IsRequired = false
    };
  }

  public static RootCommand GetCommand()
  {
    var Root = GetRootCommand();
    var C = GetTrainCommand(Root);
    var ProjectArgument = GetProjectArgument();
    var NoBuildOption = GetNoBuildOption();
    var ExtraArgumentsOption = GetExtraArgumentsOption();
    var CurriculaConstraint = GetCurriculaConstraintOption();
    C.AddArgument(ProjectArgument);
    C.AddOption(NoBuildOption);
    C.AddOption(ExtraArgumentsOption);
    C.AddOption(CurriculaConstraint);

    C.SetHandler(DotnetTrain.Handle, new TargetAssemblyResolutionRequestBinder(
      ProjectArgument, NoBuildOption, ExtraArgumentsOption), CurriculaConstraint);

    return Root;
  }

  static Command GetTrainCommand(RootCommand Root)
  {
    var Command = new Command("train",
      "Train models with ThoughtSharp for a single .NET project. Defaults to the current directory.");
    Root.AddCommand(Command);

    return Command;
  }
}