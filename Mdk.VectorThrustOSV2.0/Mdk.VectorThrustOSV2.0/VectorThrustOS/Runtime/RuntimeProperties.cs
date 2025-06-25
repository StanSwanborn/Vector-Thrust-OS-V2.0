using IngameScript.VectorThrustOS.Architecture.Interfaces;
using IngameScript.VectorThrustOS.Controllers;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.VectorThrustOS.Runtime
{
    internal class RuntimeProperties : ILoadFromStorage
    {
        /** Ship / ShipController properties **/
        public ShipController MainController { get; set; }
        public MyShipMass MyShipMass { get; set; }

        /** Main Properties **/
        public bool DampChanged { get; set; }
        public bool Parked { get; set; }
        public bool AlreadyParked { get; set; }
        public bool CruisedNT { get; set; }
        public bool SetTOV { get; set; }
        public bool TagAll { get; set; }
        public bool Error { get; set; }
        public bool OldDampeners { get; set; }
        public bool IsStation { get; set; }
        public bool TrulyParked { get; set; }
        public bool ParkAvailable { get; set; }
        public bool AlmostBraked { get; set; }
        public bool Cruise { get; set; }
        public bool Dampeners { get; set; }
        public bool DampenersIsPressed { get; set; }
        public bool CruiseIsPressed { get; set; }
        public bool GearIsPressed { get; set; }
        public bool AllowParkIsPressed { get; set; }
        public bool RotorsStopped { get; set; }
        public bool JustCompiled { get; set; }
        public bool PauseSequence { get; set; }
        public bool Check { get; set; }
        public bool ApplyTags { get; set; }
        public bool Greedy { get; set; }
        public bool ThrustOn { get; set; }
        public bool RechargeCancelled { get; set; }
        public bool ParkedWithConnector { get; set; }
        public bool UnparkedCompletely { get; set; }
        public bool ParkedCompletely { get; set; }
        public bool ChangedRuntime { get; set; }
        public bool ForceUnpark { get; set; }
        public bool CruiseByArg { get; set; }
        public bool ChangeDampeners { get; set; }
        public bool Global_GravChanged { get; set; }

        /** Useful Properties **/
        public string OldTag { get; set; }
        public string Tag { get; set; }
        public string TextSurfaceKeyword { get; set; }
        public string LCDName { get; set; }

        /** Calculated Steering Properties **/
        public const double TOVval = 0.25;
        public double Sv { get; set; }
        public double GravityStrength { get; set; }
        public double Mvin { get; set; }
        public double Accel { get; set; }
        public double MaxAccel { get; set; }
        public double TotalEffectiveThrust { get; set; }
        public double TotalVTThrPrecision { get; set; }
        public double Force { get; set; }
        public double DisplayGearAccel { get; set; }
        public double TgotTOV { get; set; }
        public double RawGearAccel { get; set; }
        public double Len { get; set; }
        public double GlobalThrustByThr { get; set; }
        public int SkipFrame { get; set; }
        public int UpdatesPerSecond { get; private set; }
        public int BlockCount { get; set; }

        public RuntimeProperties()
        {
            // Main properties to be set true by default
            this.Dampeners = true;
            this.JustCompiled = true;
            this.Check = true;
            this.Greedy = true;
            this.ThrustOn = true;
            this.UnparkedCompletely = true;

            // Useful properties to be initialized with default values
            OldTag = "";
            Tag = "|VT|";
            TextSurfaceKeyword = "VT:";
            LCDName = "VTLCD";

            // Calculated steering properties to be initialized with default values
            Sv = 0;
            GravityStrength = 0;
            Mvin = 0;
            Accel = 0;
            MaxAccel = 0;
            TotalEffectiveThrust = 0;
            TotalVTThrPrecision = 0;
            Force = 0;
            DisplayGearAccel = 0;
            TgotTOV = 0;
            RawGearAccel = 0;
            Len = 0;
            GlobalThrustByThr = 0;
            SkipFrame = 0;
            UpdatesPerSecond = 60;
            BlockCount = 0;
        }

        public void LoadFromStorage(string[] storageParts)
        {
            if (storageParts.Length > 0)
            {
                var tagParts = storageParts[0].Split(':');

                if (tagParts.Length == 2)
                {
                    Tag = tagParts[0];
                    Greedy = bool.Parse(tagParts[1]);
                }
            }

            if (storageParts.Length > 3)
                Cruise = bool.Parse(storageParts[3]);
        }
    }
}