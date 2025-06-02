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

using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Example.FizzBuzz;
using ThoughtSharp.Runtime;
using static Disposition;

Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

Console.WriteLine("Training...");
const int TotalTrainingPasses = 1000000;
const int ReportEvery = 1000;

DoFizzBuzz();
//DoChooseShape();

//void DoForcedTraining()
//{
//  var Builder = new TorchBrainBuilder(1, 1);

//  var Brain = Builder.Build();

//  var Random = new Random();
//  var Successes = 0;
//  var Failures = 0;
//  var Exceptions = 0;

//  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
//  {
//    if (Iteration % ReportEvery == 0)
//      Report(Iteration);

//    var Input = Random.NextSingle();
//    var Inference = Brain.MakeInference([Input]);
//    var Expected = Input > .9 ? 0f : 1f;
//    if (Inference.Result[0] - Expected > 0.01)
//    {
//      Inference.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected])));
//      Failures++;
//    }
//    else
//    {
//      Successes++;
//    }
//  }

//  Report(TotalTrainingPasses);

//  Console.WriteLine("Done.");


//  void Report(int I)
//  {
//    Console.WriteLine(
//      $"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
//  }
//}

//void DoArea()
//{
//  var BrainBuilder = TorchBrainBuilder.For<ShapesMind.Input, ShapesMind.Output>();

//  var Random = new Random();
//  var Successes = 0;
//  var Failures = 0;
//  var Exceptions = 0;

//  var ShapesMind = new ShapesMind(BrainBuilder.Build());

//  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
//  {
//    var Failure = false;
//    if (Iteration % ReportEvery == 0)
//      Report(Iteration);

//    var Input = Random.NextSingle() * 200 - 100;
//    var T = ShapesMind.MakeSquare(Input);


//    var Expected = Input * Input;
//    try
//    {
//      var Actual = T.ConsumeDetached();


//      if (Math.Abs(Actual.Area - Expected) > Math.Abs(Expected * .001f))
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
//      Failure = true;
//      Failures++;
//      Exceptions++;
//    }

//    if (Failure)
//      T.Feedback.ResultShouldHaveBeen(new() {Area = Expected});
//  }

//  Report(TotalTrainingPasses);

//  Console.WriteLine("Done.");

//  void Report(int I)
//  {
//    Console.WriteLine(
//      $"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
//  }
//}

//void DoAreaRaw()
//{
//  var BrainBuilder = new TorchBrainBuilder(1, 1);

//  var Brain = BrainBuilder.Build();

//  var Random = new Random();
//  var Successes = 0;
//  var Failures = 0;
//  var Exceptions = 0;

//  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
//  {
//    var Failure = false;
//    if (Iteration % ReportEvery == 0)
//      Report(Iteration);

//    var Input = Random.NextSingle();

//    var Output = Brain.MakeInference([Input]);


//    var Expected = Input * Input;
//    try
//    {
//      var Actual = Output.Result[0];


//      if (Math.Abs(Actual - Expected) > Math.Abs(Expected * .01f))
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
//      Failure = true;
//      Failures++;
//      Exceptions++;
//    }

//    if (Failure)
//      Output.Train((0, new BinaryCrossEntropyWithLogitsLossRule([Expected])));
//  }

//  Report(TotalTrainingPasses);

//  Console.WriteLine("Done.");

//  void Report(int I)
//  {
//    Console.WriteLine(
//      $"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
//  }
//}

