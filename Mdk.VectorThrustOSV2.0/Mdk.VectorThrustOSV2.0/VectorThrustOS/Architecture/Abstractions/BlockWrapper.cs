using IngameScript.VectorThrustOS.Architecture.Interfaces;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Architecture.Abstractions
{
    abstract class BlockWrapper<T> : IBlockWrapper where T : class, IMyTerminalBlock
    {
        public T TheBlock { get; set; }

        public MyGridProgram Program { get; private set; }

        public BlockWrapper(T block, MyGridProgram program)
        {
            Program = program;
            TheBlock = block;
        }

        // not allowed for some reason
        IMyTerminalBlock IBlockWrapper.TheBlock
        {
            get { return TheBlock; }
            set { TheBlock = (T)value; }
        }

        public string CName => TheBlock.CustomName;
    }
}
