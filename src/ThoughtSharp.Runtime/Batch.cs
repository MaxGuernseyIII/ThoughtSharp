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

/// <summary>
/// An abstract representation of a batch of time sequences of something.
/// </summary>
/// <typeparam name="T">The content of each timestep.</typeparam>
public sealed record Batch<T>
{
  Batch(ImmutableArray<Sequence> Sequences)
  {
    this.Sequences = Sequences;
  }

  /// <summary>
  /// Each independent sequence of timesteps in the batch.
  /// </summary>
  public ImmutableArray<Sequence> Sequences { get; }

  /// <summary>
  /// Constructs a <see cref="Batch{T}"/>. This is an immutable object.
  /// </summary>
  public sealed record Builder
  {
    ImmutableArray<Sequence> Sequences { get; init; } = [];

    /// <summary>
    /// Add a sequence to the constructed <see cref="Batch{T}"/>. Does not modify this object.
    /// </summary>
    /// <param name="ConfigureBuilder">The operation that defines the contents of the sequence.</param>
    /// <returns>A new <see cref="Builder"/> that will produce a <see cref="Batch{T}"/> with the additional sequence in it.</returns>
    public Builder AddSequence(Func<SequenceBuilder, SequenceBuilder> ConfigureBuilder)
    {
      return this with {Sequences = [..Sequences, ConfigureBuilder(new()).Build()]};
    }

    /// <summary>
    /// Create the specified <see cref="Batch{T}"/>.
    /// </summary>
    /// <returns>A batch as configured by this object.</returns>
    public Batch<T> Build()
    {
      return new(Sequences);
    }

    /// <summary>
    /// A builder for a sequence of timesteps. This object is immutable.
    /// </summary>
    public sealed record SequenceBuilder()
    {
      ImmutableArray<T> Steps = [];

      /// <summary>
      /// Add a new step to the sequence. This will not change the current builder instance.
      /// </summary>
      /// <param name="NextStep">The new step.</param>
      /// <returns>A new instance of <see cref="SequenceBuilder"/> that includes the additional step.</returns>
      public SequenceBuilder AddStep(T NextStep)
      {
        return this with {Steps = [..Steps, NextStep]};
      }

      /// <summary>
      /// Creates the specified <see cref="Sequence"/>.
      /// </summary>
      /// <returns>The sequence defined by this object.</returns>
      public Sequence Build()
      {
        return new(Steps);
      }
    }
  }

  /// <summary>
  /// Encapsulates a sequence of objects, with each such object representing a timestep in that sequence.
  /// </summary>
  /// <param name="Steps">The timesteps.</param>
  public sealed record Sequence(IReadOnlyList<T> Steps)
  {
    /// <summary>
    /// All the steps in the sequence.
    /// </summary>
    public IReadOnlyList<T> Steps { get; } = Steps;

    /// <inheritdoc />
    public bool Equals(Sequence? Other)
    {
      if (Other is null) return false;
      if (ReferenceEquals(this, Other)) return true;
      return Steps.SequenceEqual(Other.Steps);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return Steps.Aggregate(0, HashCode.Combine);
    }
  }

  /// <inheritdoc />
  public bool Equals(Batch<T>? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return Sequences.SequenceEqual(Other.Sequences);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return Sequences.Aggregate(0, HashCode.Combine);
  }
}

/// <summary>
/// A convenience class for creating common kinds of <see cref="Batch{T}"/>.
/// </summary>
public static class Batch
{
  /// <summary>
  /// A convenience class for creating batches of feature sets (float arrays).
  /// </summary>
  public static class OfFeatureSets
  {
    /// <summary>
    /// A builder for a <see cref="Batch{T}"/> of feature sets.
    /// </summary>
    public static Batch<float[]>.Builder Builder => new();
  }
}