void DoFizzBuzz()
{
  var BrainBuilder = TorchBrainBuilder.ForTraining<FizzBuzzMind>()
    .UsingSequence(Outer =>
      Outer
        .AddGRU(128)
        .AddParallel(P => P
          .AddLogicPath(16, 4, 8)
          .AddPath(S => S))
    );

  var Mind = new FizzBuzzMind(BrainBuilder.Build());
  var HybridReasoning = new FizzBuzzHybridReasoning(Mind);
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;
  var ContiguousSuccessBatches = 0;

  var BatchSuccesses = 0;

  var BatchFizzFailures = 0;
  var BatchBuzzFailures = 0;
  var BatchFizzBuzzFailures = 0;
  var BatchWriteFailures = 0;
  var LastWriteFailure = string.Empty;
  List<int> ByteDeviationPerBatch = [];
  List<int> ByteDeviation = [];
  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
  {
    if (Iteration % ReportEvery == 0)
      Report(Iteration);

    if (ContiguousSuccessBatches >= 5)
      break;

    var Input = Iteration % byte.MaxValue;
    var Terminal = new StringBuilderFizzBuzzTerminal();
    var T = HybridReasoning.WriteForOneNumber(Terminal, (byte) Input);
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
          ByteDeviationPerBatch.Add(Math.Abs(WrittenByte - Input));

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
        T.Feedback.TrainWith(
          M => M.Fizz());
        break;
      case Buzz:
        T.Feedback.TrainWith(
          M => M.Buzz());
        break;
      case FizzBuzz:
        T.Feedback.TrainWith(
          M => M.Fizz(), 
          M => M.Buzz());
        break;
      default:
        T.Feedback.TrainWith(
          M => M.WriteNumber((byte) Input));
        break;
    }
  }

  Report(TotalTrainingPasses);

  Console.WriteLine("Done.");

  Console.WriteLine(HybridReasoning.DoFizzBuzz(1, 100));

  void Report(int I)
  {
    if (BatchBuzzFailures + BatchFizzFailures + BatchFizzBuzzFailures + BatchWriteFailures == 0)
      ContiguousSuccessBatches++;
    else
      ContiguousSuccessBatches = 0;

    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    Console.WriteLine($"  Contiguous success batches:          {ContiguousSuccessBatches}");
    Console.WriteLine($"  Batch success rate:                  {BatchSuccesses * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch fizz failure rate:             {BatchFizzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch buzz failure rate:             {BatchBuzzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch fizzbuzz failure rate:         {BatchFizzBuzzFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Batch write failure rate:            {BatchWriteFailures * 1f / ReportEvery:P}");
    Console.WriteLine($"  Last write failure:                  {LastWriteFailure}");
    if (ByteDeviationPerBatch.Any())
    {
      ByteDeviation.AddRange(ByteDeviationPerBatch);
      Console.WriteLine(
        $"  Written byte deviation (this batch): {ByteDeviationPerBatch.Select(D => D * 1f).Average():0.00}");
      Console.WriteLine($"  Written byte deviation (running):    {ByteDeviation.Select(D => D * 1f).Average():0.00}");
    }
    else
    {
      Console.WriteLine("  No deviation this batch.");
    }

    BatchSuccesses = 0;
    BatchFizzFailures = 0;
    BatchBuzzFailures = 0;
    BatchFizzBuzzFailures = 0;
    BatchWriteFailures = 0;
    ByteDeviationPerBatch.Clear();
  }
}

void MindfulComparisonCheck()
{
  var Brain = TorchBrainBuilder.ForTraining<ComparisonMind>().ForLogic().Build();
  var Mind = new ComparisonMind(Brain);
  var Random = new Random();
  var Successes = 0;
  var Failures = 0;
  var Exceptions = 0;

  List<bool> FailureLog = [];

  foreach (var Iteration in Enumerable.Range(0, TotalTrainingPasses))
  {
    if (Iteration % ReportEvery == 0)
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
    catch (Exception Ex)
    {
      Exceptions++;
      Failures++;
      Failure = true;

      Console.Write(Ex);
    }

    FailureLog.Add(Failure);

    //if (Failure)
    T.Feedback.ResultShouldHaveBeen(new()
    {
      Comparison = (short) Expected
    });
  }

  Report(TotalTrainingPasses);

  Console.WriteLine("Done.");

  void Report(int I)
  {
    Console.WriteLine(
      $"{I * 100 / TotalTrainingPasses}% complete: {Successes} successes, {Failures} failures, {Exceptions} exceptions");
    if (I >= 1000)
      Console.WriteLine($"  Errors Per 1000: {FailureLog[^1000..].Count(F => F)}");
  }
}

