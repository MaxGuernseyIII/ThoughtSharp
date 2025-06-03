# ThoughtSharp.Adapters.TorchSharp

The package that adapts [TorchSharp](https://github.com/dotnet/TorchSharp) for use with a ThoughtSharp project.

This package implements the abstractions necessary to create an instance of `Bran` using TorchSharp as a backing technology.

Key implementations:
  
  - `TorchBrain`
  - `TorchBrainFactory`
  - `TorchBrainBuilder` (which just glues together `BrainBuilder<TBrain, TModel, TDevice>` with the torch adapters)

The simplest way to integrate it into your system is with a line like this:

```CSharp
var Brain = TorchBrainBuilder.For<MindType>().UsingStandard().Build();
```

## Installation

```bash
dotnet add package ThoughtSharp.Adapters.TorchSharp
```