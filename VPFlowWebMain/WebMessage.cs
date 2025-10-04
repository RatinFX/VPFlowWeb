using Newtonsoft.Json;
using System.Collections.Generic;

namespace VPFlowWebMain
{
    internal class WebMessage
    {
        public string Sender { get; set; }
        public object Payload { get; set; }
    }

    internal enum SenderType
    {
        BtnApply,
        BtnOther
    }

    internal class WebMessage<T>
        where T : BasePayload
    {
        public SenderType Sender { get; set; }
        public T Payload { get; set; }
        public WebMessage(SenderType sender, object payload)
        {
            Sender = sender;
            Payload = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(payload));
        }
    }

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