void DoChooseShape()
{
  //foreach (var I in Enumerable.Range(0, 10))
  //{
  //  ShapeRenderer.RenderShapeToImage(new Shape() { Points = [..Geometry.PrepareForUseInTraining(Geometry.MakeCircle())] }.Normalize(), $"shape{I}.png", closeShape: false);
  //}
  //throw new ApplicationException("Generated.");

  var BrainBuilder = TorchBrainBuilder.ForTraining<ShapeClassifyingMind>()
    .UsingParallel(P => P.AddLogicPath(4, 2).AddMathPath(4, 2));
  var Brain = BrainBuilder.Build();

  var FileName = $"choose-shape_{BrainBuilder.CompactDescriptiveText}.pt";
  try
  {
    Brain.Load(FileName);
  }
  catch
  {
    Console.WriteLine($"Could not load {FileName}, starting from scratch...");
  }

  Console.CancelKeyPress +=
    delegate { Save(); };

  AppDomain.CurrentDomain.ProcessExit +=
    delegate { Save(); };

  void Save()
  {
    Console.WriteLine("Saving to: " + Path.GetFullPath(FileName));
    Brain.Save(FileName);
  }

  var Mind = new ShapeClassifyingMind(Brain);

  var Random = new Random();
  Dictionary<ShapeType, (CognitiveOption<ShapeHandler, ShapeHandlerDescriptor> Option, Func<IEnumerable<Point>>
    MakeShapePoints)> OptionsMap = new()
  {
    [ShapeType.Line] = (new(new LineHandler(), new()
    {
      Features =
      {
        Open = Required,
        Closed = Prohibited,
        ConcaveDepressions = Prohibited,
        ConvexProtrusions = Prohibited,
        FlatSurfaces = Prohibited,
        RadialFlatSurfaces = Prohibited,
        Corners = Prohibited,
        RoundedSegments = Prohibited,
        StraightSegments = Required
      },
      Constraints =
      {
        MaximumVertices = 2,
        MinimumVertices = 2
      }
    }), Geometry.MakeLine),
    [ShapeType.Rectangle] = (new(new RectangleHandler(), new()
    {
      Features =
      {
        Open = Prohibited,
        Closed = Required,
        ConcaveDepressions = Prohibited,
        ConvexProtrusions = Prohibited,
        FlatSurfaces = Required,
        RadialFlatSurfaces = Required,
        Corners = Required,
        RoundedSegments = Prohibited,
        StraightSegments = Required
      },
      Constraints =
      {
        MaximumVertices = 4,
        MinimumVertices = 4,
        MinimumSymmetricalAxes = 2,
        MinimumVertexAngle = MathF.PI / 2 * .95f,
        MaximumVertexAngle = MathF.PI / 2 * 1.05f
      }
    }), Geometry.MakeBox),
    [ShapeType.Irregular] = (new(new IrregularHandler(),
      new()
      {
        Features =
        {
          Open = Prohibited,
          Closed = Required,
          ConcaveDepressions = Supported,
          ConvexProtrusions = Supported,
          FlatSurfaces = Supported,
          RadialFlatSurfaces = Supported,
          Corners = Supported,
          RoundedSegments = Prohibited,
          StraightSegments = Supported
        }
      }
    ), Geometry.MakeIrregular),
    [ShapeType.Circle] = (new(new CircleHandler(),
      new()
      {
        Features =
        {
          Open = Prohibited,
          Closed = Required,
          ConcaveDepressions = Prohibited,
          ConvexProtrusions = Prohibited,
          FlatSurfaces = Prohibited,
          RadialFlatSurfaces = Prohibited,
          Corners = Prohibited,
          RoundedSegments = Required,
          StraightSegments = Prohibited
        },
        Constraints =
        {
          MaximumVertices = 0,
          MinimumSymmetricalAxes = 8
        }
      }), Geometry.MakeCircle),
    [ShapeType.Star] = (new(new StarHandler(),
      new()
      {
        Features =
        {
          Open = Prohibited,
          Closed = Required,
          ConcaveDepressions = Required,
          ConvexProtrusions = Required,
          FlatSurfaces = Required,
          RadialFlatSurfaces = Prohibited,
          Corners = Required,
          RoundedSegments = Prohibited,
          StraightSegments = Required
        },
        Constraints =
        {
          MinimumSymmetricalAxes = 3
        }
      }), Geometry.MakeStar),
    [ShapeType.Arc] = (new(new ArcHandler(),
      new()
      {
        Features =
        {
          Open = Required,
          Closed = Prohibited,
          ConcaveDepressions = Prohibited,
          ConvexProtrusions = Prohibited,
          FlatSurfaces = Prohibited,
          RadialFlatSurfaces = Prohibited,
          Corners = Prohibited,
          RoundedSegments = Required,
          StraightSegments = Prohibited
        },
        Constraints =
        {
          MinimumSymmetricalAxes = 1,
          MaximumVertices = 2,
          MinimumVertices = 2
        }
      }), Geometry.MakeArc),
    [ShapeType.Plus] = (new(new PlusHandler(),
      new()
      {
        Features =
        {
          Open = Prohibited,
          Closed = Required,
          ConcaveDepressions = Required,
          ConvexProtrusions = Required,
          FlatSurfaces = Required,
          RadialFlatSurfaces = Required,
          Corners = Required,
          RoundedSegments = Prohibited,
          StraightSegments = Required
        },
        Constraints =
        {
          MinimumSymmetricalAxes = 2,
          MinimumVertexAngle = MathF.PI / 2 * .95f,
          MaximumVertexAngle = MathF.PI / 2 * 1.05f
        }
      }), Geometry.MakePlus)
  };

  ImmutableArray<ImmutableArray<ShapeType>> ShapeSamplesPlan =
  [
    [ShapeType.Line],
    [ShapeType.Line, ShapeType.Rectangle],
    [ShapeType.Rectangle, ShapeType.Circle],
    [ShapeType.Star, ShapeType.Plus],
    [ShapeType.Arc, ShapeType.Circle],
    [ShapeType.Irregular, ShapeType.Circle],
    [ShapeType.Irregular, ShapeType.Plus],
    [..OptionsMap.Keys.Except([ShapeType.Irregular])],
    [..OptionsMap.Keys]
  ];

  ImmutableArray<(ImmutableArray<ShapeType> ShapeSamples, ImmutableArray<ShapeType> AvailableOptions, int
    ConfidenceThreshold, int ConfidenceSampleCount)> ProduceCurriculum()
  {
    var Result = new List<(ImmutableArray<ShapeType> ShapeSamples, ImmutableArray<ShapeType> AvailableOptions, int
      ConfidenceThreshold, int ConfidenceSampleCount)>();

    foreach (var ShapeSamples in ShapeSamplesPlan)
    {
      var Surplus = OptionsMap.Keys.Except(ShapeSamples);

      foreach (var I in Enumerable.Range(0, Surplus.Count() + 1))
      {
        const int SampleScale = 1;
        Result.Add((ShapeSamples, [..ShapeSamples.Concat(Surplus.Take(I))], 98 * SampleScale, 100 * SampleScale));
      }
    }

    var Last = Result.Last();

    Result.Add((Last.ShapeSamples, Last.AvailableOptions, 3000, 3000));

    return [..Result];
  }

  var Curriculum = ProduceCurriculum();

  //foreach (var ToRemove in OptionsMap.Keys.Except([ShapeType.Line, ShapeType.Circle]).ToArray()) 
  //  OptionsMap.Remove(ToRemove);

  var Failures = 0;
  var Exceptions = 0;

  var FailureDictionary = new Dictionary<(ShapeType, Type), int>();

  void RecordFailure(ShapeType ForType, Type Found)
  {
    var Key = (ForType, Found);
    if (!FailureDictionary.TryGetValue(Key, out var Value))
      FailureDictionary[Key] = Value = 0;

    Value += 1;

    FailureDictionary[Key] = Value;
  }


  var RecentSample = new Queue<bool>();

  int RecentSampleSuccessRate()
  {
    return RecentSample.Count(V => V);
  }

  foreach (var ((CourseShapeTypes, CourseOptions, CourseConfidenceThreshold, CourseConfidenceSampleCount), Level) in
           Curriculum.Select((Course, I) => (Course, I + 1)))
  {
    var RawOptions =
      new List<CognitiveOption<ShapeHandler, ShapeHandlerDescriptor>>(OptionsMap
          .Where(Pair => CourseOptions.Contains(Pair.Key)).Select(Pair => Pair.Value.Option))
        .ToImmutableArray();
    Console.WriteLine(
      $"Starting level {Level}: shapes {string.Join(", ", CourseShapeTypes)} with options {string.Join(", ", CourseOptions)}");

    void RecordSuccess(bool IsSuccess)
    {
      RecentSample.Enqueue(IsSuccess);
      while (RecentSample.Count > CourseConfidenceSampleCount)
        RecentSample.Dequeue();
    }

    foreach (var I in Enumerable.Range(0, TotalTrainingPasses))
    {
      if (I % ReportEvery == 0 && I != 0) Report();

      var ThisTrialShapeType = RandomOneOf(CourseShapeTypes);
      //var ThisTrialShapeType = ShapeType.Line;
      var ThisTrialOptions = RandomizeOrderOf(RawOptions);
      var Category = new ShapeHandlerCategory(ThisTrialOptions);
      var Shape = new Shape
      {
        Points = Geometry.PrepareForUseInTraining(OptionsMap[ThisTrialShapeType].MakeShapePoints()).ToArray()
        //Points = Geometry.GetSimplified(OptionsMap[ThisTrialShapeType].MakeShapePoints()).ToArray()
      };

      var T = Mind.ChooseHandlerFor(Shape.Normalize(), Category);

      var Expected = OptionsMap[ThisTrialShapeType].Option.Payload;
      var Failure = false;
      try
      {
        var Actual = T.ConsumeDetached();
        if (Actual != Expected)
        {
          Failure = true;
          Failures++;
        }

        if (Failure)
          RecordFailure(ThisTrialShapeType, Actual.GetType());
      }
      catch (Exception Ex)
      {
        Failure = true;
        Failures++;
        Exceptions++;

        Console.Write(Ex);
      }

      T.Feedback.SelectionShouldHaveBeen(Expected);

      RecordSuccess(!Failure);

      if (RecentSample.Count >= CourseConfidenceSampleCount && RecentSampleSuccessRate() >= CourseConfidenceThreshold)
      {
        Console.WriteLine($"Finished with level {Level} in {I + 1} runs.");
        Failures = 0;
        Exceptions = 0;
        RecentSample.Clear();
        FailureDictionary.Clear();
        break;
      }

      void Report()
      {
        Console.WriteLine($"Runs: {I}");
        Console.WriteLine($"    Failures:         {Failures}");
        Console.WriteLine($"    Exceptions:       {Exceptions}");
        Console.WriteLine($"    Recent Successes: {RecentSampleSuccessRate()} out of {CourseConfidenceSampleCount}");
        Console.WriteLine("    Batch Failures:");
        foreach (var ((Type, Actual), FailureCount) in FailureDictionary.OrderByDescending(Pair => Pair.Value))
          Console.WriteLine($"        {$"{Type}->{Actual.Name}:",-12}{FailureCount}");
        FailureDictionary.Clear();
        Save();
        Console.WriteLine();
      }
    }
  }

  T RandomOneOf<T>(IReadOnlyList<T> Samples)
  {
    return Samples[Random.Next(Samples.Count)];
  }

  List<T> RandomizeOrderOf<T>(IReadOnlyList<T> Samples)
  {
    var Copy = new List<T>(Samples);
    var Result = new List<T>();

    while (Copy.Any())
    {
      var Sample = RandomOneOf(Copy);
      Copy.Remove(Sample);
      Result.Add(Sample);
    }

    return Result;
  }
}

