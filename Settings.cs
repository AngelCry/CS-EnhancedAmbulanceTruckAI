using System;
using System.Collections.Generic;

namespace EnhancedAmbulanceAI
{
    public sealed class Settings
    {
        private Settings()
        {
            Tag = "[ARIS] Enhanced Ambulance AI";

            DispatchGap = 5;
        }

        private static readonly Settings _Instance = new Settings();
        public static Settings Instance { get { return _Instance; } }

        public readonly string Tag;

        public readonly int DispatchGap;
    }
}