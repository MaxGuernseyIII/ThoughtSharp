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
using System.Text;
using Tests.Mocks;
using ThoughtSharp.Scenarios.Model;

namespace Tests;

static class Any
{
  static readonly Random Core = new();

  public static float Float => Core.NextSingle();

  public static float FloatLessThanOrEqualTo(float Boundary)
  {
    return Boundary - Float;
  }

  public static float FloatGreaterThanOrEqualTo(float Boundary)
  {
    return Boundary + Float;
  }

  public static float FloatLessThan(float Boundary)
  {
    float Candidate;

    do
    {
      Candidate = Boundary - Float;
    } while (Candidate >= Boundary);

    return Candidate;
  }

  public static float FloatGreaterThan(float Boundary)
  {
    float Candidate;

    do
    {
      Candidate = Boundary + Float;
    } while (Candidate <= Boundary);

    return Candidate;
  }

  public static float FloatWithin(float Target, float Epsilon)
  {
    var Range = 2 * Epsilon;
    var Noise = Float * Range - Epsilon;

    return Target + Noise;
  }

  public static float FloatOutsideOf(float Target, float Epsilon)
  {
    return Bool ? FloatGreaterThan(Target + Epsilon) : FloatLessThan(Target - Epsilon);
  }

  public static long Long => Core.Next();
  public static bool Bool => Core.Next(2) == 1;

  public static float PositiveOrNegativeFloat => Core.NextSingle() * 2 - 1;
  public static string NormalString => Guid.NewGuid().ToString("n");

  public static int Int(int Boundary1, int Boundary2)
  {
    var Min = Math.Min(Boundary1, Boundary2);
    var Max = Math.Max(Boundary1, Boundary2);
    var Gap = 1 + (Max - Min);

    return Min + Core.Next(Gap);
  }

  public static int Int()
  {
    return Int(1, 100);
  }

  public static int IntOtherThan(int Antagonist)
  {
    int Candidate;

    do
    {
      Candidate = Int();
    } while (Candidate == Antagonist);

    return Candidate;
  }

  public static float[] FloatArray()
  {
    return FloatArray(Int(1, 3));
  }

  public static string ASCIIString(int Length)
  {
    var ResultBuilder = new StringBuilder();

    foreach (var _ in Enumerable.Range(0, Length)) ResultBuilder.Append((char) Core.Next(128));

    return ResultBuilder.ToString().TrimEnd(['\0']);
  }

  public static string StringWithBitsLength(int Length)
  {
    var ResultBuilder = new StringBuilder();

    foreach (var _ in Enumerable.Range(0, Length / (sizeof(char) * 8)))
      ResultBuilder.Append((char) Core.Next(char.MaxValue));

    return ResultBuilder.ToString().TrimEnd((char) 0);
  }

  public static T EnumValue<T>()
    where T : struct, Enum
  {
    return Of(Enum.GetValues<T>());
  }

  public static T Of<T>(IReadOnlyList<T> Options)
  {
    return Options[Core.Next(Options.Count)];
  }

  public static float[] FloatArray(int Count)
  {
    var Result = new float[Count];

    for (var I = 0; I < Count; ++I)
      Result[I] = Float;

    return Result;
  }

  public static ImmutableArray<(int Amount, bool Result)> ConvergenceRecord(int Amount)
  {
    var Result = new List<(int, bool)>();

    while (Amount > 0)
    {
      var ChunkSize = Any.Int(1, Amount);

      Result.Add((ChunkSize, Any.Bool));

      Amount -= ChunkSize;
    }

    return [..Result];
  }

  public static ImmutableArray<MockRunnable> MockRunnables()
  {
    return [..Enumerable.Range(0, Int(1, 5)).Select(_ => new MockRunnable())];
  }

  public static TrainingMetadata TrainingMetadata()
  {
    return new()
    {
      MaximumAttempts = Any.Int(1, 10),
      SampleSize = Any.Int(1, 10),
      SuccessFraction = Any.Float,
      MinimumDynamicWeight = Any.Float,
      MaxinimumDynamicWeight = Any.Float
    };
  }

  public static IReadOnlyList<T> ListOf<T>(Func<T> MakeItem, int Minimum, int Maximum)
  {
    var Count = Any.Int(Minimum, Maximum);
    var List = new List<T>();
    foreach (var _ in Enumerable.Range(0, Count))
    {
      List.Add(MakeItem());
    }

    return List;
  }

  public static char Char => (char)Any.Int(0, char.MaxValue);

  public static int IndexOf<T>(IReadOnlyList<T> List)
  {
    return Int(0, List.Count - 1);
  }
}