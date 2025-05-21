using ThoughtSharp.Runtime;

namespace Tests.Mocks;

public class MockLengthCodec<T>(int Length) : ThoughtDataCodec<T>
{
  public int Length { get; } = Length;
  public void EncodeTo(T ObjectToEncode, Span<float> Target)
  {
    throw new NotImplementedException();
  }

  public T DecodeFrom(ReadOnlySpan<float> Source)
  {
    throw new NotImplementedException();
  }
}