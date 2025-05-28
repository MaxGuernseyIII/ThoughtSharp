// MIT License
// 
// Copyright (c) 2025-2025 Hexagon Software LLC
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

namespace ThoughtSharp.Runtime;

public class ChooseFeedback<TSelectable>(InferenceFeedback Underlying, IReadOnlyList<TSelectable> Options, Func<ushort, IReadOnlyList<(int, LossRule)>> ToExpectedOutput)
{
  public static FeedbackSource<ChooseFeedbackConfigurator<TSelectable>, ChooseFeedback<TSelectable>> GetSource<TOutput>(IReadOnlyList<TSelectable> Options, Func<ushort, TOutput> GetOutputObject)
    where TOutput : CognitiveData<TOutput>
  {
    var Configurator = new ChooseFeedbackConfigurator<TSelectable>(Options, I =>
    {
      var Stream = new LossRuleStream();
      var W = new LossRuleWriter(Stream, 0);
      var Output = GetOutputObject(I);
      Output.WriteAsLossRules(W);
      
      return Stream.PositionRulePairs;
    });
    return new(Configurator, Configurator.Create);
  }

  public void SelectionShouldHaveBeen(TSelectable Payload)
  {
    var ExpectedOutput = ToExpectedOutput((ushort) Options.ToList().IndexOf(Payload));
    Underlying.ApplyLoses(ExpectedOutput);
  }
}

public class ChooseFeedbackConfigurator<TSelectable>(IReadOnlyList<TSelectable> Options, Func<ushort, IReadOnlyList<(int, LossRule)>> ToLossRules)
{
  public Inference FinalInference { get; set; } = null!;
  public ChooseFeedback<TSelectable> Create()
  {
    return new(new(FinalInference), Options, ToLossRules);
  }
}