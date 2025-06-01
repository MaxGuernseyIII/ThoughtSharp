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



using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using ThoughtSharp.Scenarios;

[assembly: MindPlace<Tests.SampleScenarios.FizzBuzzScenarios.FizzBuzzMindPlace, Tests.SampleScenarios.FizzBuzzScenarios.FizzBuzzMind, TorchBrain>]

namespace Tests.SampleScenarios;

public static partial class FizzBuzzScenarios
{
  public class FizzBuzzMindPlace(MindPlaceConfig Config) : MindPlace<FizzBuzzMind, TorchBrain>
  {
    public TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.ForTraining<FizzBuzzMind>()
        .UsingParallel(P => P
          .AddLogicPath(16, 4, 8)
          .AddPath(S => S)).Build();
    }

    public void LoadSavedBrain(TorchBrain ToLoad, string Discriminator)
    {
      ToLoad.Load(GetSavePath(Discriminator));
    }

    public void SaveBrain(TorchBrain ToSave, string Discriminator)
    {
      ToSave.Save(GetSavePath(Discriminator));
    }

    string GetSavePath(string Discriminator)
    {
      return $"{Config.Strings["models.path"]}fizzbuzz{Discriminator}.pt";
    }
  }

  [ScenarioSet(ConvergenceThreshold = 1000)]
  public class SimpleScenarios(FizzBuzzMind Mind)
  {
    [Scenario]
    public void Fizz()
    {

    }

    [Scenario]
    public void Buzz()
    {

    }

    [Scenario]
    [TrainingDependency(Scenario = nameof(Fizz), RequiredConfidenceThreshold = .98)]
    [TrainingDependency(Scenario = nameof(Buzz), RequiredConfidenceThreshold = .98)]
    public void FizzBuzz()
    {

    }

    [Scenario(ConvergenceThreshold = 3000)]
    public void WriteValueScenario()
    {

    }
  }

  [ScenarioSet(ConvergenceThreshold = 500)]
  [TrainingDependency(ScenarioSet = typeof(SimpleScenarios), RequiredConfidenceThreshold = .98)]
  public class CompleteScenario(FizzBuzzMind Mind)
  {
    [Scenario(ConvergenceThreshold = 100)]
    public void From1To100()
    {

    }
  }

  [CognitiveActions]
  public partial interface FizzBuzzTerminal
  {
    void WriteNumber([Categorical] byte ToWrite);
    void Fizz();
    void Buzz();
  }

  [CognitiveData]
  public partial class FizzBuzzInput
  {
    public byte Value { get; set; }

    public static CognitiveDataCodec<byte> ValueCodec { get; } =
      new CompositeCodec<byte>(
        new BitwiseOneHotNumberCodec<byte>(),
        new NormalizeNumberCodec<byte, float>(
          byte.MinValue, byte.MaxValue, new RoundingCodec<float>(new CopyFloatCodec())));
  }


  [Mind]
  public partial class FizzBuzzMind
  {
    [Use]
    public partial Thought<bool, UseFeedback<FizzBuzzTerminal>> WriteForNumber(FizzBuzzTerminal Surface, FizzBuzzInput InputData);
  }
}