enum ShapeType
{
  Line,
  Rectangle,
  Irregular,
  Circle,
  Star,
  Arc,
  Plus
}

interface ShapeHandler
{
  void Handle();
}

class RectangleHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's a square. I'll draw on it.");
  }
}

class IrregularHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's irregular. I'll put it in a puzzle.");
  }
}

class LineHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's a line. I'll connect it to something.");
  }
}

class CircleHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's a line. I'll connect it to something.");
  }
}

class StarHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's a star. I'll throw it at an enemy.");
  }
}

class ArcHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's an arc. I will use it to build something.");
  }
}

class PlusHandler : ShapeHandler
{
  public void Handle()
  {
    Console.WriteLine("It's a plus. I'll rotate it and use it to multiply.");
  }
}

[Mind]
partial class ShapeClassifyingMind
{
  [Choose]
  public partial Thought<ShapeHandler, ChooseFeedback<ShapeHandler>> ChooseHandlerFor(Shape Shape,
    ShapeHandlerCategory Handlers);
}

[CognitiveCategory<ShapeHandler, ShapeHandlerDescriptor>(3)]
partial class ShapeHandlerCategory;

[CognitiveData]
partial class Shape
{
  public const byte PointCount = 50;
  [CognitiveDataCount(PointCount)] public Point[] Points { get; set; } = new Point[PointCount];

