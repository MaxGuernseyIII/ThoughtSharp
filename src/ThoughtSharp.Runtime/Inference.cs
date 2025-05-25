namespace ThoughtSharp.Runtime;

public interface Inference : IDisposable
{
  ReadOnlySpan<float> Result { get; }

  void Incentivize(float Reward, params IReadOnlyList<Range> Ranges);
}