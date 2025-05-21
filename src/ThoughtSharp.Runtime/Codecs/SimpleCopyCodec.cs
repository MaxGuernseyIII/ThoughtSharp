namespace ThoughtSharp.Runtime.Codecs
{
  public class SimpleCopyCodec : ThoughtDataCodec<float>
  {
    public int Length => 1;

    public void EncodeTo(float ObjectToEncode, Span<float> Target)
    {
      Target[0] = ObjectToEncode;
    }

    public float DecodeFrom(ReadOnlySpan<float> Source)
    {
      return 0;
    }
  }
}
