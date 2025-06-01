# ThoughtSharp

ThoughtSharp is a library to support the development of hybrid-reasoning algorithms. It currently natively supports
a TorchSharp backend.

ThoughtSharp provides an abstraction layer over neural network reasoning. It supports the following:
 * The `use` operation - let AI decide how to use an object in a particular situation
 * The `choose` operation - let AI choose one out of an arbitrarily large number of options for you
 * The `make` operation - let AI generate a data structure for you
 * Marshalling and unmarshalling of `cognitive data` to and from tensors

Below is an example of an interface that a FizzBuzz solution could use in ThoughtSharp.

```CSharp
[Mind]
partial class FizzBuzzMind
{
  [Use]
  public partial Thought<bool, UseFeedback<FizzBuzzTerminal>> WriteForNumber(
    FizzBuzzTerminal Surface, byte Number);
}

[CognitiveActions]
public partial interface FizzBuzzTerminal
{
  void WriteNumber([Categorical]byte ToWrite);
  void Fizz();
  void Buzz();
}
```

Then, to use that, you can create a hybrid reasoning class:

```CSharp
class FizzBuzzHybridReasoning(FizzBuzzMind Mind)
{
  public Thought<string, NullFeedback> DoFizzBuzz(
    byte Start,
    byte End)
  {
    return Thought.WithoutFeedback.Think(R =>
    {
      var Terminal = new StringBuilderFizzBuzzTerminal();

      foreach (var I in Enumerable.Range(Start, End - Start + 1))
        R.Incorporate(WriteForOneNumber(Terminal, (byte) I));

      return Terminal.Content.ToString();
    });
  }

  public Thought<List<UseFeedback<FizzBuzzTerminal>>> WriteForOneNumber(FizzBuzzTerminal Terminal, byte Input)
  {
    return Thought.Do(R =>
    {
      var Feedback = new List<UseFeedback<FizzBuzzTerminal>>();
      using var Chaining = Mind.EnterChainedReasoning();

      foreach (var _ in Enumerable.Range(0, 5))
      {
        var Thought = Mind.WriteForNumber(Terminal, new() { Value = Input});
        Feedback.Add(Thought.Feedback);
        
        if (!R.Consume(Thought))
          break;
      }

      return Feedback;
    });
  }
}
```

If you decide that you want more control, you can do things like change the codec used to pass in the parameter.
For instance, the following code makes sure the each bit of the byte is encoded into a one-hot array **and** the
scalar value is normalized to [0..1].

```CSharp
[CognitiveData]
partial class FizzBuzzInput
{
  public byte Value { get; set; }

  public static CognitiveDataCodec<byte> ValueCodec { get; } = 
    new CompositeCodec<byte>(
      new BitwiseOneHotNumberCodec<byte>(),
      new NormalizeNumberCodec<byte,float>(
        byte.MinValue, byte.MaxValue, new RoundingCodec<float>(new CopyFloatCodec())));
}

[Mind]
partial class FizzBuzzMind
{
  [Use]
  public partial Thought<bool, UseFeedback<FizzBuzzTerminal>> WriteForNumber(
    FizzBuzzTerminal Surface, FizzBuzzInput InputData);
}
```

Building the network is also something that has been abstracted to the point of being a little easier:

```CSharp
  var BrainBuilder = TorchBrainBuilder.ForTraining<FizzBuzzMind>()
    .UsingParallel(P => P
      .AddLogicPath(16, 4, 8)
      .AddPath(S => S));

  var Mind = new FizzBuzzMind(BrainBuilder.Build());
```

Once you have an instance of the appropriate `Mind`, and you've wrapped it in whatever hybrid logic class(es)
you need, you can invoke it to get both the product (which I suppose ML experts call the "Prediction") and an
object that can be used to train the network.

```CSharp
    var T = HybridReasoning.WriteForOneNumber(Terminal, (byte) Input);
```

In the above example, `T` is an instance of type `Thought<List<UseFeedback<FizzBuzzTerminal>>>`. That is "a
thought with a feedback object that is a list of feedback objects for individual `use`s of the `FizzBuzzTerminal`
action surface".

Each of those feedback objects for the `use` operation represents one decision the AI made about
how to use the `FizzBuzzTerminal` it was given. Those feedback objects can be used to train the underlying neural
network. For instance, for the number `15`, you would tell the network this:

```
T.Feedback.First().ExpectationsWere((Mock, More) =>
{
  Mock.Fizz();
  More.Value = true;
});
T.Feedback.ElementAtOrDefault(1)?.ExpectationsWere((Mock, More) =>
{
  Mock.Buzz();
  More.Value = false;
});
```

In other words, the first call to the `use` operation should have called `Fizz()` and requested another call. The
second call to the `use` operation should have called `Buzz()` and indicated that no more calls wee required.

There are similar semantically-structured feedback options for `choose` operations and `make` operations.

```CSharp
// feedback on the selection from a set of options
T.Feedback.SelectionShouldHaveBeen(Expected);

// feedback on the make operation
T.Feedback.ResultShouldHaveBeen(new() {Area = Expected});
```

# Current State

This framework is in development. I haven't even published it on NuGet, yet. The interface might change radically
between now and when I do. I am currently mostly working on building the hybrid testing-training infrastructure.

# Where it Came From

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