  public Shape Normalize()
  {
    var HotPoints = Points.Where(P => P.IsHot).ToArray();
    var ColdPoints = Points.Where(P => !P.IsHot).ToArray();

    if (!HotPoints.Any())
      return this;

    var MinX = HotPoints.Select(P => P.X).Min();
    var MinY = HotPoints.Select(P => P.Y).Min();
    var MaxX = HotPoints.Select(P => P.X).Max();
    var MaxY = HotPoints.Select(P => P.Y).Max();
    var ScaleX = MaxX - MinX;
    var ScaleY = MaxY - MinY;
    var Scale = 1 / MathF.Max(ScaleX, ScaleY);
    var CenterX = (MinX + MaxX) / 2;
    var CenterY = (MinY + MaxY) / 2;

    if (Scale <= float.Epsilon)
      return this;

    return new()
    {
      Points =
      [
        ..HotPoints.Select(P => P with
        {
          X = (P.X - CenterX) * Scale + .5f,
          Y = (P.Y - CenterY) * Scale + .5f
        }),
        ..ColdPoints.Select(_ => new Point())
      ]
    };
  }
}

[CognitiveData]
partial class ShapeHandlerDescriptor
{
  public ShapeFeatures Features { get; set; } = new();
  public ShapeLimitations Constraints { get; set; } = new();
}

