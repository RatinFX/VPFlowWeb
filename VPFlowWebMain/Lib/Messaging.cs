using Newtonsoft.Json;
using System;
using VPFlowWebMain.Models;

namespace VPFlowWebMain.Lib
{
    internal class Messaging
    {
        internal static (SenderType? senderType, object payload) Process(string messageJson)
        {
            Logging.Log("Original messageJson: " + messageJson);

            var ogWebMessage = JsonConvert.DeserializeObject<WebMessageBase>(messageJson);
            if (ogWebMessage == null)
            {
                //MessageBox.Show("Web message handling error: invalid message format\n- " + messageJson);
                Logging.Error("Web message handling error: invalid message format: " + messageJson);
                return (null, null);
            }

            var senderConvertionSuccess = Enum.TryParse(ogWebMessage.SenderType, true, out SenderType senderType);
            if (!senderConvertionSuccess)
            {
                //MessageBox.Show("Web message handling error: unknown sender\n- " + ogWebMessage.Sender);
                Logging.Error("Web message handling error: unknown sender: " + ogWebMessage.SenderType);
                return (null, null);
            }

            return (senderType, ogWebMessage.Payload);
        }

        internal static WebMessage<T> CreateWebMessage<T>(SenderType senderType, object payload)
            where T : BasePayload
        {
            return new WebMessage<T>(senderType, payload);
        }
    }
}
