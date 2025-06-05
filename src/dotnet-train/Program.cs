// See https://aka.ms/new-console-template for more information

using CommandLine;
using dotnet_train;

Parser.Default.ParseArguments<Options>(args)
  .WithParsed(Args =>
  {
    Console.WriteLine($"You want me to parse {Args.RelativeAssemblyPath}");
  })
  .WithNotParsed(_ =>
  {
    Console.Error.WriteLine("Your arguments weren't understood. Try --help.");
    Environment.ExitCode = -1;
  });
