using IngameScript.VectorThrustOSV2.Configuration;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOSV2.Runtime
{
    internal class RuntimeTracker
    {
        /** Public Properties **/
        public double MaxRuntime { get; private set; }
        public double AverageRuntime { get; private set; }
        public double LastRuntime { get; private set; }
        public bool CanPrint { get; private set; }
        public bool JustPrinted { get; private set; }
        public int FrameCount { get; private set; }
        public int MaxFrameCapacity { get; private set; }

        /** Private Properties **/
        private int FramePrintCount { get; set; }
        private ExponentialMovingAverage ExpMovingAvg { get; set; }

        /** Readonly properties **/
        private readonly MyGridProgram _baseProgram;
        private readonly ThrustOSConfig _thrustOSConfig;

        private readonly Dictionary<UpdateFrequency, int> _updateFrequencyToFrameCount = new Dictionary<UpdateFrequency, int>
        {
            { UpdateFrequency.Update1, 1 },
            { UpdateFrequency.Update10, 10 },
            { UpdateFrequency.Update100, 100 }
        };

        /** Private Methods **/
        private bool CurrentFrameCountExceedsMaxFrameCapacity() => FrameCount >= MaxFrameCapacity;
        private bool ProgramRuntimeIsUpdateFrequency(UpdateFrequency frequency) => _baseProgram.Runtime.UpdateFrequency == frequency;

        public RuntimeTracker(MyGridProgram baseProgram, ThrustOSConfig thrustOSConfig, int avgFrameCap, int maxFrameCap)
        {
            FrameCount          = 0;
            FramePrintCount     = 0;
            _baseProgram        = baseProgram;
            _thrustOSConfig     = thrustOSConfig;
            MaxFrameCapacity    = maxFrameCap;
            ExpMovingAvg        = new ExponentialMovingAverage(avgFrameCap);
        }

        public void ChangeAvgFrameCapacity(int newAvgFrameCapacity) => ExpMovingAvg = new ExponentialMovingAverage(newAvgFrameCapacity);

        public void Process()
        {
            this.LastRuntime = _baseProgram.Runtime.LastRunTimeMs;

            this.ExpMovingAvg.AddDataPoint(LastRuntime);
            this.AverageRuntime = ExpMovingAvg.Average;

            // Update MaxRuntime: reset to average every MaxFrameCapacity frames, otherwise track the highest recent runtime.
            if (FrameCount % MaxFrameCapacity == 0) this.MaxRuntime = this.AverageRuntime;
            else if (_baseProgram.Runtime.LastRunTimeMs > this.MaxRuntime) this.MaxRuntime = this.LastRuntime;

            if (CurrentFrameCountExceedsMaxFrameCapacity()) FrameCount = 0;
            else
            {
                FrameCount      += _updateFrequencyToFrameCount.GetValueOrDefault(_baseProgram.Runtime.UpdateFrequency);
                FramePrintCount += _updateFrequencyToFrameCount.GetValueOrDefault(_baseProgram.Runtime.UpdateFrequency);
            }

            if (CanPrint) JustPrinted = true;
            else if (JustPrinted) JustPrinted = false;

            CanPrint = FramePrintCount >= _thrustOSConfig.FramesPerPrint;

            if (CanPrint) FramePrintCount = 0;
        }
    }
}