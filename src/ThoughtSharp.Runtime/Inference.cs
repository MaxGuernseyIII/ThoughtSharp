namespace ThoughtSharp.Runtime;

public interface Inference
{
  ReadOnlySpan<float> Result { get; }
}