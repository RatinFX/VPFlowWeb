using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal abstract class BasePayload { }

    internal class ApplyPayload : BasePayload
    {
        public List<float> Coordinates { get; set; }
    }

    internal class OtherPayload : BasePayload
    {
        public string SomeData { get; set; }
        public string SomeData2 { get; set; }
    }
}
