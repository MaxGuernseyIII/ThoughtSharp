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
/// Trains a single use operation. Adapts a batch feedback sink (typically <see cref="BatchUseFeedbackSink{TSurface}" />) with a batch size of 1.
/// </summary>
/// <param name="Core">The adapted sink.</param>
/// <typeparam name="TSurface">The type of action surface that was acted upon in the use operation.</typeparam>
public class UseFeedbackSink<TSurface>(FeedbackSink<IReadOnlyList<UseFeedbackMethod<TSurface>>> Core)
  : FeedbackSink<UseFeedbackMethod<TSurface>>
{
  /// <inheritdoc />
  public void TrainWith(UseFeedbackMethod<TSurface> Configure)
  {
    Core.TrainWith([Configure]);
  }
}

/// <summary>
/// Trains a set of parallel use outcomes.
/// </summary>
/// <param name="TimeSequences">The individual use outcomes to be trained.</param>
/// <param name="CommitBatch">The action that forces training of the entire batch.</param>
/// <typeparam name="TSurface">The type of action surface that was acted upon.</typeparam>
public class BatchUseFeedbackSink<TSurface>(
  IReadOnlyList<(TSurface Mock, Action<bool> CommitOne)> TimeSequences,
  Action CommitBatch)
  : FeedbackSink<IReadOnlyList<UseFeedbackMethod<TSurface>>>
{
  /// <inheritdoc />
  public void TrainWith(IReadOnlyList<UseFeedbackMethod<TSurface>> ConfigureAll)
  {
    foreach (var ((Mock, CommitOne), Configure) in TimeSequences.Zip(ConfigureAll))
    {
      var RequiresMore = new BoxedBool();
      Configure(Mock, RequiresMore);
      CommitOne(RequiresMore.Value);
    }

    CommitBatch();
  }
}