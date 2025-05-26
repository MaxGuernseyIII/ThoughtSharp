namespace ThoughtSharp.Runtime;

public class NullFeedback
{
  NullFeedback() {}

  public static NullFeedback Instance { get; } = new();
}