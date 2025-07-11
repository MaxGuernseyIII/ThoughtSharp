﻿// MIT License
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

namespace ThoughtSharp.Scenarios.Model;

public class ConvergenceTracker(int Length, Summarizer Summarizer)
{
  readonly Summarizer Summarizer = Summarizer;

  readonly int Length = Length;
  readonly Queue<float> Results = new();

  protected bool Equals(ConvergenceTracker Other)
  {
    lock (Results)
    {
      return Results.SequenceEqual(Other.Results) && Length == Other.Length && Equals(Summarizer, Other.Summarizer);
    }
  }

  public override bool Equals(object? Obj)
  {
    if (Obj is null) return false;
    if (ReferenceEquals(this, Obj)) return true;
    if (Obj.GetType() != GetType()) return false;
    return Equals((ConvergenceTracker) Obj);
  }

  public override int GetHashCode()
  {
    lock (Results)
    {
      return HashCode.Combine(Results.Aggregate(0, HashCode.Combine), Length, Summarizer);
    }
  }

  public double MeasureConvergence()
  {
    lock (Results)
    {
      return Summarizer.Summarize([..Results.Concat(Enumerable.Repeat(0f, Length - Results.Count))]);
    }
  }

  public void RecordResult(float Result)
  {
    RecordResults([Result]);
  }

  public void RecordResult(Transcript Transcript)
  {
    RecordResults(Transcript.Grades.Select(Pair => Pair.Score));
  }

  void RecordResults(IEnumerable<float> NewResults)
  {
    lock (Results)
    {
      foreach (var NewResult in NewResults) 
        Results.Enqueue(NewResult);
      while (Results.Count > Length)
        Results.Dequeue();
    }
  }
}