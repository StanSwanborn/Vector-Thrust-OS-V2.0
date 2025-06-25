using IngameScript.VectorThrustOS.SequenceRuntime.Sequences;
using IngameScript.VectorThrustOS.SequenceRuntime;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Runtime;
using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript.VectorThrustOS.Factories
{
    internal class SequenceFactories
    {
        readonly MyGridProgram      _baseProgram;        
        readonly ThrustOSConfig     _thrustOSConfig;
        readonly RuntimeProperties  _runtimeProperties;
        readonly RuntimeCollections _runtimeCollections;
        readonly RuntimeTracker     _runtimeTracker;

        public SequenceFactories(
            MyGridProgram baseProgram,
            ThrustOSConfig thrustOSConfig,
            RuntimeTracker runtimeTracker,
            RuntimeProperties runtimeProperties,
            RuntimeCollections runtimeCollections
            )
        {
            _baseProgram        = baseProgram;
            _thrustOSConfig     = thrustOSConfig;
            _runtimeTracker     = runtimeTracker;
            _runtimeProperties  = runtimeProperties;
            _runtimeCollections = runtimeCollections;
        }

        public Func<SequenceAssigner, BlockManagerSequence> BlockManagerSequenceFactory(SequenceAssigner getBatteryStatsSequenceAssigner) =>
            sequenceParent => new BlockManagerSequence(
                _baseProgram,
                _thrustOSConfig,
                _runtimeTracker,
                _runtimeProperties,
                _runtimeCollections,
                getBatteryStatsSequenceAssigner,
                sequenceParent
            );
    }
}