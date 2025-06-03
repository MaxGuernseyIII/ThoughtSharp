// See https://aka.ms/new-console-template for more information

using ThoughtSharp.Runtime;

Console.WriteLine("Hello, World!");

[Mind]
public partial class TheMind
{
  [Make]
  public partial CognitiveResult<D, D> MakeIt();
}

[CognitiveData]
public partial class D
{
  
}