enum Disposition
{
  Prohibited,
  Supported,
  Required
}

[CognitiveData]
partial class ShapeFeatures
{
  public Disposition RoundedSegments { get; set; }
  public Disposition StraightSegments { get; set; }
  public Disposition ConcaveDepressions { get; set; }
  public Disposition ConvexProtrusions { get; set; }
  public Disposition FlatSurfaces { get; set; }
  public Disposition RadialFlatSurfaces { get; set; }
  public Disposition Corners { get; set; }
  public Disposition Open { get; set; }
  public Disposition Closed { get; set; }
}

[CognitiveData]
partial class ShapeLimitations
{
  [CognitiveDataBounds<byte>(0, Shape.PointCount)]
  public byte MinimumVertices { get; set; }

  [CognitiveDataBounds<byte>(0, Shape.PointCount)]
  public byte MaximumVertices { get; set; } = Shape.PointCount;

  [CognitiveDataBounds<byte>(0, 8)] public byte MinimumSymmetricalAxes { get; set; }

  [CognitiveDataBounds<float>(0, MathF.PI * 2)]
  public float MinimumVertexAngle { get; set; }

  [CognitiveDataBounds<float>(0, MathF.PI * 2)]
  public float MaximumVertexAngle { get; set; } = MathF.PI;
}

[CognitiveData]
partial record Point
{
  public bool IsHot { get; set; }
  public float X { get; set; } = .5f;
  public float Y { get; set; } = .5f;
}

static class Geometry
{
  static readonly Random Random = new();

  static float RandomScale => Random.NextSingle() * 100f;
  static Vector2 RandomVector => new(RandomScale, RandomScale);

  static Matrix3x2 RandomTransform => CreateTransform(RandomVector, RandomRadians, RandomScale, RandomVector);

  static float RandomRadians => Random.NextSingle() * MathF.PI * 2;

  static Matrix3x2 CreateTransform(Vector2 Center, float RotationRadians, float Scale, Vector2 Translation)
  {
    return
      Matrix3x2.CreateTranslation(-Center) *
      Matrix3x2.CreateRotation(RotationRadians) *
      Matrix3x2.CreateScale(Scale) *
      Matrix3x2.CreateTranslation(Translation);
  }

  static IEnumerable<Point> AddNoise(IEnumerable<Point> Points, float Radius)
  {
    return Points.Select(P => AddNoise(P, Radius));
  }

  static IEnumerable<Point> Transform(IEnumerable<Point> Points, Matrix3x2 Transform)
  {
    return Points.Select(P => TransformPoint(P, Transform));
  }

  static Point AddNoise(Point Point, float Radius)
  {
    var Direction = RandomRadians;
    var Length = Random.NextSingle() * Radius;

    return Point with
    {
      X = Point.X + MathF.Sin(Direction) * Length,
      Y = Point.Y + MathF.Cos(Direction) * Length
    };
  }

  static Point TransformPoint(Point Point, Matrix3x2 Transform)
  {
    var Vector = new Vector2(Point.X, Point.Y);
    var NewVector = Vector2.Transform(Vector, Transform);

    return Point with {X = NewVector.X, Y = NewVector.Y};
  }

