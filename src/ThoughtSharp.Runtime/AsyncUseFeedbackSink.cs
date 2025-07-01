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

/// <summary>
/// Trains a single asynchronous use operation. This is an adapter over a batch feedback sink (typically <see cref="AsyncBatchUseFeedbackSink{TSurface}"/>) and trains with a batch size of 1.
/// </summary>
/// <param name="Core"></param>
/// <typeparam name="TSurface"></typeparam>
public class AsyncUseFeedbackSink<TSurface>(FeedbackSink<IReadOnlyList<AsyncUseFeedbackMethod<TSurface>>> Core)
  : FeedbackSink<AsyncUseFeedbackMethod<TSurface>>
{
  /// <inheritdoc />
  public void TrainWith(AsyncUseFeedbackMethod<TSurface> Configure)
  {
    Core.TrainWith([Configure]);
  }
}

/// <summary>
/// Trains a parallel set of use outcomes.
/// </summary>
/// <param name="TimeSequences">The training operation for each of the parallel operations taken in separate time sequences.</param>
/// <param name="CommitBatch">The operation that triggers training of the entire batch.</param>
/// <typeparam name="TSurface"></typeparam>
public class AsyncBatchUseFeedbackSink<TSurface>(
  IReadOnlyList<(TSurface Mock, Action<bool> CommitOne)> TimeSequences,
  Action CommitBatch)
  : FeedbackSink<IReadOnlyList<AsyncUseFeedbackMethod<TSurface>>>
{
  /// <inheritdoc />
  public void TrainWith(IReadOnlyList<AsyncUseFeedbackMethod<TSurface>> ConfigureAll)
  {
    foreach (var ((Mock, CommitOne), Configure) in TimeSequences.Zip(ConfigureAll))
    {
      var RequiresMore = new BoxedBool();
      Configure(Mock, RequiresMore).GetAwaiter().GetResult();
      CommitOne(RequiresMore.Value);
    }

    CommitBatch();
  }
}