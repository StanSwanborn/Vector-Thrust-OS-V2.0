using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Runtime;
using IngameScript.VectorThrustOS.Enums;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS
{
    internal class VectorThrustOSProgram
    {
        readonly ThrustOSConfig     _thrustOSConfig;
        readonly RuntimeProperties  _runtimeProperties;
        readonly RuntimeTracker     _runtimeTracker;

        public VectorThrustOSProgram(MyGridProgram parent)
        {
            _thrustOSConfig     = new ThrustOSConfig();
            _runtimeProperties  = new RuntimeProperties();
            _runtimeTracker     = new RuntimeTracker(parent, _thrustOSConfig, 100, 500);

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