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
using TorchSharp;

Console.WriteLine("Training...");
const int TotalTrainingPasses = 1000000;
const int ReportEvery = 1000;

DoFizzBuzz(TotalTrainingPasses, ReportEvery);

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
    if (Inference.Result[0] - Expected > 0.01)
    {
      Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected])));
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


    var Expected = Input * Input;
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
      T.Feedback.ResultShouldHaveBeen(new() {Area = Expected});
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


    var Expected = Input * Input;
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
      Output.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected])));
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
  var BrainBuilder = TorchBrainBuilder.For<FizzBuzzMind>().ForLogic(16, 8).WithStateCoefficient(8).WithPath([]);

  var Mind = new FizzBuzzMind(BrainBuilder.Build());
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  var BatchSuccesses = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  var BatchFizzFailures = 0;
  var BatchBuzzFailures = 0;
  var BatchFizzBuzzFailures = 0;
  var BatchWriteFailures = 0;
  var LastWriteFailure = string.Empty;
  List<int> ByteDeviation = [];
  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = Random.Next(byte.MinValue, byte.MaxValue);
    var Terminal = new StringBuilderTerminal();
    var T = Thought.Do(R =>
    {
      var Feedback = new List<UseFeedback<Terminal>>();
      FizzBuzzHybridReasoning.WriteForOneNumber(Mind, 5, R, Terminal, Input, Feedback);

      return Feedback;
    });

    var Failure = false;
    string Expected;
    const string Fizz = "fizz";
    const string Buzz = "buzz";
    const string FizzBuzz = Fizz + Buzz;
    if (Input % 15 == 0)
      Expected = FizzBuzz;
    else if (Input % 3 == 0)
      Expected = Fizz;
    else if (Input % 5 == 0)
      Expected = Buzz;
    else
      Expected = $"{Input}";

    var Actual = string.Empty;
    try
    {
      T.RaiseAnyExceptions();

      Actual = Terminal.Content.ToString();
      if (Expected is not (Fizz or Buzz or FizzBuzz))
        foreach (var WrittenByte in Terminal.WrittenBytes)
          ByteDeviation.Add(Math.Abs(WrittenByte - Input));

      if (Actual != Expected)
      {
        Failure = true;
        Failures++;
      }
      else
      {
        Successes++;
        BatchSuccesses++;
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
        case Fizz:
          BatchFizzFailures++;
          break;
        case Buzz:
          BatchBuzzFailures++;
          break;
        case FizzBuzz:
          BatchFizzBuzzFailures++;
          break;
        default:
          BatchWriteFailures++;
          LastWriteFailure = $"expected <{Expected}> got <{Actual}>";
          break;
      }

    switch (Expected)
    {
      case Fizz:
        T.Feedback.First().ExpectationsWere((Mock, More) =>
        {
          Mock.Fizz();
          More.Value = false;
        });
        break;
      case Buzz:
        T.Feedback.First().ExpectationsWere((Mock, More) =>
        {
          Mock.Buzz();
          More.Value = false;
        });
        break;
      case FizzBuzz:
        T.Feedback.First().ExpectationsWere((Mock, More) =>
        {
          Mock.Fizz();
          More.Value = true;
        });
        T.Feedback.ElementAtOrDefault(1)?.ExpectationsWere((Mock, More) =>
        {
          Mock.Buzz();
          More.Value = false;
        });
        break;
      default:
        T.Feedback.First().ExpectationsWere((Mock, More) =>
        {
          Mock.WriteNumber((byte) Input);
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
    Console.WriteLine($"  Batch success rate:             {BatchSuccesses * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch fizz failure rate:        {BatchFizzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch buzz failure rate:        {BatchBuzzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch fizzbuzz failure rate:    {BatchFizzBuzzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch write failure rate:       {BatchWriteFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Last write failure:             {LastWriteFailure}");
    if (ByteDeviation.Any())
      Console.WriteLine($"  Average written byte deviation: {ByteDeviation.Select(D => D * 1f).Average():0.00}");
    BatchSuccesses = 0;
    BatchFizzFailures = 0;
    BatchBuzzFailures = 0;
    BatchFizzBuzzFailures = 0;
    BatchWriteFailures = 0;
    ByteDeviation.Clear();
  }
}

void Modulo3Check(int TotalTrainingPasses1, int ReportEvery1)
{
  //var BrainBuilder = new TorchBrainBuilder(2, 1).ForLogic();
  //BrainBuilder.StateCoefficient = 16;
  var Brain = new TorchBrainForTrainingMode(new StatePassThroughModule(torch.nn.Sequential(
    torch.nn.Linear(8, 64),
    torch.nn.ReLU(),
    torch.nn.Linear(64, 32),
    torch.nn.ReLU(),
    torch.nn.Linear(32, 1)
  )), new(DeviceType.CPU), 0);
  //var Brain = BrainBuilder.Build();
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  List<bool> FailureLog = [];

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = (byte) Random.Next(byte.MinValue, byte.MaxValue + 1);
    var InputVector = new float[8];
    foreach (var I in Enumerable.Range(0, 8))
      InputVector[I] = (Input >> I) & 1;

    var Expected = Input % 3 == 0;

    var Inference = Brain.MakeInference(InputVector);
    var Actual = Inference.Result[0] > .5;

    var Failure = false;

    if (Actual != Expected)
    {
      Failure = true;
      Failures++;
    }
    else
    {
      Successes++;
    }

    FailureLog.Add(Failure);

    //if (Failure)
    Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected ? 1 : 0])));
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    if (I >= 1000)
      Console.WriteLine($"  Errors Per 1000: {FailureLog[^1000..].Count(F => F)}");
  }
}

