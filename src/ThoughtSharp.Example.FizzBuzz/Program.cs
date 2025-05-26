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
const int TotalTrainingPasses = 20000;
const int ReportEvery = 100;

DoForcedTraining(TotalTrainingPasses, ReportEvery);

void DoForcedTraining(int TotalTrainingPasses1, int ReportEvery1)
{
  var Builder = new TorchBrainBuilder(1, 1);
 
  var Brain = Builder.Build();

  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = Random.NextSingle();
    var Inference = Brain.MakeInference([Input]);
    var Expected = Input > .9 ? 0f : 1f;
    if ((Inference.Result[0] - Expected) > 0.01)
    {
      Inference.Train([Expected]);
      Failures++;
    }
    else
    {
      Successes++;
    }
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");


  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
  }
}

void DoArea(int TotalTrainingPasses1, int ReportEvery1)
{
  var BrainBuilder = TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>();

  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  var ShapesMind = new ShapesMind(BrainBuilder.Build());

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    var Failure = false;
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = Random.NextSingle() * 200 - 100;
    var T = ShapesMind.MakeSquare(Input);


    float Expected = Input * Input;
    try
    {
      var Actual = T.ConsumeDetached();


      if (Math.Abs(Actual.Area - Expected) > Math.Abs(Expected * .001f))
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
      Failure = true;
      Failures++;
      Exceptions++;
    }

    if (Failure)
      T.Feedback.ResultShouldHaveBeen(new() { Area = Expected });
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
  }
}
void DoAreaRaw(int TotalTrainingPasses1, int ReportEvery1)
{
  var BrainBuilder = new TorchBrainBuilder(1, 1);

  var Brain = BrainBuilder.Build();

  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    var Failure = false;
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = Random.NextSingle();

    var Output = Brain.MakeInference([Input]);


    float Expected = Input * Input;
    try
    {
      var Actual = Output.Result[0];


      if (Math.Abs(Actual - Expected) > Math.Abs(Expected * .01f))
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
      Failure = true;
      Failures++;
      Exceptions++;
    }

    if (Failure)
      Output.Train([Expected]);
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
  }
}


void DoFizzBuzz(int TotalTrainingPasses1, int ReportEvery1)
{
  var Mind = new FizzBuzzMind(TorchBrainBuilder.For<FizzBuzzMind.Input, FizzBuzzMind.Output>()
    .Build());
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = Random.Next(1, 101);
    var Terminal = new StringBuilderTerminal();
    var T = Thought.WithFeedback<IReadOnlyList<UseFeedback<Terminal>>>.Do(R =>
    {
      FizzBuzzHybridReasoning.WriteForOneNumber(Mind, 5, R, Terminal, Input);
    });

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

    if (Failure)
      switch (Expected)
      {
        case "Fizz":
          T.Feedback.FirstOrDefault()?.ExpectationsWere((Mock, More) =>
          {
            Mock.Fizz();
            More.Value = false;
          });
          return;
        case "Buzz":
          T.Feedback.FirstOrDefault()?.ExpectationsWere((Mock, More) =>
          {
            Mock.Buzz();
            More.Value = false;
          });
          return;
        case "FizzBuzz":
          T.Feedback.FirstOrDefault()?.ExpectationsWere((Mock, More) =>
          {
            Mock.Fizz();
            More.Value = true;
          });
          T.Feedback.ElementAtOrDefault(1)?.ExpectationsWere((Mock, More) =>
          {
            Mock.Buzz();
            More.Value = false;
          });
          return;
        default:
          T.Feedback.FirstOrDefault()?.ExpectationsWere((Mock, More) =>
          {
            Mock.WriteNumber((short)Input);
            More.Value = false;
          });
          break;
      }
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
  }
}
void DoFizzBuzzRaw(int TotalTrainingPasses1, int ReportEvery1)
{
  var Builder = new TorchBrainBuilder(1, 2);
  Builder.Layers.Clear();
  Builder.Layers.Add(new()
  {
    Features = 100,
    ActivationType = TorchBrainBuilder.ActivationType.ReLU
  });
  Builder.Layers.Add(new()
  {
    Features = 100,
  });
  Builder.SetDefaultClassificationConfiguration();
  var Brain = Builder.Build();

  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Failure = false;
    var Input = Random.Next(1, 101);

    var ExpectedFizz = Input % 3 == 0;
    var ExpectedBuzz = Input % 5 == 0;

    var Inference = Brain.MakeInference([Input * 0.01f]);

    try
    {
      var Results = Inference.Result;
      var Fizz = Results[0] > .5;
      var Buzz = Results[1] > .5;

      if ((Fizz, Buzz) != (ExpectedFizz, ExpectedBuzz))
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

    if (Failure)
      Inference.Train([ExpectedFizz ? 1 : 0, ExpectedBuzz ? 1 : 0]);
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  Console.WriteLine();

  for (var I = 1; I <= 100; ++I)
  {
    using var Inference = Brain.MakeInference([I * 0.01f]);
    var InferenceResult = Inference.Result;
    var Fizz = InferenceResult[0] > .5;
    var Buzz = InferenceResult[1] > .5;

    if (Fizz)
      Console.Write("Fizz");
    if (Buzz)
      Console.Write("Fizz");
    if (!(Fizz || Buzz))
      Console.Write(I);
  }

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
  }
}