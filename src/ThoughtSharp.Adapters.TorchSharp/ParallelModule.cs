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

using TorchSharp;

namespace ThoughtSharp.Adapters.TorchSharp;

public sealed class ParallelModule : torch.nn.Module<TorchInferenceParts, TorchInferenceParts>
{
  readonly torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Left;
  readonly torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Right;

  public ParallelModule(torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Left, torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Right,
    string Name = "_unnamed") : base(Name)
  {
    this.Left = Left;
    this.Right = Right;

    RegisterComponents();
  }

  public override TorchInferenceParts forward(TorchInferenceParts Input)
  {
    var LeftOutput = Left.forward(Input with { State = Input.State.Left! });
    var RightOutput = Right.forward(Input with{ State = Input.State.Right! });

    //Console.WriteLine($"Left path output: [{string.Join(", ", LeftOutput.Payload.shape)}]");
    //Console.WriteLine($"Right path output: [{string.Join(", ", RightOutput.Payload.shape)}]");

    var Result = new TorchInferenceParts()
    {
      Payload = torch.cat([LeftOutput.Payload, RightOutput.Payload], 1),
      State = Input.State with
      {
        Left = LeftOutput.State,
        Right = RightOutput.State
      }
    };

    return Result;
  }
}