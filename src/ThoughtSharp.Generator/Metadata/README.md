# ThoughtSharp.Generator

This package generates all the higher-level abstractions for your ThoughtSharp project.

This includes types of `Mind`, which are the high-level wrappers around neural networks that allow you to make semantic calls rather than
fiddle with floating point arrays. Each `Mind` can have any number of the following three kinds of operations:

  - `Make` - create some data based on inputs
  - `Choose` - select from an arbitrarily-large set of options based on inputs
  - `Use` - make calls to an interface based on inputs

It also includes generators for several supporting kinds of class:

  - `CognitiveData` - objects that marshal and unmarshal to and from tensors, used throughout the system
  - `CognitiveCategory` - arbitrarily large groups of options selected using the `Choose` operation
  - `CognitiveActions` - an action surface to manipulate with the `Use` operation

## Installation

```bash
dotnet add package ThoughtSharp.Generator
```