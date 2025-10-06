using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal abstract class BasePayload { }

    internal class ApplyPayload : BasePayload
    {
        public List<float> coordinates { get; set; }
    }

    internal class SettingsPayload : BasePayload
    {
        public string theme { get; set; }
        public bool displayLogs { get; set; }
        public bool checkForUpdatesOnStart { get; set; }
        public bool ignoreLongSectionWarning { get; set; }
        public bool onlyCreateNecessaryKeyframes { get; set; }
    }
}