  public static IEnumerable<Point> PrepareForUseInTraining(IEnumerable<Point> Points)
  {
    return NormalizePointsArraySize(Transform(AddNoise(Points, 0.01f), RandomTransform));
  }

  static IEnumerable<Point> NormalizePointsArraySize(IEnumerable<Point> Noisy2)
  {
    var PossiblyNoisy = Noisy2.Take(Shape.PointCount);
    List<Point> Result = [.. PossiblyNoisy];
    Result.AddRange(Enumerable.Repeat(new Point(), Shape.PointCount - Result.Count));
    return Result;
  }

  public static List<Point> MakeLine()
  {
    return MakeLine(new(0, 0), new(0, 1), Random.Next(20, Shape.PointCount + 1), false);
  }

  public static List<Point> MakeLine(Vector2 From, Vector2 To, int Samples, bool SkipFrom)
  {
    var Result = new List<Point>();
    var SegmentLength = (To - From) / (Samples - 1);

    for (var I = SkipFrom ? 1 : 0; I < Samples; ++I)
    {
      var Vector = From + SegmentLength * I;

      Result.Add(new() {IsHot = true, X = Vector.X, Y = Vector.Y});
    }

    return Result;
  }

  public static List<Point> MakeIrregular()
  {
    const int MinPoints = 3;
    const int MaxPoints = 9;
    const float NoiseStrength = 0.5f;
    const float MinimumNoise = 0.1f;
    var VerticesCount = Random.Next(MinPoints, MaxPoints + 1);

    var BaseRadius = 0.5f;

    var Center = new Vector2(0, 0);
    var Points = new List<Point>();

    var VertexBaseStep = MathF.PI * 2 / VerticesCount;

    Vector2 GetVertex(int Index)
    {
      var Angle = Index * VertexBaseStep + VertexBaseStep * (.9f + Random.NextSingle() * .2f);
      var Direction = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle));
      var RawNoise = Random.NextSingle() - 0.5f;
      var Noise = RawNoise * NoiseStrength + MathF.Sign(RawNoise) * MinimumNoise;
      var Radius = BaseRadius + Noise;
      return Center + Direction * Radius;
    }

    var FirstVertex = GetVertex(0);
    var LastVertex = FirstVertex;

    for (var I = 1; I < VerticesCount; I++)
    {
      var ThisVertex = GetVertex(I);

      Points.AddRange(MakeLine(LastVertex, ThisVertex, Random.Next(3, 7), true));
      LastVertex = ThisVertex;
    }

    Points.AddRange(MakeLine(LastVertex, FirstVertex, Random.Next(3, 7), true));

    return Points;
  }

  public static List<Point> MakeBox()
  {
    var AspectRatio = .5f + Random.NextSingle();
    var LinePartCount = 4 + Random.Next(7);
    var Result = new List<Point>();
    Result.AddRange(MakeLine(new(0, 0), new(AspectRatio, 0), LinePartCount, true));
    Result.AddRange(MakeLine(new(AspectRatio, 0), new(AspectRatio, 1), LinePartCount, true));
    Result.AddRange(MakeLine(new(AspectRatio, 1), new(0, 1), LinePartCount, true));
    Result.AddRange(MakeLine(new(0, 1), new(0, 0), LinePartCount, true));

    return Result;
  }

  public static List<Point> MakeStar()
  {
    var Count = 3 + Random.Next(5);
    var SpokeRadianStep = MathF.PI * 2 / Count;
    var SpokeRadianHalfStep = SpokeRadianStep / 2;
    var SpokeLength = 1.1f + Random.NextSingle();
    var PitLength = .5f + Random.NextSingle() * .25f;
    var Result = new List<Point>();
    var SegmentSamples = 4;

    foreach (var I in Enumerable.Range(0, Count))
    {
      var ThisSpokeVertex = GetVertex(I * SpokeRadianStep) * SpokeLength;
      var ThisPitVertex = GetVertex(I * SpokeRadianStep + SpokeRadianHalfStep) * PitLength;
      var NextSpokeVertex = GetVertex((I + 1) * SpokeRadianStep) * SpokeLength;

      Result.AddRange(MakeLine(ThisSpokeVertex, ThisPitVertex, SegmentSamples, true));
      Result.AddRange(MakeLine(ThisPitVertex, NextSpokeVertex, SegmentSamples, true));
    }

    return Result;

    Vector2 GetVertex(float Radians)
    {
      return new(MathF.Sin(Radians), MathF.Cos(Radians));
    }
  }

  public static List<Point> MakePlus()
  {
    var SegmentsPerLine = 3 + Random.Next(2);
    var AspectRatio = .5f + Random.NextSingle();
    var XCutFactor = .1f + Random.NextSingle() * .2f;
    var YCutFactor = .1f + Random.NextSingle() * .2f;
    var XCut = AspectRatio * XCutFactor;
    var YCut = YCutFactor;
    var Result = new List<Point>();
    Result.AddRange(MakeLine(new(XCut, YCut), new(XCut, 0), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 0), new(AspectRatio - XCut, 0), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 0), new(AspectRatio - XCut, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, YCut), new(AspectRatio, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio, YCut), new(AspectRatio, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio, 1 - YCut), new(AspectRatio - XCut, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 1 - YCut), new(AspectRatio - XCut, 1), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(AspectRatio - XCut, 1), new(XCut, 1), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 1), new(XCut, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(XCut, 1 - YCut), new(0, 1 - YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(0, 1 - YCut), new(0, YCut), SegmentsPerLine, true));
    Result.AddRange(MakeLine(new(0, YCut), new(XCut, YCut), SegmentsPerLine, true));
    return Result;
  }

  public static List<Point> MakeArc()
  {
    var MinLength = .25f;
    var Length = MinLength + RandomRadians * (.5f - MinLength);
    return MakeArc(0, Length, Random.Next(20, 50), false);
  }

  public static List<Point> MakeCircle()
  {
    return MakeArc(0f, MathF.PI * 2, Random.Next(Shape.PointCount / 2, Shape.PointCount), true);
  }

  static List<Point> MakeArc(float StartAngleRadians, float EndAngleRadians, int SampleCount, bool SkipFirst)
  {
    var Center = new Vector2(0, 0);
    var Result = new List<Point>();
    var Step = (EndAngleRadians - StartAngleRadians) / (SampleCount - 1);

    for (var I = SkipFirst ? 1 : 0; I < SampleCount; I++)
    {
      var Angle = StartAngleRadians + I * Step;
      var X = Center.X + MathF.Cos(Angle);
      var Y = Center.Y + MathF.Sin(Angle);

      Result.Add(new() {IsHot = true, X = X, Y = Y});
    }

    return Result;
  }

  public static IEnumerable<Point> GetSimplified(IEnumerable<Point> MakeShapePoints)
  {
    return NormalizePointsArraySize(MakeShapePoints);
  }
}

