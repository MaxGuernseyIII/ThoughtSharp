namespace ThoughtSharp.Runtime;

/// <summary>
/// Used to configure how the final layer of a built brain isolates features.
/// </summary>
/// <param name="Stream">The stream of isolation boundaries.</param>
/// <param name="Base">The base address that is added to all write operations.</param>
public class IsolationBoundariesWriter(IsolationBoundaryStream Stream, int Base = 0)
{
  /// <summary>
  /// Creates a new writer over the same stream with a base that has been adjusted by <see cref="Base"/>.
  /// </summary>
  /// <param name="Offset">The offset to add to the base.</param>
  /// <returns>A new writer configured as specified.</returns>
  public IsolationBoundariesWriter AddOffset(int Offset)
  {
    return new(Stream, Base + Offset);
  }

  /// <summary>
  /// Records an isolation boundary specified in the local address space of this writer.
  /// </summary>
  /// <param name="LocalBoundary">The boundary (relative to this writer) to write.</param>
  public void Write(int LocalBoundary)
  {
    Stream.WriteBoundary(Base + LocalBoundary);
  }
}