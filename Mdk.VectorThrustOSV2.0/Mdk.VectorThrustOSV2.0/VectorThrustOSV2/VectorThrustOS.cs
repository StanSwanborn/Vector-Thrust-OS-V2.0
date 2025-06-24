using IngameScript.VectorThrustOSV2.Configuration;
using IngameScript.VectorThrustOSV2.Runtime;
using IngameScript.VectorThrustOSV2.Enums;
using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript.VectorThrustOSV2
{
    internal class VectorThrustOS
    {
        readonly ThrustOSConfig     _thrustOSConfig;
        readonly RuntimeProperties  _runtimeProperties;
        readonly RuntimeTracker     _runtimeTracker;

        public VectorThrustOS(MyGridProgram parent)
        {
            _thrustOSConfig     = new ThrustOSConfig();
            _runtimeProperties  = new RuntimeProperties();
            _runtimeTracker     = new RuntimeTracker(parent, _thrustOSConfig, 100, 500);

            parent.Runtime.UpdateFrequency = UpdateFrequency.Update1;
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

        // Populate the runtime properties from the saved string (if available).
        public void Load(string storage)
        {
            var saved = storage.Split(';');

            if (saved.Length > 0)
            {
                var tagParts = saved[0].Split(':');
                if (tagParts.Length == 2)
                {
                    _runtimeProperties.Tag      = tagParts[0];
                    _runtimeProperties.Greedy   = bool.Parse(tagParts[1]);
                }
            }

            if (saved.Length > 1)
                _thrustOSConfig.AllowPark = bool.Parse(saved[1]);

            if (saved.Length > 2 && int.TryParse(saved[2], out var outGear))
                _thrustOSConfig.Gear = Math.Min(outGear, _thrustOSConfig.Accelerations.Count - 1);

            if (saved.Length > 3)
                _runtimeProperties.Cruise = bool.Parse(saved[3]);
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