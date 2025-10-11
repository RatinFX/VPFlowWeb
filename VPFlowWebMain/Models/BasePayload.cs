using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal abstract class BasePayload { }

    internal class ApplyPayload : BasePayload
    {
        public List<Point> points { get; set; }
    }

    public class Point
    {
        public string id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public Vector2 handleOut { get; set; }
        public Vector2 handleIn { get; set; }
    }

    public class Vector2
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
