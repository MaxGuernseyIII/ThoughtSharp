using ThoughtSharp.Runtime;

namespace Tests.Mocks;

public class MockFeedbackSink<T> : FeedbackSink<T>
{
  public readonly List<T> RecordedFeedback = [];

  public void TrainWith(T Feedback)
  {
    RecordedFeedback.Add(Feedback);
  }
}