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

public sealed record Batch<T> : AbstractTensor
{
  Batch(ImmutableArray<Sequence> Sequences)
  {
    this.Sequences = Sequences;
  }

  public ImmutableArray<Sequence> Sequences { get; }

  public sealed record Builder
  {
    ImmutableArray<Sequence> Sequences { get; init; } = [];

    public Builder AddSequence(Func<SequenceBuilder, SequenceBuilder> ConfigureBuilder)
    {
      return this with {Sequences = [..Sequences, ConfigureBuilder(new()).Build()]};
    }

    public Batch<T> Build()
    {
      return new(Sequences);
    }

    public sealed record SequenceBuilder()
    {
      ImmutableArray<T> Steps = [];

      public SequenceBuilder AddStep(T NextStep)
      {
        return this with {Steps = [..Steps, NextStep]};
      }

      public Sequence Build()
      {
        return new(Steps);
      }
    }
  }

  public sealed record Sequence(IReadOnlyList<T> Steps)
  {
    public IReadOnlyList<T> Steps { get; } = Steps;

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

  public bool Equals(Batch<T>? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return Sequences.SequenceEqual(Other.Sequences);
  }

  public override int GetHashCode()
  {
    return Sequences.Aggregate(0, HashCode.Combine);
  }
}

public static class Batch
{
  public static class OfTensorData
  {
    public static Batch<TensorData>.Builder Builder => new();
  }
}