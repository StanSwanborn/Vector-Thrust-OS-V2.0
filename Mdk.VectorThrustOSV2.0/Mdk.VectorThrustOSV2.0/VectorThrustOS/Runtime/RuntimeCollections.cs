using IngameScript.VectorThrustOS.Controllers;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Runtime
{
    internal class RuntimeCollections
    {
        public List<ShipController> Controllers { get; set; } = new();
        public List<IMyShipController> ControllerBlocks { get; set; } = new();
        public List<IMyShipController> CControllerBlocks { get; set; } = new();
        public List<ShipController> ControlledControllers { get; set; } = new();
        public List<VectorThrust> VectorThrusters { get; set; } = new();
        public List<IMyThrust> NormalThrusters { get; set; } = new();
        public List<IMyTextPanel> Screens { get; set; } = new();
        public List<IMyShipConnector> Connectors { get; } = new();
        public List<IMyLandingGear> LandingGears { get; } = new();
        public List<IMyGasTank> TankBlocks { get; set; } = new();
        public List<IMyTerminalBlock> CruiseThrottle { get; } = new();
        public List<List<VectorThrust>> VTThrottleGroups { get; } = new();
        public List<Surface> Surfaces { get; set; } = new();
        public List<IMyThrust> VTThrusters { get; set; } = new();
        public List<IMyMotorStator> VTRotors { get; set; } = new();
        public List<IMyBatteryBlock> TaggedBatteries { get; set; } = new();
        public List<IMyBatteryBlock> NormalBatteries { get; set; } = new();
        public List<IMyThrust> ThrustersInput { get; set; } = new();
        public List<IMyMotorStator> RotorsInput { get; set; } = new();
        public List<ShipController> ControllersInput { get; } = new();
        public List<IMyTextPanel> InputScreens { get; set; } = new();
        public List<IMyMotorStator> AbandonedRotors { get; } = new();
        public List<IMyThrust> AbandonedThrusters { get; } = new();
        public List<IMySoundBlock> SoundBlocks { get; set; } = new();
        public List<IMyShipConnector> ConnectorBlocks { get; set; } = new();
        public List<IMyLandingGear> LandingGearBlocks { get; set; } = new();
        public List<IMyBatteryBlock> BatteriesBlocks { get; set; } = new();
        public List<IMyTerminalBlock> BatteriesSequence { get; set; } = new();
        public List<IMyTerminalBlock> AllBlocks { get; } = new();
        public List<double> OutputBatsSeq { get; set; } = new();
        public List<double> Tets { get; set; } = new();
        public List<int> TDividers { get; set; } = new() { 1, 1 };
        public Dictionary<string, object> CMInputs { get; set; }

        public RuntimeCollections()
        {
            
        }

    }
}
