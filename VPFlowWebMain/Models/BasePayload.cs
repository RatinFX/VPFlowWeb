using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal abstract class BasePayload { }

    internal class ApplyPayload : BasePayload
    {
        public List<float> Coordinates { get; set; }
    }

    internal class SettingsPayload : BasePayload
    {
        public string Theme { get; set; }
        public bool DisplayLogs { get; set; }
        public bool CheckForUpdatesOnStart { get; set; }
        public bool IgnoreLongSectionWarning { get; set; }
        public bool OnlyCreateNecessaryKeyframes { get; set; }
    }
}
