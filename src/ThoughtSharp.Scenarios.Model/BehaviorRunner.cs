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

namespace ThoughtSharp.Scenarios.Model;

public sealed record BehaviorRunner(MindPool Pool, Type HostType, MethodInfo BehaviorMethod) : Runnable
{
  public async Task<RunResult> Run()
  {
    var Constructor = HostType.GetConstructors().Single();
    var Minds = Constructor.GetParameters().Select(P => Pool.GetMind(P.ParameterType)).ToArray();
    var Instance = Constructor.Invoke(Minds);

    try
    {
      var Result = BehaviorMethod.Invoke(Instance, []);
      if (Result is Task T)
        await T;
    }
    catch (Exception Exception)
    {
      var Unwrapped = Exception is TargetInvocationException ? Exception.InnerException : Exception;
      return new()
      {
        Status = BehaviorRunStatus.Failure,
        Exception = Unwrapped
      };
    }

    return new()
    {
      Status = BehaviorRunStatus.Success
    };
  }
}