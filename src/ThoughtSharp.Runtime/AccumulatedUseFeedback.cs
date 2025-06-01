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
  public static FeedbackSource<AccumulatedUseFeedbackConfigurator<T>, AccumulatedUseFeedback<T>> GetSource<T>()
  {
    var Configurator = new AccumulatedUseFeedbackConfigurator<T>();
    return new(Configurator, Configurator.CreateFeedback);
  }
}

public class AccumulatedUseFeedbackConfigurator<T>
{
  readonly List<UseFeedback<T>> Steps = [];

  public AccumulatedUseFeedback<T> CreateFeedback()
  {
    return new([..Steps]);
  }

  public void AddStep(UseFeedback<T> Step)
  {
    Steps.Add(Step);
  }
}

public class AccumulatedUseFeedback<T>(ImmutableArray<UseFeedback<T>> Steps)
{
  public void UseShouldHaveBeen(params IEnumerable<Action<T>> Actions)
  {
    if (!Actions.Any())
    {
      Steps.First().ExpectationsWere((_, More) => More.Value = false);
      return;
    }

    Action<T>[] ActionsArray = [..Actions];

    var PrecedingSteps = Steps[..^1];
    var Index = 0;
    foreach (var PrecedingStep in PrecedingSteps)
    {
      var ThisIndex = Index;
      PrecedingStep.ExpectationsWere((M, More) =>
      {
        ActionsArray[ThisIndex](M);
        More.Value = true;
      });
      Index++;
    }
    var FinalStep = Steps[^1];
    FinalStep.ExpectationsWere((M, More) =>
    {
      ActionsArray[Index](M);
      More.Value = false;
    });
  }
}