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

class TorchBrainFactory : BrainFactory<TorchBrain, torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device>
{
  public static TorchBrainFactory Instance => new((M, D) => new(M, D));

  readonly Func<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device, TorchBrain> MakeBrain;

  internal TorchBrainFactory(Func<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device, TorchBrain> MakeBrain)
  {
    this.MakeBrain = MakeBrain;
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateTimeAware(IEnumerable<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Children, torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Pooling)
  {
    return new TimeAwareModule([..Children], Pooling);
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateLinear(int InputFeatures, int OutputFeatures, bool WithBias)
  {
    var Unwrapped = torch.nn.Linear(InputFeatures, OutputFeatures, hasBias:WithBias);

    return new StatePassThroughModule(Unwrapped);
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateAttentionPooling(int InputFeatures)
  {
    return new AttentionPooling(InputFeatures);
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateTanh()
  {
    return new StatePassThroughModule(torch.nn.Tanh());
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateSequence(params IEnumerable<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Children)
  {
    torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Initial = new FullPassThroughModule();
    return Children.Aggregate(Initial, (Current1, Module) => new CompositeModule(Current1, Module));
  }

  public TorchBrain CreateBrain(torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Model, torch.Device Device)
  {
    return MakeBrain(Model.to(Device), Device);
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateParallel(params IEnumerable<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Children)
  {
    return Children.Skip(1).Aggregate(Children.First(), (Previous, Current) => new ParallelModule(Previous, Current));
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateGRU(
    int InputFeatures, 
    int OutputFeatures,
    int GRULayers, 
    torch.Device Device)
  {
    var Underlying = torch.nn.GRU(InputFeatures, OutputFeatures, GRULayers);
    var Adapter = new GRUAdapter(Underlying, OutputFeatures, GRULayers, Device);  
    return Adapter;
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateLatestTimeStepInStatePooling()
  {
    return new LatestTimeStepInStatePooling();
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateMeanOverTimeStepsPooling()
  {
    return new MeanOverTimeStepsPooling();
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateReLU()
  {
    return new StatePassThroughModule(torch.nn.ReLU());
  }

  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateSiLU()
  {
    return new StatePassThroughModule(torch.nn.SiLU());
  }

  public torch.Device GetDefaultOptimumDevice()
  {
    return torch.cuda.is_available() ? GetCUDADevice() : GetCPUDevice();
  }

  public torch.Device GetCPUDevice()
  {
    return new(DeviceType.CPU);
  }

  public torch.Device GetCUDADevice()
  {
    return new(DeviceType.CUDA);
  }
}