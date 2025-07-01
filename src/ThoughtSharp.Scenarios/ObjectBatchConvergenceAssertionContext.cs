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

using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios;

public sealed record ObjectBatchConvergenceAssertionContext<TSubject>(CognitiveResult<IReadOnlyList<TSubject>, IReadOnlyList<TSubject>> Subject)
{
  CognitiveResult<IReadOnlyList<TSubject>, IReadOnlyList<TSubject>> Subject { get; } = Subject;

  public ObjectBatchConvergenceAssertionContext<TSubject> WithSummarizer(Summarizer Summarizer)
  {
    return this with {Summarizer = Summarizer};
  }

  Summarizer Summarizer { get; init; } = null!;

  public Transcript Target(
    IReadOnlyList<TSubject> Targets, 
    Func<ObjectConvergenceComparison<TSubject>, ObjectConvergenceComparison<TSubject>> SetExpectations)
  {
    Subject.FeedbackSink.TrainWith(Targets);

    var Grades = new List<Grade>();

    foreach (var (Actual, Expected) in Subject.Payload.Zip(Targets))
    {
      var Comparison = SetExpectations(new(Actual, Expected));
      Grades.Add(Grade.Merge(Summarizer, Comparison.Grades));
    }

    return new([..Grades]);
  }
}