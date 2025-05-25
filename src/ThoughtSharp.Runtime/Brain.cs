namespace ThoughtSharp.Runtime;

public interface Brain : IDisposable
{
  Inference MakeInference(float[] Parameters);
}