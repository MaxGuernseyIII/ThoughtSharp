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

using JetBrains.Annotations;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios;

[PublicAPI]
public class BatchCognitiveResultAssertionContext<TResultFeedback>(
  CognitiveResult<IReadOnlyList<TResultFeedback>, IReadOnlyList<TResultFeedback>> Subject)
{
  public void Is(IReadOnlyList<TResultFeedback> Expected)
  {
    Is(Expected, C => C.ExpectEqual(D => D));
  }

  public void Is(IReadOnlyList<TResultFeedback> Expected, Action<ObjectComparison<TResultFeedback>> Assertion)
  {
    Subject.FeedbackSink.TrainWith(Expected);

    Subject.Payload.Count.ShouldBe(Expected.Count, "Expected {0} batch(es) but found {1}");

    foreach (var (ActualItem, ExpectedItem) in Subject.Payload.Zip(Expected))
      Assertion(new(ExpectedItem, ActualItem));
  }

  public ObjectBatchConvergenceAssertionContext<TResultFeedback> ConvergesOn()
  {
    return new(Subject);
  }
}