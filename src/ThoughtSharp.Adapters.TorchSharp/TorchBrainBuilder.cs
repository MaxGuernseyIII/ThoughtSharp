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
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace ThoughtSharp.Adapters.TorchSharp;

// ReSharper disable once UnusedMember.Global
public class TorchBrainBuilder(int InputLength, int OutputLength)
{
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

  public sealed record Path
  {
    public List<Layer> Layers { get; set; } = [];
  }

  public List<Path> Paths { get; set; } =
  [
    new()
    {
      Layers =
      [
        new()
        {
          Features = InputLength * 20
        },
        new()
        {
          Features = (InputLength * 20 + OutputLength) / 2
        }
      ]
    }
  ];


  public int StateCoefficient { get; set; } = 0;
  public bool AllowTraining { get; set; } = true;

  public ExecutionDevice Device { get; set; } = torch.cuda.is_available() ? ExecutionDevice.CUDA : ExecutionDevice.CPU;

  public static TorchBrainBuilder For<TInput, TOutput>()
    where TInput : CognitiveData<TInput>
    where TOutput : CognitiveData<TOutput>
  {
    return new(TInput.Length, TOutput.Length);
  }

  public static TorchBrainBuilder For<TMind>()
    where TMind : Mind
  {
    return new(TMind.InputLength, TMind.OutputLength);
  }

  public TorchBrainBuilder ForClassification()
  {
    return this;
  }

  public TorchBrainBuilder ForLogic(ushort Layer1ScaleBy = 16, ushort Layer2ScaleBy = 4, ushort? Layer3ScaleBy = null)
  {
    return Blank().AddLogicPath(Layer1ScaleBy, Layer2ScaleBy, Layer3ScaleBy);
  }

  public TorchBrainBuilder ForMath(ushort Layer1ScaleBy = 16, ushort Layer2ScaleBy = 4, ushort? Layer3ScaleBy = null)
  {
    return Blank().AddMathPath();
  }

  public TorchBrainBuilder AddMathPath(ushort Layer1ScaleBy = 4, ushort Layer2ScaleBy = 4, ushort? Layer3ScaleBy = null)
  {
    var Path = new Path()
    {
      Layers =
      [
        new()
        {
          Features = InputLength * Layer1ScaleBy,
          ActivationType = ActivationType.Tanh
        },
        new()
        {
          Features = InputLength * Layer2ScaleBy,
          ActivationType = ActivationType.Tanh
        },
      ]
    };

    if (Layer3ScaleBy is { } Scale)
      Path.Layers.Add(new()
      {
        Features = InputLength * Scale,
        ActivationType = ActivationType.Tanh
      });

    Paths.Add(Path);

    return this;
  }

  public TorchBrainBuilder AddLogicPath(ushort Layer1ScaleBy = 16, ushort Layer2ScaleBy = 4, ushort? Layer3ScaleBy = null)
  {
    var Path = new Path();

    Path.Layers.AddRange([
      new()
      {
        Features = InputLength * Layer1ScaleBy,
        ActivationType = ActivationType.ReLU
      },
      new()
      {
        Features = InputLength * Layer2ScaleBy,
        ActivationType = ActivationType.ReLU
      }
    ]);

    if (Layer3ScaleBy is {} Scale)
    {
      Path.Layers.Add(new()
      {
        Features = InputLength * Scale,
        ActivationType = ActivationType.ReLU
      });
    }
    
    Paths.Add(Path);

    return this;
  }

  TorchBrainBuilder Blank()
  {
    Paths.Clear();
    return this;
  }

