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

public static class AccumulatedUseFeedback
{
  public static FeedbackSource<AccumulatedUseFeedbackConfigurator<T>, AccumulatedUseSemanticFeedback<T>> GetSource<T>()
  {
    var Configurator = new AccumulatedUseFeedbackConfigurator<T>();
    return new(Configurator, Configurator.CreateFeedback);
  }
}

public sealed record AccumulatedUseSemanticFeedback<TSurface>(params ImmutableArray<SemanticFeedbackSink<UseSemanticFeedbackMethod<TSurface>>> Steps) : SemanticFeedbackSink<IEnumerable<Action<TSurface>>>
{
  public void TrainWith(params IEnumerable<Action<TSurface>> Actions)
  {
    if (!Actions.Any())
      Actions = [delegate { }];

    var ActionsArray = Actions.ToImmutableArray();

    (Action<TSurface> Action, bool RequiresMore)[] Expectations = [.. ActionsArray[..^1].Select(A => (A, true)), (ActionsArray[^1], false)];
    ImmutableArray<(SemanticFeedbackSink<UseSemanticFeedbackMethod<TSurface>> Feedback, (Action<TSurface> Action, bool IstLast))> Assertions = [..Steps.Zip(Expectations)];

    foreach (var (Feedback, (Action, RequiresMore)) in Assertions)
    {
      Feedback.TrainWith((Surface, More) =>
      {
        Action(Surface);
        More.Value = RequiresMore;
      });
    }
  }

  public bool Equals(AccumulatedUseSemanticFeedback<TSurface>? Other)
  {
    if (Other is null) return false;
    if (ReferenceEquals(this, Other)) return true;
    return Steps.SequenceEqual(Other.Steps);
  }

  public override int GetHashCode()
  {
    return Steps.GetHashCode();
  }
}