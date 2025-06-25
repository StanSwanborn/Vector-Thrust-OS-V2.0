using IngameScript.VectorThrustOS.SequenceRuntime;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Factories;
using IngameScript.VectorThrustOS.Runtime;
using IngameScript.VectorThrustOS.Enums;
using Sandbox.ModAPI.Ingame;
using IngameScript.VectorThrustOS.SequenceRuntime.Sequences;

namespace IngameScript.VectorThrustOS
{
    internal class VectorThrustOSProgram
    {
        /** SequenceAssigners **/
        readonly SequenceAssigner<BlockManagerSequence>     _blockManager;
        readonly SequenceAssigner<GetBatteryStatsSequence>  _batteryStats;

        readonly SequenceAssigner   _mainChecker;
        readonly SequenceAssigner   _getScreen;
        readonly SequenceAssigner   _getControllers;
        readonly SequenceAssigner   _getVectorThrusters;
        readonly SequenceAssigner   _checkParkBlocks;
        readonly SequenceAssigner   _assign;

        /** SequenceFactories **/
        readonly SequenceFactories _sequenceFactories;

        /** VectorThrustOS Runtime and Config properties **/
        readonly ThrustOSConfig     _thrustOSConfig;
        readonly RuntimeProperties  _runtimeProperties;
        readonly RuntimeCollections _runtimeCollections;
        readonly RuntimeTracker     _runtimeTracker;

        public VectorThrustOSProgram(MyGridProgram parent)
        {
            _thrustOSConfig     = new ThrustOSConfig();
            _runtimeProperties  = new RuntimeProperties();
            _runtimeCollections = new RuntimeCollections();
            _runtimeTracker     = new RuntimeTracker(parent, _thrustOSConfig, 100, 500);
            
            _sequenceFactories = new SequenceFactories(
                baseProgram:        parent,
                thrustOSConfig:     _thrustOSConfig,
                runtimeTracker:     _runtimeTracker,
                runtimeProperties:  _runtimeProperties,
                runtimeCollections: _runtimeCollections
            );

            _blockManager = new SequenceAssigner(_sequenceFactories.BlockManagerSequenceFactory(), true);

            parent.Echo("--VTOS V2.0 Started--");


        }

        public string Save() => 
            string.Join(
                separator: ";", 
                string.Join(":", _runtimeProperties.Tag, _runtimeProperties.Greedy), 
                _thrustOSConfig.AllowPark, 
                _thrustOSConfig.Gear, 
                _runtimeProperties.Cruise
            );

        public void Load(string storage)
        {
            var saved = storage.Split(';');

            _runtimeProperties.LoadFromStorage(saved);
            _thrustOSConfig.LoadFromStorage(saved);
        }

        public void Run(RunArgumentEnum runArgument)
        {
            _runtimeTracker.Process();

            if(!_runtimeProperties.ParkedCompletely || runArgument != RunArgumentEnum.NONE || _runtimeProperties.TrulyParked)
            {
                MyShipVelocities velocities = 
            }
        }
    }
}