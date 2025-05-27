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

public class InferenceFeedback(Inference Target)
{
  public void OutputShouldHaveBeen(float[] ExpectedOutput)
  {
    Target.Train(ExpectedOutput);
  }
}

public class MakeFeedback<TMade>(InferenceFeedback Underlying, Func<TMade, float[]> ToExpectedOutput)
  where TMade : CognitiveData<TMade>
{
  public void ResultShouldHaveBeen(TMade Expected)
  {
    var Buffer = ToExpectedOutput(Expected);
    Underlying.OutputShouldHaveBeen(Buffer);
  }
}

public class BoxedBool
{
  public bool Value { get; set; }
}

public class UseFeedback<TSurface>(TSurface Mock, Action<bool> Commit)
{
  public delegate void TrainingMethod(TSurface Mock, BoxedBool ShouldHaveRequestedMore);

  public void ExpectationsWere(TrainingMethod Configure)
  {
    var ShouldHaveRequestedMore = new BoxedBool();
    Configure(Mock, ShouldHaveRequestedMore);

    Commit(ShouldHaveRequestedMore.Value);
  }
}

public class AsyncUseFeedback<TSurface>(TSurface Mock, Action<bool> Commit)
{
  public delegate Task TrainingMethod(TSurface Mock, BoxedBool ShouldHaveRequestedMore);

  public void ExpectationsWere(TrainingMethod Configure)
  {
    var ShouldHaveRequestedMore = new BoxedBool();
    Configure(Mock, ShouldHaveRequestedMore);

    Commit(ShouldHaveRequestedMore.Value);
  }
}