static class ShapeRenderer
{
  public static void RenderShapeToImage(Shape shape, string outputPath, int imageSize = 256, bool closeShape = true)
  {
    using var bmp = new Bitmap(imageSize, imageSize);
    using var gfx = Graphics.FromImage(bmp);
    gfx.Clear(Color.White);

    using var hotPen = new Pen(Color.Black, 2);
    using var hotPen2 = new Pen(Color.DarkGray, 4);
    using var coldPen = new Pen(Color.LightGray, 1);

    var hotPoints = shape.Points.Where(p => p.IsHot).ToArray();
    var coldPoints = shape.Points.Where(p => !p.IsHot).ToArray();

    // Draw hot-point lines
    for (var i = 0; i < hotPoints.Length - 1; i++)
    {
      var p1 = ToPixel(hotPoints[i], imageSize);
      var p2 = ToPixel(hotPoints[i + 1], imageSize);
      gfx.DrawLine(i % 2 == 0 ? hotPen : hotPen2, p1, p2);
    }

    // Optionally close the shape
    if (closeShape && hotPoints.Length > 2)
    {
      var first = ToPixel(hotPoints[0], imageSize);
      var last = ToPixel(hotPoints[^1], imageSize);
      gfx.DrawLine(hotPen, last, first);
    }

    // Optionally draw cold points
    foreach (var point in coldPoints)
    {
      var pixel = ToPixel(point, imageSize);
      gfx.DrawEllipse(coldPen, pixel.X - 1, pixel.Y - 1, 2, 2);
    }

    bmp.Save(outputPath, ImageFormat.Png);
  }

  static PointF ToPixel(Point p, int imageSize)
  {
    return new(
      p.X * imageSize * .9f + imageSize * .05f,
      (1f - p.Y) * imageSize * .9f + imageSize * .05f // invert Y-axis for top-down image
    );
  }
}