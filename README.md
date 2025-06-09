# ThoughtSharp
![ThoughtSharp badge](https://img.shields.io/nuget/dt/ThoughtSharp.svg) 

ThoughtSharp is a library to support the development of hybrid-reasoning algorithms and object-oriented systems.
It currently natively supports a TorchSharp backend.

ThoughtSharp provides an abstraction layer over neural network reasoning. It supports the following:
 * The `use` operation - let AI decide how to use an object in a particular situation
 * The `choose` operation - let AI choose one out of an arbitrarily large number of options for you
 * The `make` operation - let AI generate a data structure for you
 * Marshalling and unmarshalling of `cognitive data` to and from tensors

## Why ThoughtSharp?

ThoughtSharp allows you to integrate neural networks into ordinary coding relatively seamlessly.

As an example, let's say you want to integrate a neural network into a solution for the classic
[FizzBuzz](https://en.wikipedia.org/wiki/Fizz_buzz) problem.

The reason you would want to use ThoughtSharp in such a case is that it allows you to make the neural network
look like "just another object", up to and including letting the neural originate calls to other objects.

## Overview of Use

An implementation of ThoughtSharp has four basic kinds of code
1. Minds - the interface between a neural network and OO logic
1. Data types - the structured data that is consumed and produced by a mind
1. Training scenarios - TDD-inspired declarations about how minds should function
1. Hybrid reasoning - Your code that uses or is used by a mind

Currently, there are code generators and/or interpreters that support a declarative approach to the first three.

## Examples

There are multiple detailed examples in the `/examples` subdirectory of this repo:

* [/examples/SimpleDemo](https://github.com/MaxGuernseyIII/ThoughtSharp/tree/master/examples/SimpleDemo)

  Use the `make` verb to calculate `y=mx+B`.
* [/examples/FizzBuzz](https://github.com/MaxGuernseyIII/ThoughtSharp/tree/master/examples/FizzBuzz)

  A hybrid-reasoning solution to the FizzBuzz problem solved with the `use` verb.
* [/examples/ShapeSelector](https://github.com/MaxGuernseyIII/ThoughtSharp/tree/master/examples/ShapeSelector)

  Pick one out of an arbitrarily-large selection of objects with the `choose` verb.

## Getting Started

Before you start working with ThoughtSharp, you'll need the `dotnet-train` package. This, obviously, only
needs to be done one time per machine whenever you want to install or upgrade.
```ps1
dotnet tool install -g dotnet-train
```

Once you've done that, you will need to set up two projects:
1. The hybrd-reasoning project
   - Must have a package reference to `ThoughtSharp`
1. The training project
   - Must have a package reference to `ThoughtSharp.Shaping`
   - Must have an adapter for a neural network (currently `ThoughtSharp.Adapters.TorchSharp`)
   - Must have a reference to the runtimes for TorchSharp (*e.g.*, `TorchSharp-cpu`)

It's always easier to say it in code...

```ps1
# create a new sample solution
dotnet new sln

# make the hybrid-reasoning project
mkdir MyCoolHybridReasoning
pushd MyCoolHybridReasoning
dotnet new classlib
dotnet add package ThoughtSharp
popd

# make the training project
mkdir MyCoolHybridReasoning.Scenarios
pushd MyCoolHybridReasoning.Scenarios
dotnet new classlib
dotnet add reference ../MyCoolHybridReasoning
dotnet add package ThoughtSharp.Shaping
dotnet add package ThoughtSharp.Adapters.TorchSharp
dotnet add package TorchSharp-cpu
popd

# add the projects to the solution
dotnet sln add MyCoolHybridReasoning
dotnet sln add MyCoolHybridReasoning.Scenarios
```

You're now ready to start defining behaviors and implementing your hybrid-reasoning code.

When you're ready to train a model, you can accomplish this with the `dotnet train` command.

```ps1
dotnet train MyCoolHybridReasoning.Scenarios
```

...or...

```ps1
pushd MyCoolHybridReasoning.Scenarios
dotnet train
popd
```

## Current State

This framework is in development. The main parts of its interface are pretty stable,
but there are pieces of functionality I haven't added, yet.

## NuGet Packages

There are several NuGet packages. This is a summary of the ones most people use:

| Package | Purpose |
|:--|:--|
| `ThoughtSharp` | A "rollup" dependency for hybrid-reasoning code. |
| `ThoughtSharp.Shaping` | A "rollup" dependency for test/training code. |
| `ThoughtSharp.Adapters.TorchSharp` | Adapts [TorchSharp](https://github.com/dotnet/TorchSharp) for use as the backing neural network technology |
| `dotnet-train` | Supplies the `dotnet train` command used to train neural networks |

## Licensing

ThoughtSharp is offered under the [MIT License](LICENSE.txt).

## Where it Came From

ThoughtSharp was born from, literally, a dream. As I was drifting off to sleep, one night,
I thought "Wouldn't it be great if there was an AI-first programming language".

I imagined all sorts of cool syntax constructs. Wouldn't it be great if there was seamless
integration between traditional code and neural networks? Something like...

```CSharp
public thoughtful bool FilterMessage(MessageValidationMind M, string Message)
{
  var F = think make MessageFeatures with M and Message;

  if (F.Violent)
  {
    Console.WriteLine("This message has been blocked because is promotes or threatens violence.");
    return false;
  }

  if (F.ContainsPersonallyIdentifyingMessages)
  {
    Console.WriteLine("This message has been blocked because it shares personally-identifying messages.")
    return false;
  }

  return true;
}
```

...or...

```CSharp
public interface LoanActions
{
  void Approve();
  void Reject();
  void RequestSupportingDocuments(DocumentationCategory Requested);
}

public class LoanOfficer(LoanOfficerMind Mind)
{
  public thoughtful void OnApplicationReady(LoaApplication Application, LoanActions Actions)
  {
    use Actions with Mind and Application;
  }
}
```

...maybe even...

```CSharp
public thoughtful Image Crop(ImageProcessingMind Mind, Image Image, params IEnumerable<CroppingStrategy> Strategies)
{
  var Strategy = think choose from Strategies with Mind and Image;

  return Strategy.Crop(Image);
}
```

A way to train these algorithms would be required as well. On top of all of that, there would need to be the usual
mechanisms that modern software developers want: a way to develop your networks from a test-driven perspective first
and foremost.

# Where It's At, Now

So I got started working on a runtime for it and, it turns out, you can solve all these problems without the
syntactic sugar above.

Right now, this project is focusd on building a meaningful runtime. When I have time to get into building a parser
for a fully-functional language, I'll get on that, too.