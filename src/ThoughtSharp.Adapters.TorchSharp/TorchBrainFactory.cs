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

//static class TorchBrainFactory
//{
//  static TorchBrainForProductionMode MakeProductionTorchBrain(
//    torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Module) => new TorchBrainForProductionMode(Module,
//    delegate { return new TorchInferenceStateNode(null); });
//}

//class TorchBrainFactory<TTorchBrain> : BrainFactory<TTorchBrain, torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, torch.Device>
//  where TTorchBrain : TorchBrain
//{
//  readonly Func<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, TTorchBrain> MakeBrain;

//  public TorchBrainFactory(Func<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>, TTorchBrain> MakeBrain)
//  {
//    this.MakeBrain = MakeBrain;
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateLinear(int InputFeatures, int OutputFeatures)
//  {
//    var Unwrapped = torch.nn.Linear(InputFeatures, OutputFeatures);
//    torch.nn.init.kaiming_uniform_(Unwrapped.weight);
//    torch.nn.init.zeros_(Unwrapped.bias);

//    return new StatePassThroughModule(Unwrapped);
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateTanh()
//  {
//    return new StatePassThroughModule(torch.nn.Tanh());
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateSequence(params IEnumerable<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Children)
//  {
//    return Children.Skip(1).Aggregate(Children.First(), (Current1, Module) => new CompositeModule(Current1, Module));
//  }

//  public TTorchBrain CreateBrain(torch.nn.Module<TorchInferenceParts, TorchInferenceParts> Model)
//  {
//    return MakeBrain(Model);
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateParallel(params IEnumerable<torch.nn.Module<TorchInferenceParts, TorchInferenceParts>> Children)
//  {
//    return Children.Skip(1).Aggregate(Children.First(), (Previous, Current) => new ParallelModule(Previous, Current));
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateGRU(int InputFeatures, int OutputFeatures)
//  {
//    return new AdditionalDimensionForSubModule(new DoubleTensorToTorchInferencePartsAdapter(torch.nn.GRU(InputFeatures, OutputFeatures)));
//  }

//  public torch.nn.Module<TorchInferenceParts, TorchInferenceParts> CreateReLU()
//  {
//    return new StatePassThroughModule(torch.nn.ReLU());
//  }
//}