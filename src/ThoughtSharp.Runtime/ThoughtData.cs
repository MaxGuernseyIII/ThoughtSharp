namespace ThoughtSharp.Runtime
{
  public interface ThoughtData
  {
    static abstract int Length { get; }

    void MarshalTo(Span<float> Target);
    void MarshalFrom(ReadOnlySpan<float> Source);
  }
}
