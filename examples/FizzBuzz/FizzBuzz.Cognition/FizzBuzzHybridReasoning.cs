// MIT License
// 
// Copyright (c) 2024-2024 Producore LLC
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

using System.Text;
using ThoughtSharp.Runtime;

namespace FizzBuzz.Cognition;

public class FizzBuzzHybridReasoning(FizzBuzzMind Mind)
{
  public string DoFullFizzBuzz()
  {
    var Terminal = new ProductionTerminal();

    for (byte I = 1; I <= 100; ++I)
    {
      CalculateStepValue(I, Terminal);
      Terminal.Flush();
    }

    return Terminal.Result.Trim();
  }

  public AccumulatedUseFeedback<FizzBuzzTerminal> CalculateStepValue(byte Input, FizzBuzzTerminal Terminal)
  {
    return Mind.Use(M => M.WriteForNumber(Terminal, new() { Value = Input }));
  }

  class ProductionTerminal : FizzBuzzTerminal
  {
    readonly StringBuilder CurrentContentBuilder = new();

    public string Result => CurrentContentBuilder.ToString();

    public void WriteNumber(byte ToWrite)
    {
      CurrentContentBuilder.Append(ToWrite);
    }

    public void Fizz()
    {
      CurrentContentBuilder.Append("fizz");
    }

    public void Buzz()
    {
      CurrentContentBuilder.Append("buzz");
    }

    public void Flush()
    {
      CurrentContentBuilder.Append(" ");
    }
  }
}