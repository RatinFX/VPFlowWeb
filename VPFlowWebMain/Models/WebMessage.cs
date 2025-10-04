using Newtonsoft.Json;

namespace VPFlowWebMain.Models
{
    internal class WebMessageBase
    {
        public string MessageType { get; set; }
        public object Payload { get; set; }
    }

    internal enum MessageType
    {
        Apply,
        Settings
    }

    internal class WebMessage<T>
        where T : BasePayload
    {
        public MessageType MessageType { get; set; }
        public T Payload { get; set; }
        public WebMessage(MessageType messageType, object payload)
        {
            MessageType = messageType;
            Payload = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(payload));
        }
    }
}
