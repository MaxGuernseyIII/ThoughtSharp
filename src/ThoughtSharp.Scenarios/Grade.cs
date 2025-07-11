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

using System.Collections.Immutable;

namespace ThoughtSharp.Scenarios;

public sealed record Grade
{
  public required float Score { get; init; }
  public required ImmutableArray<string> Annotations { get; init; }

  public bool Equals(Grade? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return Score.Equals(Other.Score) && Annotations.SequenceEqual(Other.Annotations);
  }

  public override int GetHashCode()
  {
    var HashCode = new HashCode();
    HashCode.Add(Score);
    foreach (var Annotation in Annotations) 
      HashCode.Add(Annotation);
    return HashCode.ToHashCode();
  }

  public static Grade Merge(Summarizer Summarizer, IEnumerable<Grade> Grades)
  {
    return new()
    {
      Score = Summarizer.Summarize([..Grades.Select(G => G.Score)]), 
      Annotations = [..Grades.SelectMany(G => G.Annotations)]
    };
  }
}