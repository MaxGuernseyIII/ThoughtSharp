namespace ThoughtSharp.Runtime;

public class IsolationBoundariesWriter(IsolationBoundaryStream Stream, int Base = 0)
{
  public IsolationBoundariesWriter AddOffset(int Offset)
  {
    return new(Stream, Base + Offset);
  }

  public void Write(int LocalBoundary)
  {
    Stream.WriteBoundary(Base + LocalBoundary);
  }
}