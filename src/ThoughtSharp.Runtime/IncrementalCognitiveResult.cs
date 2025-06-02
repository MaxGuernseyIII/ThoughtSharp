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

public readonly record struct IncrementalCognitiveResult<TPayload, TFeedback, TDelta, TDeltaFeedback>
  : CognitiveResult<TPayload, TFeedback>
{
  readonly Func<IEnumerable<TDelta>, TPayload> CombineIntoPayload;
  readonly Func<TFeedback, IEnumerable<TDeltaFeedback>> SeparateIntoFeedbackDeltas;

  readonly struct InnerFeedbackSink(
    ImmutableArray<CognitiveResult<TDelta, TDeltaFeedback>> DeltaResults,
    Func<TFeedback, IEnumerable<TDeltaFeedback>> SeparateIntoFeedbackDeltas)
    : FeedbackSink<TFeedback>
  {
    public void TrainWith(TFeedback Feedback)
    {
      var DeltaFeedbackItems = SeparateIntoFeedbackDeltas(Feedback);
      foreach (var (Result, DeltaFeedback) in DeltaResults.Zip(DeltaFeedbackItems))
        Result.FeedbackSink.TrainWith(DeltaFeedback);
    }
  }

  public IncrementalCognitiveResult(Func<IEnumerable<TDelta>, TPayload> CombineIntoPayload,
    Func<TFeedback, IEnumerable<TDeltaFeedback>> SeparateIntoFeedbackDeltas)
  {
    this.CombineIntoPayload = CombineIntoPayload;
    this.SeparateIntoFeedbackDeltas = SeparateIntoFeedbackDeltas;
  }

  ImmutableArray<CognitiveResult<TDelta, TDeltaFeedback>> DeltaResults { get; init; } = [];

  public TPayload Payload => CombineIntoPayload(DeltaResults.Select(Result => Result.Payload));

  public FeedbackSink<TFeedback> FeedbackSink => new InnerFeedbackSink(DeltaResults, SeparateIntoFeedbackDeltas);

  public IncrementalCognitiveResult<TPayload, TFeedback, TDelta, TDeltaFeedback> Including(
    CognitiveResult<TDelta, TDeltaFeedback> Increment)
  {
    return this with {DeltaResults = [..DeltaResults, Increment]};
  }
}