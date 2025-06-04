namespace ThoughtSharp.Scenarios.Model;

public record FalseGate : Gate
{
  public bool IsOpen => false;
}