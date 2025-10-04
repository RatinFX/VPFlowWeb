using Newtonsoft.Json;
using System;
using VPFlowWebMain.Models;

namespace VPFlowWebMain.Lib
{
    internal class Messaging
    {
        internal static (MessageType? messageType, object payload) Process(string payload)
        {
            Logging.Log("Original payload: " + payload);

            var ogWebMessage = JsonConvert.DeserializeObject<WebMessageBase>(payload);
            if (ogWebMessage == null)
            {
                Logging.Error("Web message handling error: invalid message format: " + payload);
                return (null, null);
            }

            var messageTypeConvertionSuccess = Enum.TryParse(ogWebMessage.MessageType, true, out MessageType messageType);
            if (!messageTypeConvertionSuccess)
            {
                Logging.Error("Web message handling error: unknown messageType: " + ogWebMessage.MessageType);
                return (null, null);
            }

            return (messageType, ogWebMessage.Payload);
        }

        internal static WebMessage<T> CreateWebMessage<T>(MessageType messageType, object payload)
            where T : BasePayload
        {
            return new WebMessage<T>(messageType, payload);
        }
    }
}
