using JetBrains.Annotations;

namespace ThoughtSharp.Runtime;

/// <summary>
/// A dummy object used for when feedback is not supported.
/// </summary>
[PublicAPI]
public class NullFeedback
{
  NullFeedback() {}

  /// <summary>
  /// The only instance.
  /// </summary>
  public static NullFeedback Instance { get; } = new();
}