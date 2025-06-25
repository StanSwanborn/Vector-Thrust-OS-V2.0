using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Architecture.Interfaces
{
    internal interface IBlockWrapper
    {
        IMyTerminalBlock TheBlock { get; set; }
        string CName { get; }
    }
}