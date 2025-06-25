using IngameScript.VectorThrustOS.Architecture.Abstractions;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Extensions;
using IngameScript.VectorThrustOS.Runtime;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.SequenceRuntime.Sequences
{
    internal class GetBatteryStatsSequence : BaseSequence<GetBatteryStatsSequence>
    {
        public GetBatteryStatsSequence(
            MyGridProgram baseProgram,
            ThrustOSConfig thrustOSConfig,
            RuntimeTracker runtimeTracker,
            RuntimeProperties runtimeProperties,
            RuntimeCollections runtimeCollections,
            SequenceAssigner<GetBatteryStatsSequence> parentSequenceAssigner
        ) : base(baseProgram, thrustOSConfig, runtimeTracker, runtimeProperties, runtimeCollections, parentSequenceAssigner)
        { }

        public override IEnumerable<int> Process()
        {
            while (true)
            {
                RuntimeCollections.OutputBatteriesSequence.Clear();
                if (RuntimeCollections.BatteriesSequence.Count > 0)
                {
                    double inputs = 0;
                    double outputs = 0;
                    double percents = 0;
                    foreach (IMyPowerProducer b in RuntimeCollections.BatteriesSequence)
                    {
                        outputs += b.CurrentOutput;
                        if (b is IMyBatteryBlock)
                        {
                            inputs += (b as IMyBatteryBlock).CurrentInput;
                            percents += (b as IMyBatteryBlock).CurrentStoredPower / (b as IMyBatteryBlock).MaxStoredPower;
                        }

                        outputs -= b.MaxOutput;
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                    inputs /= inputs != 0 ? RuntimeCollections.BatteriesSequence.Count : 1;
                    outputs /= outputs != 0 ? RuntimeCollections.BatteriesSequence.Count : 1;
                    percents *= percents != 0 ? (100 / RuntimeCollections.BatteriesSequence.Count) : 1;

                    RuntimeCollections.OutputBatteriesSequence = new List<double> { inputs, outputs, percents.Round(0) };
                    yield return ThrustOSConfig.FramesBetweenActions;
                }

                ParentSequenceAssigner.Doneloop = true;
                yield return ThrustOSConfig.FramesBetweenActions;
            }
        }
    }
}
