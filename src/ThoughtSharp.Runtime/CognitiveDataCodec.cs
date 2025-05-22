namespace ThoughtSharp.Runtime;

public interface CognitiveDataCodec<T>
{
  int Length { get; }

  void EncodeTo(T ObjectToEncode, Span<float> Target);
  T DecodeFrom(ReadOnlySpan<float> Source);
}