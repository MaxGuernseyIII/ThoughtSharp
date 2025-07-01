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
using FluentAssertions;

namespace Tests;

static class TestDataExtensions
{
  public static ImmutableArray<T> WithOneLess<T>(this ImmutableArray<T> Source)
  {
    return Source.MutateUntilDifferent(S => S.RemoveAt(Any.IndexOf(Source)));
  }

  public static ImmutableArray<T> WithOneMore<T>(this ImmutableArray<T> Source, Func<T> GetNext)
  {
    return Source.MutateUntilDifferent(ToChange => ToChange.Insert(Any.Int(0, ToChange.Length), GetNext()));
  }

  public static ImmutableArray<T> WithOneReplaced<T>(this ImmutableArray<T> Source, Func<T, T> GetReplacement)
  {
    return Source.MutateUntilDifferent(ToChange =>
    {
      var Index = Any.IndexOf(ToChange);
      return ToChange.SetItem(Index, GetReplacement(ToChange[Index]));
    });
  }

  public static ImmutableArray<T> MutateUntilDifferent<T>(
    this ImmutableArray<T> Source, Func<ImmutableArray<T>, ImmutableArray<T>> Mutate)
  {
    var Tries = 0;

    ImmutableArray<T> Candidate;
    do
    {
      Candidate = Mutate(Source);
      (Tries++).Should().BeLessThan(100);
    } while (Candidate.SequenceEqual(Source));

    return Candidate;
  }
}