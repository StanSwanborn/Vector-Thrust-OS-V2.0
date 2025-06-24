using IngameScript.VectorThrustOSV2;
using IngameScript.VectorThrustOSV2.Enums;
using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        readonly VectorThrustOS _vectorThrustOS;

        public Program()
        {
            Echo("Program() Start");

            _vectorThrustOS = new VectorThrustOS(this);

            // Manually call Load, which is typically called automatically.
            Load();

            // In original code, the blocks are initialized here.
            Echo("Program() End");
        }

        public void Save() => Storage = _vectorThrustOS.Save();
        public void Load() => _vectorThrustOS.Load(Storage);

        public void Main(string argument)
        {
            if (!Enum.TryParse(argument, true, out RunArgumentEnum runArgument))
                runArgument = RunArgumentEnum.NONE;

            _vectorThrustOS.Run(runArgument);
        }
    }
}