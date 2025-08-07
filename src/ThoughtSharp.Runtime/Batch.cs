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

namespace ThoughtSharp.Runtime;

public sealed record Batch
{
  Batch(ImmutableArray<Sequence> Cases)
  {
    this.Cases = Cases;
  }

  public static class OfTensorData
  {
    public static Builder Builder => new();
  }

  public int TerminalCount => 1;

  public ImmutableArray<Sequence> Cases { get; }

  public sealed record Builder
  {
    ImmutableArray<Sequence> Sequences { get; init; } = [];

    public Builder AddSequence(Func<SequenceBuilder, SequenceBuilder> ConfigureBuilder)
    {
      return this with { Sequences = [.. Sequences, ConfigureBuilder(new()).Build()] };
    }

    public Batch Build()
    {
      return new(Sequences);
    }

    public sealed record SequenceBuilder()
    {
      ImmutableArray<TensorData> Steps = [];

      public SequenceBuilder AddStep(TensorData NextStep)
      {
        return this with { Steps = [.. Steps, NextStep] };
      }

      public Sequence Build()
      {
        return new(Steps);
      }
    }
  }

  public sealed record Sequence(IReadOnlyList<TensorData> Steps)
  {
    public IReadOnlyList<TensorData> Steps { get; } = Steps;

    public bool Equals(Sequence? Other)
    {
      if (Other is null) return false;
      if (ReferenceEquals(this, Other)) return true;
      return Steps.SequenceEqual(Other.Steps);
    }

    public override int GetHashCode()
    {
      return Steps.Aggregate(0, HashCode.Combine);
    }
  }

  public bool Equals(Batch? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return Cases.SequenceEqual(Other.Cases);
  }

  public override int GetHashCode()
  {
    return Cases.Aggregate(0, HashCode.Combine);
  }

  public BatchTerminal GetTerminal(int TerminalNumber)
  {
    return new BatchTerminal(this);
  }
}