namespace ThoughtSharp.Runtime;

public interface ThoughtDataCodec<T>
{
  int Length { get; }

  void EncodeTo(T ObjectToEncode, Span<float> Target);
  T DecodeFrom(ReadOnlySpan<float> Source);
}