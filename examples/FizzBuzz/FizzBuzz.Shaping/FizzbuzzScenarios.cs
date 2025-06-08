// MIT License
// 
// Copyright (c) 2024-2024 Producore LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text;
using FizzBuzz.Cognition;
using JetBrains.Annotations;
using ThoughtSharp.Scenarios;

namespace FizzBuzz.Shaping;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FizzbuzzScenarios
{
  public static Func<byte, bool> IsDivisibleBy(int Denominator)
  {
    return B => B % Denominator == 0;
  }

  public static Func<byte, bool> IsNotDivisibleBy(int Denominator)
  {
    return B => B % Denominator != 0;
  }

  record MockActionSurface : FizzBuzzTerminal
  {
    readonly StringBuilder ContentBuilder = new();

    public void WriteNumber(byte ToWrite)
    {
      ContentBuilder.Append(ToWrite);
    }

    public void Fizz()
    {
      ContentBuilder.Append("fizz");
    }

    public void Buzz()
    {
      ContentBuilder.Append("buzz");
    }

    public virtual bool Equals(MockActionSurface? Other)
    {
      if (Other is null) return false;
      if (ReferenceEquals(this, Other)) return true;
      return ContentBuilder.Equals(Other.ContentBuilder);
    }

    public override string ToString()
    {
      return $"surface:<{ContentBuilder}>";
    }

    public override int GetHashCode()
    {
      return ContentBuilder.GetHashCode();
    }
  }

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  [Capability]
  public class Calculations(FizzBuzzMind Mind)
  {
    const int FizzFactor = 3;
    const int BuzzFactor = 5;

    readonly FizzBuzzHybridReasoning Reasoning = new(Mind);
    readonly MockActionSurface Surface = new();

    [Behavior]
    public void Fizz()
    {
      var Input = TrainingData.Byte
        .Where(IsDivisibleBy(FizzFactor))
        .Where(IsNotDivisibleBy(BuzzFactor))
        .First();

      var Result = Reasoning.CalculateStepValue(Input, Surface);

      Assert.That(Result).ProducedCallsOn(Surface,
        S => S.Fizz()
      );
    }

    [Behavior]
    public void Buzz()
    {
      var Input = TrainingData.Byte
        .Where(IsDivisibleBy(BuzzFactor))
        .Where(IsNotDivisibleBy(FizzFactor))
        .First();

      var Result = Reasoning.CalculateStepValue(Input, Surface);

      Assert.That(Result).ProducedCallsOn(Surface,
        S => S.Buzz()
      );
    }

    [Behavior]
    public void FizzBuzz()
    {
      var Input = TrainingData.Byte
        .Where(IsDivisibleBy(BuzzFactor * FizzFactor))
        .First();

      var Result = Reasoning.CalculateStepValue(Input, Surface);

      Assert.That(Result).ProducedCallsOn(Surface,
        S => S.Fizz(),
        S => S.Buzz()
      );
    }

    [Behavior]
    public void WriteValue()
    {
      var Input = TrainingData.Byte
        .Where(IsNotDivisibleBy(BuzzFactor))
        .Where(IsNotDivisibleBy(FizzFactor))
        .First();

      var Result = Reasoning.CalculateStepValue(Input, Surface);

      Assert.That(Result).ProducedCallsOn(Surface,
        S => S.WriteNumber(Input)
      );
    }
  }

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  [Dependency(typeof(Calculations))]
  [Capability]
  public class Solution(FizzBuzzMind Mind)
  {
    const string ExpectedFinalOutput =
      "1 2 fizz 4 buzz fizz 7 8 fizz buzz 11 fizz 13 14 fizzbuzz 16 17 fizz 19 buzz fizz 22 23 fizz buzz 26 fizz 28 29 fizzbuzz 31 32 fizz 34 buzz fizz 37 38 fizz buzz 41 fizz 43 44 fizzbuzz 46 47 fizz 49 buzz fizz 52 53 fizz buzz 56 fizz 58 59 fizzbuzz 61 62 fizz 64 buzz fizz 67 68 fizz buzz 71 fizz 73 74 fizzbuzz 76 77 fizz 79 buzz fizz 82 83 fizz buzz 86 fizz 88 89 fizzbuzz 91 92 fizz 94 buzz fizz 97 98 fizz buzz";

    readonly FizzBuzzHybridReasoning Reasoning = new(Mind);

    [Behavior]
    public void FullFizzBuzz()
    {
      var Result = Reasoning.DoFullFizzBuzz();

      Result.ShouldBe(ExpectedFinalOutput);
    }
  }
}