  public Brain Build()
  {
    var TotalStateSize = StateCoefficient * InputLength;
    var TotalInputFeatures = TotalStateSize > 0 ? TotalStateSize : InputLength;
    var TotalOutputFeatures = OutputLength;

    var DeviceType = Device switch
    {
      ExecutionDevice.CUDA => global::TorchSharp.DeviceType.CUDA,
      _ => global::TorchSharp.DeviceType.CPU
    };
    var FirstPath = Paths.First();
    var (Transformer, CombinedWidth) = BuildTorchModules(FirstPath, TotalInputFeatures, TotalOutputFeatures, DeviceType);
    (Transformer, CombinedWidth) = Paths.Skip(1).Aggregate((Transformer, Width: CombinedWidth),
      (Current, NextPath) =>
      {
        var (NewPipeline, AdditionalWidth) = BuildTorchModules(NextPath, TotalInputFeatures, TotalOutputFeatures, DeviceType);
        return (new ParallelModule(Current.Transformer, NewPipeline),
          Current.Width + AdditionalWidth);
      });

    var FinalTransform = MakeLinearTransform(CombinedWidth, OutputLength);
    var FullPipeline = nn.Sequential(Transformer, FinalTransform);

    nn.Module<TorchInferenceParts, TorchInferenceParts> PassThrough = new StatePassThroughModule(FullPipeline);
    var Module = PassThrough;
    if (TotalStateSize > 0)
      Module = new CompositeModule(new AdditionalDimensionForSubModule(new DoubleTensorToTorchInferencePartsAdapter(nn.GRU(
        inputSize: InputLength,
        hiddenSize: TotalStateSize)
      )), Module);

    return AllowTraining
      ? new TorchBrainForTrainingMode(Module, new(DeviceType), TotalStateSize)
      : new TorchBrainForProductionMode(Module, new(DeviceType), TotalStateSize);
  }

  (nn.Module<Tensor, Tensor>, int FinalWidth) BuildTorchModules(Path Path, int TotalInputFeatures, int TotalOutputFeatures, DeviceType DeviceType)
  {
    var TorchLayers = new List<torch.nn.Module<torch.Tensor, torch.Tensor>>();
    var FinalWidth = TotalInputFeatures;
    foreach (var Layer in Path.Layers) 
      FinalWidth = AddModulesForLayer(Layer, TorchLayers, FinalWidth);

    

    var Transformer = nn.Sequential(TorchLayers.ToArray()).to(DeviceType);
    return (Transformer, FinalWidth);
  }

  protected static int AddModulesForLayer(Layer Layer, List<torch.nn.Module<torch.Tensor, torch.Tensor>> TorchLayers,
    int InFeatures)
  {
    var OutFeatures = Layer.Features;
    var Linear = MakeLinearTransform(InFeatures, OutFeatures);
    TorchLayers.Add(Linear);
    var ActivationLayer = Layer.ActivationType switch
    {
      ActivationType.LeakyReLU => torch.nn.LeakyReLU(),
      ActivationType.ReLU => torch.nn.ReLU(),
      ActivationType.Sigmoid => torch.nn.Sigmoid(),
      ActivationType.Tanh => torch.nn.Tanh(),
      ActivationType.Softmax => torch.nn.Softmax(1),
      _ => (torch.nn.Module<torch.Tensor, torch.Tensor>?) null
    };
    if (ActivationLayer is not null)
      TorchLayers.Add(ActivationLayer);
    return OutFeatures;
  }

  static Linear MakeLinearTransform(int InFeatures, int OutFeatures)
  {
    var Linear = torch.nn.Linear(InFeatures, OutFeatures);
    torch.nn.init.kaiming_uniform_(Linear.weight);
    torch.nn.init.zeros_(Linear.bias);
    return Linear;
  }

  public sealed record Layer
  {
    public required int Features { get; set; }
    public ActivationType ActivationType { get; set; } = ActivationType.Tanh;
  }

  public TorchBrainBuilder WithStateCoefficient(int Coefficient)
  {
    StateCoefficient = Coefficient;

    return this;
  }

  public TorchBrainBuilder WithPath(List<Layer> Layers)
  {
    Paths.Add(new()
    {
      Layers = Layers
    });

    return this;
  }
}