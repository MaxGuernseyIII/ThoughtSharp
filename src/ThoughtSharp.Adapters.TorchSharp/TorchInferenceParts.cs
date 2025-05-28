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

public record TorchInferenceParts
{
  public required TorchInferenceStateNode State { get; set; }
  public required torch.Tensor Payload { get; set; }

  public TorchInferenceParts UnSqueeze()
  {
    return new()
    {
      Payload = Payload.unsqueeze(0),
      State = State.UnSqueeze()
    };
  }

  public TorchInferenceParts Squeeze()
  {
    return new()
    {
      Payload = Payload.squeeze(0),
      State = State.Squeeze()
    };
  }
}

public record TorchInferenceStateNode(torch.Tensor? State) : IDisposable
{
  public TorchInferenceStateNode? Left { get; init; }
  public TorchInferenceStateNode? Right { get; init; }

  public TorchInferenceStateNode UnSqueeze() => new(State?.unsqueeze(0))
  {
    Left = Left?.UnSqueeze(),
    Right = Right?.UnSqueeze()
  };

  public TorchInferenceStateNode Squeeze() => new(State?.squeeze(0))
  {
    Left = Left?.Squeeze(),
    Right = Right?.Squeeze()
  };

  public void Dispose()
  {
    Left?.Dispose();
    Right?.Dispose();
    State?.Dispose();
  }
}