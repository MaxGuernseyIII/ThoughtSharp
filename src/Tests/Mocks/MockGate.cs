using FluentAssertions;
using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockGate : Gate
{
  public Queue<bool> Answers = [];

  public MockGate(params IEnumerable<bool> Answers)
  {
    foreach (var Answer in Answers) 
      this.Answers.Enqueue(Answer);
  }

  public bool IsOpen
  {
    get
    {
      Answers.Count.Should().BeGreaterThan(0);
      return Answers.Dequeue();
    }
  }
}