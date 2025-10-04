using Newtonsoft.Json;
using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    internal class WebMessageBase
    {
        public string SenderType { get; set; }
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
        public SenderType SenderType { get; set; }
        public T Payload { get; set; }
        public WebMessage(SenderType senderType, object payload)
        {
            SenderType = senderType;
            Payload = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(payload));
        }
    }
}
