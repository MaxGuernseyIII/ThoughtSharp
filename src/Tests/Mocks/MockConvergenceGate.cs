using FluentAssertions;
using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockConvergenceGate : ConvergenceGate
{
  public Queue<bool> Answers = [];

  public bool IsGateCleared()
  {
    Answers.Count.Should().BeGreaterThan(0);
    return Answers.Dequeue();
  }
}