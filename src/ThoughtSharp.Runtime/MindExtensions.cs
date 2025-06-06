﻿// MIT License
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

namespace ThoughtSharp.Runtime;

public static class MindExtensions
{
  public static AccumulatedUseFeedback<TSurface> Use<TMind, TSurface>(
    this TMind Mind,
    Func<TMind, CognitiveResult<bool, UseFeedbackMethod<TSurface>>> Action,
    MindExtensions.UseConfiguration? Configuration = null)
    where TMind : Mind<TMind>
  {
    var ChainedMind = Mind.WithChainedReasoning();
    var EffectiveConfiguration = Configuration ?? new();
    var Source = AccumulatedUseFeedback.GetSource<TSurface>();
    foreach (var _ in Enumerable.Range(0, EffectiveConfiguration.MaxTrials))
    {
      var T = Action(ChainedMind);
      Source.Configurator.AddStep(T.FeedbackSink);
      var MoreRequested = T.Payload;
      if (!MoreRequested)
        break;
    }

    return Source.CreateFeedback();
  }

  public class UseConfiguration
  {
    public int MaxTrials { get; init; } = 5;
  }
}