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

public class UseFeedbackSink<TSurface>(TSurface Mock, Action<bool> Commit)
  : FeedbackSink<UseFeedbackMethod<TSurface>>
{
  public void TrainWith(UseFeedbackMethod<TSurface> Configure)
  {
    var ShouldHaveRequestedMore = new BoxedBool();
    Configure(Mock, ShouldHaveRequestedMore);

    Commit(ShouldHaveRequestedMore.Value);
  }
}

public class BatchUseFeedbackSink<TSurface>(
  IReadOnlyList<(TSurface Mock, Action<bool> CommitOne)> TimeSequences,
  Action CommitBatch)
  : FeedbackSink<IReadOnlyList<UseFeedbackMethod<TSurface>>>
{
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