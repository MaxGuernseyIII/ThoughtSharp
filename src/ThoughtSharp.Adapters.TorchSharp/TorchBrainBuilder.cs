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


  public bool AllowTraining { get; set; } = true;

  public ExecutionDevice Device { get; set; } = cuda.is_available() ? ExecutionDevice.CUDA : ExecutionDevice.CPU;

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
    var Path = new Path
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
        }
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

  public TorchBrainBuilder AddLogicPath(ushort Layer1ScaleBy = 16, ushort Layer2ScaleBy = 4,
    ushort? Layer3ScaleBy = null)
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

    if (Layer3ScaleBy is { } Scale)
      Path.Layers.Add(new()
      {
        Features = InputLength * Scale,
        ActivationType = ActivationType.ReLU
      });

    Paths.Add(Path);

    return this;
  }

  public TorchBrainBuilder Blank()
  {
    Paths.Clear();
    return this;
  }

  public TorchBrain Build()
  {
    var TotalInputFeatures = InputLength;

    var DeviceType = Device switch
    {
      ExecutionDevice.CUDA => global::TorchSharp.DeviceType.CUDA,
      _ => global::TorchSharp.DeviceType.CPU
    };
    var FirstPath = Paths.First();
    var DeviceInstance = new Device(DeviceType);
    var (Transformer, CombinedWidth, MakeState) = BuildTorchModules(FirstPath, TotalInputFeatures, DeviceInstance);
    (Transformer, CombinedWidth, MakeState) = Paths.Skip(1).Aggregate((Transformer, Width: CombinedWidth, MakeState),
      (Current, NextPath) =>
      {
        var (NewPipeline, AdditionalWidth, NewMakeState) =
          BuildTorchModules(NextPath, TotalInputFeatures, DeviceInstance);
        return (new ParallelModule(Current.Transformer, NewPipeline),
          Current.Width + AdditionalWidth, () => new(null)
          {
            Left = Current.MakeState(),
            Right = NewMakeState()
          });
      });

    var FinalTransform = MakeLinearTransform(CombinedWidth, OutputLength);
    var FullPipeline = new CompositeModule(Transformer, FinalTransform);

    return AllowTraining
      ? new TorchBrainForTrainingMode(FullPipeline, DeviceInstance, MakeState)
      : new TorchBrainForProductionMode(FullPipeline, DeviceInstance, MakeState);
  }

  (nn.Module<TorchInferenceParts, TorchInferenceParts>, int FinalWidth, Func<TorchInferenceStateNode>)
    BuildTorchModules(Path Path, int TotalInputFeatures, Device Device)
  {
    var PathStateSize = Path.StateCoefficient * TotalInputFeatures;
    var TorchLayers = new List<nn.Module<Tensor, Tensor>>();
    var FinalWidth = TotalInputFeatures;
    if (PathStateSize != 0)
      FinalWidth = PathStateSize;
    foreach (var Layer in Path.Layers)
      FinalWidth = AddModulesForLayer(Layer, TorchLayers, FinalWidth);


    nn.Module<TorchInferenceParts, TorchInferenceParts> Transformer =
      new StatePassThroughModule(nn.Sequential(TorchLayers.ToArray()).to(Device.type));
    Transformer = Path.StateCoefficient > 0
      ?
        //new CompositeModule(
        //(MakeLinearTransform(TotalInputFeatures, PathStateSize)),
        new CompositeModule(
          new AdditionalDimensionForSubModule(new DoubleTensorToTorchInferencePartsAdapter(nn.GRU(
            InputLength,
            PathStateSize),
            0, null!
          )), Transformer)
        //)
      : Transformer;
    return (Transformer, FinalWidth, () =>
    {
      var Tensor = zeros(new long[] {1, PathStateSize}, ScalarType.Float32, Device);
      return new(Tensor);
    });
  }

  protected static int AddModulesForLayer(Layer Layer, List<nn.Module<Tensor, Tensor>> TorchLayers,
    int InFeatures)
  {
    var OutFeatures = Layer.Features;
    var Linear = nn.Linear(InFeatures, OutFeatures);
    nn.init.kaiming_uniform_(Linear.weight);
    nn.init.zeros_(Linear.bias);
    TorchLayers.Add(Linear);
    var ActivationLayer = Layer.ActivationType switch
    {
      ActivationType.LeakyReLU => nn.LeakyReLU(),
      ActivationType.ReLU => nn.ReLU(),
      ActivationType.Sigmoid => nn.Sigmoid(),
      ActivationType.Tanh => nn.Tanh(),
      ActivationType.Softmax => nn.Softmax(1),
      _ => (nn.Module<Tensor, Tensor>?) null
    };
    if (ActivationLayer is not null)
      TorchLayers.Add(ActivationLayer);
    return OutFeatures;
  }

  static nn.Module<TorchInferenceParts, TorchInferenceParts> MakeLinearTransform(int InFeatures, int OutFeatures)
  {
    var Linear = nn.Linear(InFeatures, OutFeatures);
    nn.init.kaiming_uniform_(Linear.weight);
    nn.init.zeros_(Linear.bias);
    return new StatePassThroughModule(Linear);
  }

  public TorchBrainBuilder WithPath(List<Layer> Layers)
  {
    Paths.Add(new()
    {
      Layers = Layers
    });

    return this;
  }

  public sealed record Path
  {
    public int StateCoefficient { get; set; } = 0;

    public List<Layer> Layers { get; set; } = [];
  }

  public sealed record Layer
  {
    public required int Features { get; set; }
    public ActivationType ActivationType { get; set; } = ActivationType.Tanh;
  }
}