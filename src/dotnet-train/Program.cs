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

var RootCommand = new RootCommand("Train models with ThoughtSharp");
var TargetArgument = new Argument<FileInfo?>(
    "--target",
    description: "The .sln, .csproj, or .dll to train",
    isDefault: true,
    parse: A =>
    {
      var ToTrain = GetFileSystemObjectToTrain(A);

      if (ToTrain is FileInfo R)
        return R;

      if (ToTrain is DirectoryInfo D)
        return ResolveDirectoryToFile(D, A);

      return null;
    })
  {
    Arity = ArgumentArity.ZeroOrOne
  }
  .AddCompletions(C =>
  {
    return Directory
      .EnumerateFileSystemEntries(Directory.GetCurrentDirectory(), "*.*", SearchOption.TopDirectoryOnly)
      .Select(F => Path.GetFileName(F)!)
      .Where(F => F.StartsWith(C.WordToComplete, StringComparison.OrdinalIgnoreCase))
      .Where(F => F.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                  F.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                  F.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
  });

RootCommand.AddArgument(TargetArgument);

RootCommand.SetHandler(ToTrain =>
{
  if (ToTrain is not null)
    Console.WriteLine($"You want me to train {ToTrain.FullName}");
  else
    Console.WriteLine("You want me to train whatever is in this directory.");
}, TargetArgument);

Environment.ExitCode = RootCommand.Invoke(args);

static FileInfo? ResolveDirectoryToFile(DirectoryInfo Directory, ArgumentResult Result)
{
  var Options = Directory.GetFiles("*.sln").Concat(Directory.GetFiles("*.csproj"));
  FileInfo? Inferred = null;
  if (!Options.Any())
    Result.ErrorMessage = "No solution or project files found in that directory";
  else if (Options.Take(2).Count() > 1)
    Result.ErrorMessage =
      $"Found too many files:{string.Join("", Options.Select(O => Environment.NewLine + "  - " + O.FullName))}";
  else
    Inferred = Options.Single();

  return Inferred;
}

FileSystemInfo? GetFileSystemObjectToTrain(ArgumentResult ArgumentResult)
{
  if (!ArgumentResult.Tokens.Any()) 
    return null;

  var Path = ArgumentResult.Tokens.Single().Value;

  if (Directory.Exists(Path))
    return new DirectoryInfo(Path);

  return new FileInfo(Path);
}