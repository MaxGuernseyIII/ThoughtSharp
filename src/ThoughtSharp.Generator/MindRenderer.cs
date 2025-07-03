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

using System.CodeDom.Compiler;

namespace ThoughtSharp.Generator;

static class MindRenderer
{
  public static string Render(MindModel M)
  {
    return GeneratedTypeFormatter.GenerateType(new(M.TypeName)
    {
      WriteHeader = W =>
      {
        W.WriteLine("using ThoughtSharp.Runtime;");
        W.WriteLine("using System.Collections.Immutable;");
        W.WriteLine();
      },
      WriteAfterTypeName = W => { W.Write($" : Mind<{M.TypeName.FullName}>"); },
      WriteBody = W =>
      {
        ushort OperationCode = 1;

        WriteConstructionMembers(W, M);
        WriteInterfaceMembers(W, M);

        RenderModelMembers(W, M, OperationCode);
      }
    });
  }

  static void WriteConstructionMembers(IndentedTextWriter W, MindModel M)
  {
    W.WriteLine("Brain Brain;");
    W.WriteLine("CognitionMode CognitionMode;");
    W.WriteLine();
    W.WriteLine($"public static {M.TypeName.FullName} Create(Brain Brain) => new(Brain);");
    W.WriteLine();
    W.WriteLine(
      $"public {M.TypeName.TypeName.Name}(Brain Brain) : this(Brain, new IsolatedCognitionMode(Brain)) {{ }}");
    W.WriteLine();
    using (W.DeclareWithBlock($"{M.TypeName.TypeName.Name}(Brain Brain, CognitionMode CognitionMode)"))
    {
      W.WriteLine("this.Brain = Brain;");
      W.WriteLine("this.CognitionMode = CognitionMode;");
    }

    W.WriteLine();
  }

  static void WriteInterfaceMembers(IndentedTextWriter W, MindModel M)
  {
    W.WriteLine(
      $"public {M.TypeName.FullName} WithChainedReasoning() => new {M.TypeName.FullName}(Brain, CognitionMode.EnterChainedLineOfReasoning());");

    W.WriteLine($"public static ImmutableArray<long> EncodedTokenClassCounts => Input.EncodedTokenClassCounts;");
    W.WriteLine($"public static int InputLength {{ get; }} = {M.TypeName.FullName}.Input.FloatLength;");
    W.WriteLine($"public static int OutputLength {{ get; }} = {M.TypeName.FullName}.Output.FloatLength;");
    W.WriteLine();

    using (W.DeclareWithBlock("public static void WriteIsolationBoundaries(IsolationBoundariesWriter W)"))
    {
      W.WriteLine("Output.WriteIsolationBoundaries(W);");
    }
  }

  static void RenderModelMembers(IndentedTextWriter W, MindModel M, ushort OperationCode)
  {
    foreach (var MakeOperation in M.MakeOperations) 
      RenderForSpecifiedMakeMethod(W, M, MakeOperation, OperationCode++);

    foreach (var UseOperation in M.UseOperations) 
      RenderForSpecifiedUseMethod(W, M, UseOperation, OperationCode++);

    foreach (var ChooseOperation in M.ChooseOperations)
      RenderChooseMethod(W, M, ChooseOperation, OperationCode++);

    foreach (var TellOperation in M.TellOperations)
      RenderTellMethod(W, M, TellOperation, OperationCode++);
  }

  static void RenderForSpecifiedUseMethod(IndentedTextWriter W, MindModel M, MindUseOperationModel UseOperation,
    ushort OperationCode)
  {
    RenderUseMethod(W, M, UseOperation, OperationCode);
    RenderBatchUseMethod(W, M, UseOperation, OperationCode);
  }

  static void RenderForSpecifiedMakeMethod(IndentedTextWriter W, MindModel M,
    MindMakeOperationModel MakeOperation, ushort OperationCode)
  {
    RenderSingularMakeMethod(W, MakeOperation);
    RenderBatchMakeMethod(W, MakeOperation, OperationCode);
  }

