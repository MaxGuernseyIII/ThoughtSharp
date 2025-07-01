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

using System.Reflection;
using System.Runtime.ExceptionServices;

namespace ThoughtSharp.Scenarios.Model;

public sealed record BehaviorRunner(MindPool Pool, Type HostType, MethodInfo BehaviorMethod) : Runnable
{
  readonly struct ConsoleCapture(
    TextWriter OldOutput, 
    TextWriter OldError,
    StringWriter Output) : IDisposable
  {
    public StringWriter Output { get; } = Output;

    public static ConsoleCapture Start()
    {
      var OldOutput = Console.Out;
      var OldError = Console.Error;
      var OutputCapture = new StringWriter();

      Console.SetOut(OutputCapture);
      Console.SetError(OutputCapture);

      return new(OldOutput, OldError, OutputCapture);
    }

    public void Dispose()
    {
      Console.SetOut(OldOutput);
      Console.SetError(OldError);
    }
  }

  public async Task<RunResult> Run()
  {
    using var Captured = ConsoleCapture.Start();

    try
    {
      var Result = Execute();

      Result = await PreProcessResult(Result);

      if (Result is Transcript ResultTranscript)
        return CreateGradedResult(ResultTranscript, Captured.Output);

      if (Result is Task T)
        await T;
    }
    catch (Exception Exception)
    {
      return CreateExceptionResult(Exception, Captured.Output);
    }

    return CreateCompletionResult(Captured.Output);
  }

  static async Task<object?> PreProcessResult(object? Result)
  {
    if (Result is Task<Grade> GradeTask)
      Result = await GradeTask;

    if (Result is Task<Transcript> TranscriptTask)
      Result = await TranscriptTask;

    if (Result is Grade G)
      Result = new Transcript([G]);

    return Result;
  }

  static RunResult CreateCompletionResult(StringWriter OutputCapture)
  {
    return new()
    {
      Status = BehaviorRunStatus.Success,
      Transcript = new([new() {Score = 1f, Annotations = []}]),
      Output = OutputCapture.ToString()
    };
  }

  static RunResult CreateExceptionResult(Exception Exception, StringWriter OutputCapture)
  {
    var ProcessedException = Exception;

    while (ProcessedException is TargetInvocationException or TypeInitializationException)
      ProcessedException = ProcessedException.InnerException;

    if (ProcessedException is FatalErrorException)
      ExceptionDispatchInfo.Throw(ProcessedException);

    var RunResult = new RunResult()
    {
      Status = BehaviorRunStatus.Failure,
      Transcript = new([
        new() {Score = 0f, Annotations = [$"unexpected exception of type {ProcessedException!.GetType().Name}"]}
      ]),
      Exception = ProcessedException,
      Output = OutputCapture.ToString()
    };
    return RunResult;
  }

  static RunResult CreateGradedResult(Transcript ResultTranscript, StringWriter OutputCapture)
  {
    return new()
    {
      Status = ResultTranscript.Grades.All(ResultGrade => ResultGrade.Score >= 1f)
        ? BehaviorRunStatus.Success
        : BehaviorRunStatus.Failure,
      Transcript = ResultTranscript,
      Output = OutputCapture.ToString()
    };
  }

  object? Execute()
  {
    var Constructor = HostType.GetConstructors().Single();
    var Minds = Constructor.GetParameters().Select(P => Pool.GetMind(P.ParameterType)).ToArray();
    var Instance = Constructor.Invoke(Minds);
    var Result = BehaviorMethod.Invoke(Instance, []);
    return Result;
  }
}