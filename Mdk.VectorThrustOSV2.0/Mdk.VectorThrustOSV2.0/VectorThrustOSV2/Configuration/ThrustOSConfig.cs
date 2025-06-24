using System.Collections.Generic;

namespace IngameScript.VectorThrustOSV2.Configuration
{
    internal class ThrustOSConfig
    {
        public string MyName { get; set; }
        public List<double> Aggressivity { get; set; }
        public float ErrorMargin { get; set; }
        public double LowThrustCutOn { get; set; }
        public double LowThrustCutOff { get; set; }
        public double LowThrustCutCruiseOn { get; set; }
        public double LowThrustCutCruiseOff { get; set; }
        public double VelPrecisionMode { get; set; }
        public List<double> Accelerations { get; set; }
        public int Gear { get; set; }
        public double GearAccel { get; set; }
        public bool TurnOffThrustersOnPark { get; set; }
        public bool RechargeOnPark { get; set; }
        public string BackupSubstring { get; set; }
        public bool RenameBackupSubstring { get; set; }
        public bool PerformanceWhilePark { get; set; }
        public bool AutoAddGridConnectors { get; set; }
        public bool AutoAddGridLandingGears { get; set; }
        public bool ForceParkIfStatic { get; set; }
        public bool AllowPark { get; set; }
        public List<double> ThrDirMultiplier { get; set; }
        public bool ThrDirOverride { get; set; }
        public double ThrusterModifier { get; private set; }
        public string[] TagSurround { get; set; }
        public bool CruisePlane { get; set; }
        public int FramesBetweenActions { get; set; }
        public bool ShowMetrics { get; set; }
        public int SkipFrames { get; set; }
        public int FramesPerPrint { get; set; }
        public bool StockValues { get; set; }
        public bool OnlyMainCockpit { get; set; }

        public ThrustOSConfig()
        {
            // Default configuration values
            MyName                  = "VT";
            Aggressivity            = new List<double> { 0.1, 1, 4 };
            ErrorMargin             = 6f;
            LowThrustCutOn          = 0.5;
            LowThrustCutOff         = 0.01;
            LowThrustCutCruiseOn    = 1;
            LowThrustCutCruiseOff   = 0.15;

            VelPrecisionMode        = 1;

            Accelerations           = new List<double> { 15, 50, 100 };
            Gear = 0;
            GearAccel = 0;

            TurnOffThrustersOnPark  = true;
            RechargeOnPark          = true;
            BackupSubstring         = "Backup";
            RenameBackupSubstring   = true;
            PerformanceWhilePark    = false;
            AutoAddGridConnectors   = false;
            AutoAddGridLandingGears = false;
            ForceParkIfStatic       = true;
            AllowPark               = false;
            ThrDirMultiplier        = new List<double> { -1, -1, 0 };
            ThrDirOverride          = false;

            ThrusterModifier        = 0.0000000001;
            TagSurround             = new string[] { "|", "|" };
            CruisePlane             = false;
            FramesBetweenActions    = 1;
            ShowMetrics             = false;
            SkipFrames              = 0;
            FramesPerPrint          = 10;
            StockValues             = true;
            OnlyMainCockpit         = false;
        }
    }
}