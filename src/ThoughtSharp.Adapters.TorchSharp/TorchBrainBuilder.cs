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

using ThoughtSharp.Runtime;
using TorchSharp;

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchBrainBuilder(int InputLength, int OutputLength)
{
  public static TorchBrainBuilder For<TInput, TOutput>()
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    return new(TInput.Length, TOutput.Length);
  }

  public enum ActivationType
  {
    LeakyReLU,
    ReLU,
    Sigmoid,
    Tanh,
    Softmax,
    None
  }

  public enum ExecutionDevice
  {
    CUDA,
    CPU
  }

  public List<Layer> Layers =
  [
    new()
    {
      Features = InputLength * 20,
    },
    new()
    {
      Features = (InputLength * 20 + OutputLength) / 2,
    }
  ];

  public record Layer
  {
    public required int Features { get; set; }
    public ActivationType ActivationType { get; set; } = ActivationType.Tanh;
  }

  public int StateSize { get; set; } = 0;
  public bool AllowTraining { get; set; } = true;

  public ExecutionDevice Device { get; set; } = torch.cuda.is_available() ? ExecutionDevice.CUDA : ExecutionDevice.CPU;
  public ActivationType FinalActivationType = ActivationType.None;

  public TorchBrainBuilder SetDefaultClassificationConfiguration()
  {
    FinalActivationType = OutputLength == 1 ? ActivationType.Sigmoid : ActivationType.Softmax;

    return this;
  }

  public Brain Build()
  {
    var TorchLayers = new List<torch.nn.Module<torch.Tensor, torch.Tensor>>();
    var InFeatures = Layers.Aggregate(InputLength, (Current, Layer) => AddModulesForLayer(Layer, TorchLayers, Current));

    AddModulesForLayer(new()
    {
      Features = OutputLength,
      ActivationType = FinalActivationType
    }, TorchLayers, InFeatures);

    var DeviceType = Device switch
    {
      ExecutionDevice.CUDA => global::TorchSharp.DeviceType.CUDA,
      _ => global::TorchSharp.DeviceType.CPU
    };
    var NeuralNet = torch.nn.Sequential(TorchLayers.ToArray()).to(DeviceType);

    return AllowTraining ? 
      new TorchBrainForTrainingMode(NeuralNet, new(DeviceType), StateSize) : 
      new TorchBrainForProductionMode(NeuralNet, new(DeviceType), StateSize);
  }

  protected static int AddModulesForLayer(Layer Layer, List<torch.nn.Module<torch.Tensor, torch.Tensor>> TorchLayers, int InFeatures)
  {
    var OutFeatures = Layer.Features;
    var Linear = torch.nn.Linear(InFeatures, OutFeatures);
    torch.nn.init.kaiming_uniform_(Linear.weight);
    torch.nn.init.zeros_(Linear.bias);
    TorchLayers.Add(Linear);
    InFeatures = OutFeatures;
    var ActivationLayer = Layer.ActivationType switch
    {
      ActivationType.LeakyReLU => torch.nn.LeakyReLU(),
      ActivationType.ReLU => torch.nn.ReLU(),
      ActivationType.Sigmoid => torch.nn.Sigmoid(),
      ActivationType.Tanh => torch.nn.Tanh(),
      ActivationType.Softmax => torch.nn.Softmax(1),
      _ => (torch.nn.Module<torch.Tensor, torch.Tensor>?)null
    };
    if (ActivationLayer is not null)
      TorchLayers.Add(ActivationLayer);
    return InFeatures;
  }
}