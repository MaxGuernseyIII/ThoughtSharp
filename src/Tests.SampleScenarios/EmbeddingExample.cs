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

using Google.Protobuf.WellKnownTypes;
using ThoughtSharp.Adapters.TorchSharp;
using ThoughtSharp.Runtime;
using ThoughtSharp.Runtime.Codecs;
using ThoughtSharp.Scenarios;

namespace Tests.SampleScenarios;

public partial class EmbeddingExample
{
  [MindPlace]
  public class EmbeddingMindPlace : MindPlace<TokenMind, TorchBrain>
  {
    public override TorchBrain MakeNewBrain()
    {
      return TorchBrainBuilder.For<TokenMind>().UsingSequence(S => S.AddEmbedding(1)).Build();
    }

    public override void LoadSavedBrain(TorchBrain ToLoad)
    {
    }

    public override void SaveBrain(TorchBrain ToSave)
    {
    }
  }

  [CognitiveData]
  public partial class TokenContainer
  {
    public int Token;

    public static readonly CognitiveDataCodec<int> TokenCodec = new TokenCodec<int>(10);
  }

  [Capability]
  public class TrainEmbedding(TokenMind Mind)
  {
    [Behavior]
    public Transcript TrainIdentity()
    {

      var Result = Mind.IdentityBatch(
        new(new() {Token = 0}),
        new(new() {Token = 1}),
        new(new() {Token = 2}),
        new(new() {Token = 3}),
        new(new() {Token = 4}),
        new(new() {Token = 5}),
        new(new() {Token = 6}),
        new(new() {Token = 7}),
        new(new() {Token = 8}),
        new(new() {Token = 9}));

      return Assert.That(Result).ConvergesOn().Target(
        [
          new() { Value = 0f },
          new() { Value = 1f }, 
          new() { Value = 2f }, 
          new() { Value = 3f }, 
          new() { Value = 4f }, 
          new() { Value = 5f }, 
          new() { Value = 6f }, 
          new() { Value = 7f }, 
          new() { Value = 8f }, 
          new() { Value = 9f }, 
        ], 
        C => C.Expect(
          F => F.Value, 
          (Expected, Actual) => Actual.ShouldConvergeOn().Approximately(Expected, .01f, .20f)));
    }
  }

  [CognitiveData]
  public partial class Result
  {
    public float Value;
  }

  [Mind]
  public partial class TokenMind
  {
    [Make]
    public partial CognitiveResult<Result, Result> Identity(TokenContainer C);
  }

  [MaximumAttempts(500000)]
  [Curriculum]
  public class EmbeddingCurriculum
  {
    [Phase(0)]
    [Include(typeof(TrainEmbedding))]
    public class OnlyPhase;
  }
}