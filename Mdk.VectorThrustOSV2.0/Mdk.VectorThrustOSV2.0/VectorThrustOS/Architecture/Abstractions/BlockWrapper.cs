using IngameScript.VectorThrustOS.Architecture.Interfaces;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Architecture.Abstractions
{
    abstract class BlockWrapper<T> : IBlockWrapper where T : class, IMyTerminalBlock
    {
        public T TheBlock { get; set; }

        public Program p;

        public BlockWrapper(T block, Program p)
        {
            this.p = p;
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
