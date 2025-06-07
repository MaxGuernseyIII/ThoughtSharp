using ThoughtSharp.Scenarios.Model;

namespace Tests.Mocks;

class MockNode : ScenariosModelNode
{
  public string Name { get; set; } = "";

  public IEnumerable<ScenariosModelNode> ChildNodes => throw new NotImplementedException();

  public TResult Query<TResult>(ScenariosModelNodeVisitor<TResult> Visitor)
  {
    throw new NotImplementedException();
  }
}