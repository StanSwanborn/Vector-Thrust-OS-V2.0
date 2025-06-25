using IngameScript.VectorThrustOS.Architecture.Abstractions;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Controllers
{
    internal class ShipController : BlockWrapper<IMyShipController>
    {
        public bool Dampener;
        public List<IMyThrust> nThrusters = new();
        public List<IMyThrust> cruiseThrusters = new();

        public ShipController(IMyShipController theBlock, Program program) : base(theBlock, program) =>
            Dampener = theBlock.DampenersOverride;

        public void SetDampener(bool val)
        {
            Dampener = val;
            TheBlock.DampenersOverride = val;
        }
    }
}