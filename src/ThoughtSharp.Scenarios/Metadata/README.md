# ThoughtSharp.Generator

This package generates provides the interface that test/training assemblies will use to express behaviors and training curricula.

Define your behaviors from a business perspective:

```CSharp
[Capability]
public class Calculations(FizzBuzzMind Mind)
{
  // ...

  [Behavior]
  public void Fizz()
  {
    var Input = AnyByteDivisibleBy(FizzFactor);

    var Result = Reasoning.CalculateStepValue(Input, Surface);

    Assert.That(Result).ProducedCallsOn(Surface,
      S => S.Fizz()
    );
  }
}
```

Then define training curricula to imbue your neural network(s) with behavior:

```CSharp
[Curriculum]
public static class FizzBuzzTrainingPlan
{
  [Phase(1)]
  [ConvergenceStandard(Fraction = .8, Of = 200)]
  [Include(typeof(Calculations))]
  public class InitialSteps;

  [Phase(2)]
  [ConvergenceStandard(Fraction = .98, Of = 500)]
  [Include(typeof(Calculations))]
  public class FocusedTraining;

  [Phase(3)]
  [ConvergenceStandard(Fraction = 1, Of = 50)]
  [Include(typeof(Calculations))]
  [Include(typeof(Solution))]
  public class FinalTraining;
}
```

## Installation

```bash
dotnet add package ThoughtSharp.Scenarios
```