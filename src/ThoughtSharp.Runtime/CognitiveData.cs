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

namespace ThoughtSharp.Runtime;

public interface CognitiveData<out T> where T : CognitiveData<T>
{
  static abstract int Length { get; }

  void MarshalTo(Span<float> Target);
  void WriteAsLossRules(LossRuleWriter Target);

  static abstract T UnmarshalFrom(ReadOnlySpan<float> Source);
}

public class LossRuleStream
{
  readonly List<(int At, LossRule Rule)> WriteableRules = [];
  public IReadOnlyList<(int At, LossRule Rule)> PositionRulePairs => WriteableRules;

  public void WriteRule(int Offset, LossRule Rule)
  {
    WriteableRules.Add((Offset, Rule));
  }
}

public interface LossRule
{
  int Length { get; }
  U Accept<T, U>(T Prediction, LossRuleVisitor<T, U> Visitor);
}

public interface LossRuleVisitor<in TPrediction, out TTarget>
{
  TTarget Visit(BinaryCrossEntropyWithLogitsLossRule Rule, TPrediction Prediction);
  TTarget Visit(MeanSquareErrorLossRule Rule, TPrediction Prediction);
  TTarget Visit(CrossEntropyLossRule Rule, TPrediction Prediction);
  TTarget Visit(HuberLossRule Rule, TPrediction Prediction);
}

public class OneDimensionalTarget(float[] Target)
{
  public float[] Target { get; } = Target;
  public int Length { get; } = Target.Length;

  bool Equals(OneDimensionalTarget Other)
  {
    return Target.SequenceEqual(Other.Target);
  }

  public override bool Equals(object? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    if (Other.GetType() != GetType()) return false;
    return Equals((OneDimensionalTarget) Other);
  }

  public override int GetHashCode()
  {
    return Target.GetHashCode();
  }
}

public class BinaryCrossEntropyWithLogitsLossRule(float[] Target) : OneDimensionalTarget(Target), LossRule
{
  public U Accept<T, U>(T Prediction, LossRuleVisitor<T, U> Visitor)
  {
    return Visitor.Visit(this, Prediction);
  }


}

public class MeanSquareErrorLossRule(float[] Target) : OneDimensionalTarget(Target), LossRule
{
  public U Accept<T, U>(T Prediction, LossRuleVisitor<T, U> Visitor)
  {
    return Visitor.Visit(this, Prediction);
  }
}

public class HuberLossRule(float[] Target) : OneDimensionalTarget(Target), LossRule
{
  public U Accept<T, U>(T Prediction, LossRuleVisitor<T, U> Visitor)
  {
    return Visitor.Visit(this, Prediction);
  }
}

public class CrossEntropyLossRule(long Index, int Length) : LossRule
{
  public long Index { get; } = Index;
  public int Length { get; } = Length;

  public U Accept<T, U>(T Prediction, LossRuleVisitor<T, U> Visitor)
  {
    return Visitor.Visit(this, Prediction);
  }

  bool Equals(CrossEntropyLossRule Other)
  {
    return Index == Other.Index && Length == Other.Length;
  }

  public override bool Equals(object? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    if (Other.GetType() != GetType()) return false;
    return Equals((CrossEntropyLossRule) Other);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Index, Length);
  }
}

public class LossRuleWriter(LossRuleStream Stream, int Base)
{
  public LossRuleWriter() : this(new(), 0) { }

  public LossRuleStream Stream { get; } = Stream;

  public LossRuleWriter ForOffset(int Offset) => new(Stream, Base + Offset);

  public void WriteLossRule(int Offset, LossRule Rule) => Stream.WriteRule(Base + Offset, Rule);
}