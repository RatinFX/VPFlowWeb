using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal abstract class BasePayload { }

    internal class ApplyPayload : BasePayload
    {
        public List<PayloadPoint> points { get; set; }
    }

    public class PayloadPoint
    {
        public string id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public Point handleOut { get; set; }
        public Point handleIn { get; set; }
    }

    public class Point
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    internal enum SelectedMode
    {
        Event = 0,
        Track = 1
    }

    internal class SettingsPayload : BasePayload
    {
        public string theme { get; set; }
        public bool displayLogs { get; set; }
        public bool checkForUpdatesOnStart { get; set; }
        public bool ignoreLongSectionWarning { get; set; }
        public bool onlyCreateNecessaryKeyframes { get; set; }
        public SelectedMode selectedMode { get; set; }
    }
}