void Modulo3AndCheck(int TotalTrainingPasses1, int ReportEvery1)
{
  //var BrainBuilder = new TorchBrainBuilder(2, 1).ForLogic();
  //BrainBuilder.StateCoefficient = 16;
  var Brain = new TorchBrainForTrainingMode(new StatePassThroughModule(torch.nn.Sequential(
    torch.nn.Linear(8, 32),
    torch.nn.ReLU(),
    torch.nn.Linear(32, 32),
    torch.nn.ReLU(),
    torch.nn.Linear(32, 1)
  )), new(DeviceType.CPU), 0);
  //var Brain = BrainBuilder.Build();
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  List<bool> FailureLog = [];

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input = (byte) Random.Next(byte.MinValue, byte.MaxValue + 1);
    var InputVector = new float[8];
    foreach (var I in Enumerable.Range(0, 8))
      InputVector[I] = (Input >> I) & 1;

    var Expected = Input % 3 == 0;

    var Inference = Brain.MakeInference(InputVector);
    var Actual = Inference.Result[0] > .5;

    var Failure = false;

    if (Actual != Expected)
    {
      Failure = true;
      Failures++;
    }
    else
    {
      Successes++;
    }

    FailureLog.Add(Failure);

    //if (Failure)
    Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected ? 1 : 0])));
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    if (I >= 1000)
      Console.WriteLine($"  Errors Per 1000: {FailureLog[^1000..].Count(F => F)}");
  }
}


void ComparisonCheck(int TotalTrainingPasses1, int ReportEvery1)
{
  //var BrainBuilder = new TorchBrainBuilder(2, 1).ForLogic();
  //BrainBuilder.StateCoefficient = 16;
  var Brain = new TorchBrainForTrainingMode(new StatePassThroughModule(torch.nn.Sequential(
    torch.nn.Linear(2, 64),
    torch.nn.Tanh(),
    torch.nn.Linear(64, 32),
    torch.nn.ReLU(),
    torch.nn.Linear(32, 1),
    torch.nn.Sigmoid()
  )), new(DeviceType.CPU), 0);
  //var Brain = BrainBuilder.Build();
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

  List<bool> FailureLog = [];

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input1 = Random.NextSingle() * 2 - 1;
    var Input2 = Random.NextSingle() * 2 - 1;
    var Expected = Input1 > Input2;

    var Inference = Brain.MakeInference([Input1, Input2]);
    var Actual = Inference.Result[0] > .5;

    var Failure = false;

    if (Actual != Expected)
    {
      Failure = true;
      Failures++;
    }
    else
    {
      Successes++;
    }

    FailureLog.Add(Failure);

    //if (Failure)
    Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected ? 1 : 0])));
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    if (I >= 1000)
      Console.WriteLine($"  Errors Per 1000: {FailureLog[^1000..].Count(F => F)}");
  }
}

