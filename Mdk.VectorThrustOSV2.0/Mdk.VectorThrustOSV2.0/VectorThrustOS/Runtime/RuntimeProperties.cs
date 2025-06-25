using IngameScript.VectorThrustOS.Architecture.Interfaces;

namespace IngameScript.VectorThrustOS.Runtime
{
    internal class RuntimeProperties : ILoadFromStorage
    {
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
        public bool PauseSeq { get; set; }
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