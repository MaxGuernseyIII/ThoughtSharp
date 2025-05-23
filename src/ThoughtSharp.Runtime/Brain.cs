namespace ThoughtSharp.Runtime;

interface Brain
{
  Inference MakeInference(float[] Parameters);
}