void MindfulComparisonCheck(int TotalTrainingPasses1, int ReportEvery1)
{
  var Brain = TorchBrainBuilder.For<ComparisonMind>().ForLogic().Build();
  var Mind = new ComparisonMind(Brain);
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  List<bool> FailureLog = [];

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
  {
    if (Iteration % ReportEvery1 == 0)
      Report(Iteration);

    var Input1 = Random.Next(0, byte.MaxValue);
    var Input2 = Random.Next(0, byte.MaxValue);
    var Expected = Input1.CompareTo(Input2);

    var T = Mind.Compare(new() {Left = (byte) Input1, Right = (byte) Input2});

    bool Failure;
    try
    {
      var Actual = T.ConsumeDetached();
      if (Actual.Comparison != Expected)
      {
        Failures++;
        Failure = true;
      }
      else
      {
        Successes++;
        Failure = false;
      }
    }
    catch
    {
      Exceptions++;
      Failures++;
      Failure = true;
    }

    FailureLog.Add(Failure);

    //if (Failure)
    T.Feedback.ResultShouldHaveBeen(new()
    {
      Comparison = (short) Expected
    });
  }

  Report(TotalTrainingPasses1);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    if (I >= 1000)
      Console.WriteLine($"  Errors Per 1000: {FailureLog[^1000..].Count(F => F)}");
  }
}

//void DoFizzBuzzRaw(int TotalTrainingPasses1, int ReportEvery1)
//{
//  var Builder = new TorchBrainBuilder(1, 2);
//  Builder.Layers.Clear();
//  Builder.Layers.Add(new()
//  {
//    Features = 100,
//    ActivationType = TorchBrainBuilder.ActivationType.ReLU
//  });
//  Builder.Layers.Add(new()
//  {
//    Features = 100
//  });
//  Builder.ForClassification();
//  var Brain = Builder.Build();

//  var Random = new Random();
//  var Successes = 0;
//  var Failures = 0;
//  var Exceptions = 0;

//  //var ShapesMind = new ShapesMind(TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>().Build());

//  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses1))
//  {
//    if (Iteration % ReportEvery1 == 0)
//      Report(Iteration);

//    var Failure = false;
//    var Input = Random.Next(1, 101);

//    var ExpectedFizz = Input % 3 == 0;
//    var ExpectedBuzz = Input % 5 == 0;

//    var Inference = Brain.MakeInference([Input * 0.01f]);

//    try
//    {
//      var Results = Inference.Result;
//      var Fizz = Results[0] > .5;
//      var Buzz = Results[1] > .5;

//      if ((Fizz, Buzz) != (ExpectedFizz, ExpectedBuzz))
//      {
//        Failure = true;
//        Failures++;
//      }
//      else
//      {
//        Successes++;
//      }
//    }
//    catch (Exception)
//    {
//      //Console.WriteLine("Original exception:");
//      //Console.WriteLine(Ex);

//      //Console.WriteLine();
//      //Console.WriteLine();
//      Failure = true;
//      Failures++;
//      Exceptions++;
//    }

//    if (Failure)
//      Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([ExpectedFizz ? 1 : 0, ExpectedBuzz ? 1 : 0])));
//  }

//  Report(TotalTrainingPasses1);

//  Console.WriteLine("Done.");

//  Console.WriteLine();

//  for (var I = 1; I <= 100; ++I)
//  {
//    using var Inference = Brain.MakeInference([I * 0.01f]);
//    var InferenceResult = Inference.Result;
//    var Fizz = InferenceResult[0] > .5;
//    var Buzz = InferenceResult[1] > .5;

//    if (Fizz)
//      Console.Write("Fizz");
//    if (Buzz)
//      Console.Write("Fizz");
//    if (!(Fizz || Buzz))
//      Console.Write(I);
//  }

//  void Report(int I)
//  {
//    Console.WriteLine(
//      $"{I * 100 / TotalTrainingPasses1}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
//  }
//}