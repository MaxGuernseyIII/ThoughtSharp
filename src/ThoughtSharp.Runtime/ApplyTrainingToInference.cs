using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThoughtSharp.Runtime
{
  class ApplyTrainingToInference(Mind Mind, Inference Target, IReadOnlyList<Range> OutputRanges, IReadOnlyList<Range> StateRanges) : TrainingPolicy
  {
    readonly IReadOnlyList<Range> OutputRanges = [..OutputRanges];
    readonly IReadOnlyList<Range> OutputAndStateRanges = [..StateRanges,..OutputRanges];

    public void IncentivizeOutput(float Reward)
    {
      Target.Incentivize(Reward, OutputRanges);
    }

    public void IncentivizeOutputAndState(float Reward)
    {
      Target.Incentivize(Reward, OutputAndStateRanges);
    }

    public Mind? Mind { get; } = Mind;
  }
}