  static void RenderTellMethod(IndentedTextWriter W, MindModel M, MindTellOperationModel TellOperation, ushort OpCode)
  {
    using (W.DeclareWithBlock(
             $"public partial void {TellOperation.Name}({TellOperation.ParameterTypeName} {TellOperation.ParameterName})"))
    {
      W.WriteLine("var InputBuffers = new System.Collections.Generic.List<float[]>();");
      using (W.DeclareWithBlock($"foreach (var Item in {TellOperation.ParameterName})"))
      {
        W.WriteLine("var InputObject = new Input();");
        W.WriteLine($"InputObject.OperationCode = {OpCode.ToLiteralExpression()};");

        W.WriteLine($"InputObject.Parameters.{TellOperation.Name}.{TellOperation.ParameterName} = Item;");

        W.WriteLine("var InputBuffer = new float[Input.FloatLength];");
        W.WriteLine("InputObject.MarshalTo(InputBuffer, []);");
        W.WriteLine("InputBuffers.Add(InputBuffer);");
      }

      W.WriteLine();
      W.WriteLine("var Inference = CognitionMode.CurrentInferenceSource.MakeInference(Batch.OfTensorData.Builder.AddSequence(S => InputBuffers.Aggregate(S, (Previous, Step) => Previous.AddStep(new() { Features = Step, Tokens = [] }))).Build());");
      W.WriteLine("var OutputObject = Output.UnmarshalFrom(Inference.Result[0], []);");
      W.WriteLine("CognitionMode = CognitionMode.RegisterNewInference(Inference);");
    }
  }

  static void RenderSingularMakeMethod(IndentedTextWriter W, MindMakeOperationModel MakeOperation)
  {
    using (W.DeclareWithBlock(
      $"public partial CognitiveResult<{MakeOperation.ReturnType}, {MakeOperation.ReturnType}> {MakeOperation.Name}({string.Join(", ", MakeOperation.Parameters.Select(P => $"{P.Type} {P.Name}"))})"))
    {
      W.WriteLine($"var Result = {MakeOperation.Name}Batch(");
      W.Indent++;
      W.WriteLine("[");
      W.Indent++;
      W.WriteLine("new(");
      W.Indent++;

      W.WriteLine(string.Join(", ", MakeOperation.Parameters.Select(P => P.Name)));

      W.Indent--;
      W.WriteLine(")");
      W.Indent--;
      W.WriteLine("]");
      W.Indent--;
      W.WriteLine(");");

      W.WriteLine($"return CognitiveResult.From(Result.Payload[0], new SingleMakeFeedbackSink<{MakeOperation.ReturnType}>(Result.FeedbackSink));");
    }
  }

