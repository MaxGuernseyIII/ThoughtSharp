using System.Collections.Immutable;
using System.Linq.Expressions;

namespace ThoughtSharp.Runtime;

public static class AccumulatedUseFeedback
{
  public static FeedbackSource<AccumulatedUseFeedbackConfigurator<T>, AccumulatedUseFeedback<T>> GetSource<T>()
  {
    var Configurator = new AccumulatedUseFeedbackConfigurator<T>();
    return new(Configurator, Configurator.CreateFeedback);
  }

  public static void NoOp<T>(T _)
  {
  }
}

public class AccumulatedUseFeedbackConfigurator<T>
{
  readonly List<UseFeedback<T>> Steps = [];

  public AccumulatedUseFeedback<T> CreateFeedback()
  {
    return new([..Steps]);
  }

  public void AddStep(UseFeedback<T> Step)
  {
    Steps.Add(Step);
  }
}

public class AccumulatedUseFeedback<T>(ImmutableArray<UseFeedback<T>> Steps)
{
  public void UseShouldHaveBeen(params IEnumerable<Expression<Action<T>>> Actions)
  {
    Steps.First().ExpectationsWere((_, More) => More.Value = false);
  }
}