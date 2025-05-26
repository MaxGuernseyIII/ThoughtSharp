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

using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Example.FizzBuzz;
using ThoughtSharp.Runtime;

Console.WriteLine("Training...");
const int TotalTrainingPasses = 10000;
const int ReportEvery = 1000;

var Mind = new FizzBuzzMind(TorchBrainBuilder.For<FizzBuzzMind.Input, FizzBuzzMind.Output>().Build());
var Random = new Random();
var Successes = 0;
var Failures = 0;
var Exceptions = 0;

var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
{
  if (Iteration % ReportEvery == 0)
    Report(Iteration);

  var Input = Random.Next(1, 101);
  var Terminal = new StringBuilderTerminal();
  var T = Thought.Do(R => { FizzBuzzHybridReasoning.WriteForOneNumber(Mind, 5, R, Terminal, Input); });

  var Failure = false;
  string Expected;
  if (Input % 15 == 0)
    Expected = "fizzbuzz";
  else if (Input % 3 == 0)
    Expected = "fizz";
  else if (Input % 5 == 0)
    Expected = "buzz";
  else
    Expected = $"{Input}";
  try
  {
    T.RaiseAnyExceptions();

    var Actual = Terminal.Content.ToString();
    

    if (Actual != Expected)
    {
      Failure = true;
      Failures++;
    }
    else
    {
      Successes++;
    }
  }
  catch (Exception)
  {
    //Console.WriteLine("Original exception:");
    //Console.WriteLine(Ex);

    //Console.WriteLine();
    //Console.WriteLine();
    Failure = true;
    Failures++;
    Exceptions++;
  }

  //if (Failure)
  //  T.Feedback
}

Report(TotalTrainingPasses);

Console.WriteLine("Done.");

void Report(int I)
{
  Console.WriteLine($"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
}