  static void RenderBatchMakeMethod(IndentedTextWriter W, MindMakeOperationModel MakeOperation,
    ushort OperationCode)
  {
    AddArgsFor(W, MakeOperation.Name, MakeOperation.Parameters);
    W.WriteLine(
      $"public CognitiveResult<IReadOnlyList<{MakeOperation.ReturnType}>, IReadOnlyList<{MakeOperation.ReturnType}>> {MakeOperation.Name}Batch(params IReadOnlyList<{MakeOperation.Name}Args> TimeSequences)");
    W.WriteLine("{");
    W.Indent++;

    var TimeStepParameterName = MakeOperation.TimeSteps?.ParameterName;

    W.WriteLine("var InputBatches = new List<float[][]>();");

    using (W.DeclareWithBlock("foreach (var __TIME_SEQUENCE__ in TimeSequences)"))
    {
      W.WriteLine("var InputBuffers = new List<float[]>();");

      W.WriteLine();

      using (TimeStepParameterName is not null
               ? W.DeclareWithBlock($@"foreach (var __TIME_STEP__ in __TIME_SEQUENCE__.{TimeStepParameterName})")
               : null)
      {
        RenderInputObjectForOpCode(W, OperationCode);

        foreach (var Parameter in MakeOperation.Parameters)
        {
          var AssignmentSource = Parameter.Name == TimeStepParameterName ? @"__TIME_STEP__" : $"__TIME_SEQUENCE__.{Parameter.Name}";
          W.WriteLine($"InputObject.Parameters.{MakeOperation.Name}.{Parameter.Name} = {AssignmentSource};");
        }

        W.WriteLine("var InputBuffer = new float[Input.FloatLength];");
        W.WriteLine("InputObject.MarshalTo(InputBuffer, []);");
        W.WriteLine("InputBuffers.Add(InputBuffer);");
      }

      W.WriteLine("InputBatches.Add([..InputBuffers]);");
    }

    WriteBatchConstructionLogic(W, "InputBatches");
    W.WriteLine();
    W.WriteLine("var Inference = CognitionMode.CurrentInferenceSource.MakeInference(__BATCH__);");
    W.WriteLine($"var ReturnObjects = new List<{MakeOperation.ReturnType}>();");
    using (W.DeclareWithBlock("foreach (var Result in Inference.Result)"))
    {
      W.WriteLine("var OutputObject = Output.UnmarshalFrom(Result, []);");
      W.WriteLine($"ReturnObjects.Add(OutputObject.Parameters.{MakeOperation.Name}.Value);");
    }
    W.WriteLine("CognitionMode = CognitionMode.RegisterNewInference(Inference);");
    W.WriteLine();

    W.WriteLine($"var OutputStart = Output.ParametersIndex + Output.OutputParameters.{MakeOperation.Name}Index;");
    W.WriteLine($"var OutputEnd = OutputStart + Output.OutputParameters.{MakeOperation.Name}Parameters.FloatLength;");

    W.WriteLine("return CognitiveResult.From(");
    W.Indent++;
    W.WriteLine("ReturnObjects,");
    W.WriteLine($"new BatchMakeFeedbackSink<{MakeOperation.ReturnType}>(");
    W.Indent++;
    W.WriteLine("new(");
    W.Indent++;
    W.WriteLine("Inference");
    W.Indent--;
    W.WriteLine("),");
    W.WriteLine("(V, Writer) => ");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("var O = new Output");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"Parameters = {{ {MakeOperation.Name} = {{ Value = V }} }}");
    W.Indent--;
    W.WriteLine("};");
    W.WriteLine();
    W.WriteLine("O.WriteAsLossRules(Writer);");
    W.Indent--;
    W.WriteLine("}");
    W.Indent--;
    W.WriteLine(")");
    W.Indent--;
    W.WriteLine(");");
    W.Indent--;
    W.WriteLine("}");
  }

  static void WriteBatchConstructionLogic(IndentedTextWriter W, string BatchListName)
  {
    W.WriteLine("var __BATCH__ = " + BatchListName + ".Aggregate(Batch.OfTensorData.Builder,");
    W.Indent++;
    W.WriteLine("(B, Sequence) => B.AddSequence(SB =>");
    W.Indent++;
    W.WriteLine("Sequence.Aggregate(SB,");
    W.Indent++;
    W.WriteLine("(Previous, Step) => Previous.AddStep(new() { Features = Step, Tokens = []})");
    W.WriteLine(")");
    W.Indent--;
    W.WriteLine(")");
    W.Indent--;
    W.WriteLine(").Build();");
    W.Indent--;
  }

  static void AddArgsFor(IndentedTextWriter W, string OperationName, IReadOnlyList<(string Name, string Type)> Parameters)
  {
    W.WriteLine();
    W.WriteLine(
      $"public partial record {OperationName}Args({string.Join(", ", Parameters.Select(P => $"{P.Type} {P.Name}"))});");
    W.WriteLine();
  }

  static void RenderUseMethod(
    IndentedTextWriter W,
    MindModel Model,
    MindUseOperationModel UseOperation,
    ushort OperationCode)
  {
    var InputParameters = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is null);
    var ActionSurfaces = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is not null);
    var ActionSurface = ActionSurfaces.Single();

    var MethodIsAsync = ActionSurface.AssociatedInterpreter!.RequiresAwait;
    var FeedbackType = $"UseFeedbackMethod<{ActionSurface.TypeName}>";
    var FeedbackSinkType = $"UseFeedbackSink<{ActionSurface.TypeName}>";
    if (MethodIsAsync)
    {
      FeedbackType = "Async" + FeedbackType;
      FeedbackSinkType = "Async" + FeedbackSinkType;
    }

    var ReturnType = $"CognitiveResult<bool, {FeedbackType}>";
    if (MethodIsAsync)
      ReturnType = $"Task<{ReturnType}>";

    var (Async, Unwrap) = MethodIsAsync
      ? ("async ", "await ")
      : ("", "");

    W.WriteLine(
      $"public partial {Async}{ReturnType} {UseOperation.Name}({string.Join(", ", UseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"var __ARGS__ = new {UseOperation.Name}Args({string.Join(", ", UseOperation.Parameters.Select(P => $"{P.Name}"))});");
    W.WriteLine($"var __RESULT__ = {Unwrap}{UseOperation.Name}Batch(__ARGS__);");
    W.WriteLine($"var __REQUIRES_MORE__ = __RESULT__.Payload[0];");

    W.WriteLine();
    W.WriteLine($"return CognitiveResult.From(__REQUIRES_MORE__, new {FeedbackSinkType}(__RESULT__.FeedbackSink));");
    W.Indent--;
    W.WriteLine("}");
  }
  static void RenderBatchUseMethod(
    IndentedTextWriter W,
    MindModel Model,
    MindUseOperationModel UseOperation,
    ushort OperationCode)
  {
    AddArgsFor(W, UseOperation.Name, [..UseOperation.Parameters.Select(P => (P.Name, P.TypeName))]);

    var InputParameters = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is null);
    var ActionSurfaces = UseOperation.Parameters.Where(P => P.AssociatedInterpreter is not null);
    var ActionSurface = ActionSurfaces.Single();

    var MethodIsAsync = ActionSurface.AssociatedInterpreter!.RequiresAwait;
    var FeedbackType = $"UseFeedbackMethod<{ActionSurface.TypeName}>";
    var FeedbackSinkType = $"BatchUseFeedbackSink<{ActionSurface.TypeName}>";
    if (MethodIsAsync)
    {
      FeedbackType = "Async" + FeedbackType;
      FeedbackSinkType = "Async" + FeedbackSinkType;
    }

    var ReturnType = $"CognitiveResult<IReadOnlyList<bool>, IReadOnlyList<{FeedbackType}>>";
    if (MethodIsAsync)
      ReturnType = $"Task<{ReturnType}>";

    W.WriteLine();
    W.WriteLine($"class {UseOperation.Name}FeedbackMock(IList<Output> Outputs, int TimeSequenceNumber) : {ActionSurface.TypeName}");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine("bool Conditioned;");
    W.WriteLine("Output Expected = new();");
    W.WriteLine();

    ushort ActionCode = 1;
    foreach (var Path in ActionSurface.AssociatedInterpreter!.Paths)
    {
      W.WriteLine(
        $"public {Path.ReturnType} {Path.MethodName}({string.Join(", ", Path.ParametersClass.Parameters.Select(P => $"{P.FullType} {P.Name}"))})");
      W.WriteLine("{");
      W.Indent++;
      W.WriteLine("if (Conditioned) throw new InvalidOperationException(\"Cannot condition a training mock twice.\");");
      W.WriteLine("Conditioned = true;");
      W.WriteLine(
        $"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.ActionCode = {ActionCode.ToLiteralExpression()};");

      foreach (var Parameter in Path.ParametersClass.Parameters)
        W.WriteLine(
          $"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.Parameters.{Path.MethodName}.{Parameter.Name} = {Parameter.Name};");

      var ReturnValue = Path.RequiresAwait ? " Task.CompletedTask" : "";
      W.WriteLine($"return{ReturnValue};");

      W.Indent--;
      W.WriteLine("}");
      W.WriteLine();
      ActionCode++;
    }
    W.WriteLine("public void Commit(bool ExpectedMore)");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine($"Expected.Parameters.{UseOperation.Name}.{ActionSurface.Name}.MoreActions = ExpectedMore;");
    W.WriteLine("Outputs[TimeSequenceNumber] = Expected;");
    W.Indent--;
    W.WriteLine("}");
    W.Indent--;
    W.WriteLine("}");
    W.WriteLine();
    var (Async, Unwrap) = MethodIsAsync
      ? ("async ", "await ")
      : ("", "");

    W.WriteLine(
      $"public {Async}{ReturnType} {UseOperation.Name}Batch(params IReadOnlyList<{UseOperation.Name}Args> __TIME_SEQUENCES__)");
    W.WriteLine("{");
    W.Indent++;

    W.WriteLine("var InputBuffers = new List<float[][]>();");

    using (W.DeclareWithBlock("foreach (var __TIME_SEQUENCE__ in __TIME_SEQUENCES__)"))
    {
      RenderInputObjectForOpCode(W, OperationCode);
      W.WriteLine();

      foreach (var Parameter in InputParameters)
        W.WriteLine($"InputObject.Parameters.{UseOperation.Name}.{Parameter.Name} = __TIME_SEQUENCE__.{Parameter.Name};");

      W.WriteLine("var InputBuffer = new float[Input.FloatLength];");
      W.WriteLine("InputObject.MarshalTo(InputBuffer, []);");
      W.WriteLine("InputBuffers.Add([InputBuffer]);");

      W.WriteLine();
    }

    WriteBatchConstructionLogic(W, "InputBuffers");

    W.WriteLine();
    W.WriteLine("var Inference = CognitionMode.CurrentInferenceSource.MakeInference(__BATCH__);");
    W.WriteLine("var OutputObjects = Inference.Result.Select(R => Output.UnmarshalFrom(R, [])).ToList();");
    W.WriteLine("CognitionMode = CognitionMode.RegisterNewInference(Inference);");

    W.WriteLine($"var FeedbackOutputs = new List<Output>(OutputObjects);");
    W.WriteLine($"var FeedbackMocks = new List<({UseOperation.Name}FeedbackMock Mock, Action<bool> CommitOne)>();");
    W.WriteLine();
    W.WriteLine("var ReturnValues = new List<bool>();");
    using (W.DeclareWithBlock("foreach (var (OutputObject, TimeSequenceIndex) in OutputObjects.Select((O, I) => (O, I)))"))
    {
      W.WriteLine(
        $"var MoreActions = ({Unwrap}OutputObject.Parameters.{UseOperation.Name}.{ActionSurface.Name}");
      W.Indent++;
      W.WriteLine(
        $".InterpretFor(__TIME_SEQUENCES__[TimeSequenceIndex].{ActionSurface.Name})).Payload;");
      W.Indent--;
      W.WriteLine("ReturnValues.Add(MoreActions);");
      W.WriteLine();
      W.WriteLine($"var FeedbackMock = new {UseOperation.Name}FeedbackMock(FeedbackOutputs, TimeSequenceIndex);");
      W.WriteLine("FeedbackMocks.Add((FeedbackMock, FeedbackMock.Commit));");
    }
    W.WriteLine($"var FeedbackSink = new {FeedbackSinkType}([..FeedbackMocks], delegate");
    using (W.EnterBlock())
    {
      W.WriteLine("var Writer = new LossRuleWriter();");
      using (W.DeclareWithBlock("foreach (var (Expected, Index) in FeedbackOutputs.Select((E, I) => (E, I)))"))
      {
        W.WriteLine("Writer = Writer.AtBeginningOfTimeSequence(Index);");
        W.WriteLine("Expected.WriteAsLossRules(Writer);");
      }
      W.WriteLine("Inference.Train(Writer.Stream.PositionRulePairs);");
    }
    W.WriteLine($");");
    W.WriteLine();
    W.WriteLine("return CognitiveResult.From(ReturnValues, FeedbackSink);");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderChooseMethod(
    IndentedTextWriter W,
    MindModel MindModel,
    MindChooseOperationModel ChooseOperation,
    ushort OpCode)
  {
    W.WriteLine(
      $"public partial {ChooseOperation.ReturnType} {ChooseOperation.Name}({string.Join(", ", ChooseOperation.Parameters.Select(P => $"{P.TypeName} {P.Name}"))})");
    W.WriteLine("{");
    W.Indent++;
    W.WriteLine(
      $"var Source = ChooseFeedback<{ChooseOperation.SelectableTypeName}>.GetSource<{ChooseOperation.Parameters.Single(P => P.Name == ChooseOperation.CategoryParameter).TypeName}.Output>();");
    W.WriteLine($"var Champion = {ChooseOperation.CategoryParameter}.AllOptions.First();");
    W.WriteLine();
    W.WriteLine($"foreach (var Contender in {ChooseOperation.CategoryParameter}.AllOptions.Skip(1))");
    using (W.EnterBlock())
    {
      RenderInputObjectForOpCode(W, OpCode);
      foreach (var Parameter in ChooseOperation.Parameters)
        if (Parameter.Name != ChooseOperation.CategoryParameter)
          W.WriteLine($"InputObject.Parameters.{ChooseOperation.Name}.{Parameter.Name} = {Parameter.Name};");

      W.WriteLine(
        $"InputObject.Parameters.{ChooseOperation.Name}.{ChooseOperation.CategoryParameter} = {ChooseOperation.CategoryParameter}.GetContestBetween(Champion, Contender);");

      W.WriteLine();
      RenderMakeInference(W, MindModel);

      W.WriteLine(
        $"var Offset = Output.ParametersIndex + Output.OutputParameters.{ChooseOperation.Name}Index + Output.OutputParameters.{ChooseOperation.Name}Parameters.{ChooseOperation.CategoryParameter}Index;");
      W.WriteLine(
        $"var T = {ChooseOperation.CategoryParameter}.Interpret(Champion, Contender, OutputObject.Parameters.{ChooseOperation.Name}.{ChooseOperation.CategoryParameter}, Inference, Offset);");
      W.WriteLine("Source.Configurator.AddSingleChoice(T.FeedbackSink);");
      W.WriteLine("Champion = T.Payload;");
    }

    W.WriteLine(
      "return CognitiveResult.From(Champion.Payload, Source.CreateFeedback());");
    W.Indent--;
    W.WriteLine("}");
  }

  static void RenderInputObjectForOpCode(IndentedTextWriter W, ushort OperationCode)
  {
    W.WriteLine("var InputObject = new Input();");
    W.WriteLine($"InputObject.OperationCode = {OperationCode.ToLiteralExpression()};");
  }

  static void RenderMakeInference(IndentedTextWriter W, MindModel Model)
  {
    W.WriteLine("var InputBuffer = new float[Input.FloatLength];");
    W.WriteLine("InputObject.MarshalTo(InputBuffer, []);");
    W.WriteLine();
    W.WriteLine("var Inference = CognitionMode.CurrentInferenceSource.MakeInference(Batch.OfTensorData.Builder.AddSequence(S => S.AddStep(new() { Features = InputBuffer, Tokens = [] })).Build());");
    W.WriteLine("var OutputObject = Output.UnmarshalFrom(Inference.Result[0], []);");
    W.WriteLine("CognitionMode = CognitionMode.RegisterNewInference(Inference);");
  }
}