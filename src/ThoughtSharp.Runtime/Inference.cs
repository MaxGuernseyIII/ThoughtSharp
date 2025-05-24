namespace ThoughtSharp.Runtime;

public interface Inference
{
  ReadOnlySpan<float> Result { get; }

  void Incentivize(float Reward, params IReadOnlyList<Range> Ranges);
}