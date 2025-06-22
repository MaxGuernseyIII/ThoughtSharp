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

using System.Collections;

namespace ThoughtSharp.Runtime;

public sealed record Batch<T>
{
  readonly T[,] Grid;

  Batch(T[,] Grid)
  {
    this.Grid = Grid;
  }

  public readonly struct TimeFirstView(Batch<T> Into)
    : IEnumerable<TimeFirstStepView>
  {
    public TimeFirstStepView this[int T] => new(Into, T);

    public IEnumerator<TimeFirstStepView> GetEnumerator()
    {
      foreach (var T in Enumerable.Range(0, Into.StepCount))
        yield return this[T];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  public readonly struct TimeFirstStepView(Batch<T> Into, int TimeStepNumber)
  {
    public TimeFirstThenBatchView InSequence => new(Into, TimeStepNumber);
  };

  public readonly struct TimeFirstThenBatchView(Batch<T> Into, int TimeStepNumber)
    : IEnumerable<T>
  {
    public T this[int SequenceNumber] => Into.Grid[TimeStepNumber, SequenceNumber];

    public IEnumerator<T> GetEnumerator()
    {
      foreach (var SequenceNumber in Enumerable.Range(0, Into.SequenceCount))
        yield return this[SequenceNumber];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  public TimeFirstView Time => new(this);
  public int StepCount => Grid.GetLength(0);
  public int SequenceCount => Grid.GetLength(1);


  public class Builder(T DefaultValue, int SequenceCount)
  {
    readonly List<T[]> Steps = [];

    public readonly struct StepBuilder(T[] ThisStep)
    {
      public T this[int SequenceNumber]
      {
        get => ThisStep[SequenceNumber];
        set => ThisStep[SequenceNumber] = value;
      }
    }

    public StepBuilder AddStep(params IReadOnlyList<T> PerBatchValues)
    {
      var NewStep = new T[SequenceCount];
      Array.Fill(NewStep, DefaultValue);
      foreach (var I in Enumerable.Range(0, PerBatchValues.Count)) 
        NewStep[I] = PerBatchValues[I];

      Steps.Add(NewStep);

      return new(NewStep);
    }

    public Batch<T> Build()
    {
      var Grid = new T[Steps.Count, SequenceCount];

      foreach (var StepNumber in Enumerable.Range(0, Steps.Count))
      foreach (var SequenceNumber in Enumerable.Range(0, SequenceCount))
        Grid[StepNumber, SequenceNumber] = Steps[StepNumber][SequenceNumber];

      return new(Grid);
    }
  }
}