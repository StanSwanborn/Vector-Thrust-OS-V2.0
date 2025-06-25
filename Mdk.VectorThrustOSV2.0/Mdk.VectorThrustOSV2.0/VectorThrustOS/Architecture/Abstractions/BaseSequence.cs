using IngameScript.VectorThrustOS.SequenceRuntime;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Runtime;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Architecture.Abstractions
{
    internal abstract class BaseSequence<T> where T : BaseSequence<T>
    {
        protected MyGridProgram         BaseProgram { get; private set; }
        protected ThrustOSConfig        ThrustOSConfig { get; private set; }
        protected RuntimeTracker        RuntimeTracker { get; private set; }
        protected RuntimeProperties     RuntimeProperties { get; private set; }
        protected RuntimeCollections    RuntimeCollections { get; private set; }
        protected SequenceAssigner<T>   ParentSequenceAssigner { get; private set; }

        protected BaseSequence(
            MyGridProgram baseProgram,
            ThrustOSConfig thrustOSConfig,
            RuntimeTracker runtimeTracker,
            RuntimeProperties runtimeProperties,
            RuntimeCollections runtimeCollections,
            SequenceAssigner<T> parentSequenceAssigner
        )
        {
            BaseProgram             = baseProgram;
            ThrustOSConfig          = thrustOSConfig;
            RuntimeTracker          = runtimeTracker;
            RuntimeProperties       = runtimeProperties;
            RuntimeCollections      = runtimeCollections;
            ParentSequenceAssigner  = parentSequenceAssigner;
        }
        protected void AddBlock<BlockType>(IMyTerminalBlock block, List<BlockType> list)
        {
            if (!RuntimeCollections.AllBlocks.Contains(block)) 
                RuntimeCollections.AllBlocks.Add(block);

            if (!list.Contains((BlockType)block)) 
                list.Add((BlockType)block);
            
            RuntimeProperties.BlockCount++;
        }

        public abstract IEnumerable<int> Process();
    }
}