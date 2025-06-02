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

using System.Text;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Example.FizzBuzz;

class CognitiveResultFizzBuzzTerminal : FizzBuzzTerminal
{
  static IEnumerable<Action<FizzBuzzTerminal>> TransformStringToFeedback(string Arg)
  {
    var I = int.Parse(Arg);
    var Fizz = I % 3 == 0;
    var Buzz = I % 5 == 0;

    if (Fizz)
      yield return T => T.Fizz();

    if (Buzz)
      yield return T => T.Buzz();

    if (!Fizz && !Buzz)
      yield return T => T.WriteNumber((byte)I);
  }

  public readonly List<byte> WrittenBytes = [];

  public IncrementalCognitiveResult<string, string, string, IEnumerable<Action<FizzBuzzTerminal>>> Result = new(
    Parts => string.Join(" ", Parts),
    Whole => Whole.Split().Select(TransformStringToFeedback));

  StringBuilder CurrentContent = new();

  public void WriteNumber(byte ToWrite)
  {
    WrittenBytes.Add(ToWrite);
    CurrentContent.Append(ToWrite);
  }

  public void Fizz()
  {
    CurrentContent.Append("fizz");
  }

  public void Buzz()
  {
    CurrentContent.Append("buzz");
  }

  public void Flush(FeedbackSink<IEnumerable<Action<FizzBuzzTerminal>>> FeedbackSink)
  {
    Result = Result.Add(CognitiveResult.From(CurrentContent.ToString(), FeedbackSink));
    CurrentContent = new();
  }
}