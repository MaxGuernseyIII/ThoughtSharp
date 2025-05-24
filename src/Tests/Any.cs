// MIT License
// 
// Copyright (c) 2024-2024 Hexagon Software LLC
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

namespace Tests;

static class Any
{
  static readonly Random Core = new();

  public static float Float => Core.NextSingle();

  public static long Long => Core.Next();
  public static bool Bool => Core.Next(2) == 1;

  public static int Int(int Boundary1, int Boundary2)
  {
    var Min = Math.Min(Boundary1, Boundary2);
    var Max = Math.Max(Boundary1, Boundary2);
    var Gap = 1 + (Max - Min);

    return Min + Core.Next(Gap);
  }

  public static string ASCIIString(int Length)
  {
    var ResultBuilder = new StringBuilder();

    foreach (var _ in Enumerable.Range(0, Length))
    {
      ResultBuilder.Append((char) Core.Next(128));
    }

    return ResultBuilder.ToString();
  }

  public static string StringWithBitsLength(int Length)
  {
    var ResultBuilder = new StringBuilder();

    foreach (var _ in Enumerable.Range(0, Length / (sizeof(char) * 8)))
    {
      ResultBuilder.Append((char)Core.Next(char.MaxValue));
    }

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
}