using Newtonsoft.Json;
using System;
using VPFlowWebMain.Models;

namespace VPFlowWebMain.Lib
{
    internal class Messaging
    {
        internal static (SenderType sender, object payload) Process(string messageJson)
        {
            Logging.Log("Original messageJson: " + messageJson);

            var ogWebMessage = JsonConvert.DeserializeObject<WebMessageBase>(messageJson);
            if (ogWebMessage == null)
            {
                //MessageBox.Show("Web message handling error: invalid message format\n- " + messageJson);
                Logging.Error("Web message handling error: invalid message format: " + messageJson);
                return null;
            }

            var senderConvertionSuccess = Enum.TryParse(ogWebMessage.Sender, true, out SenderType sender);
            if (!senderConvertionSuccess)
            {
                //MessageBox.Show("Web message handling error: unknown sender\n- " + ogWebMessage.Sender);
                Logging.Error("Web message handling error: unknown sender: " + ogWebMessage.Sender);
                return null;
            }

            return (sender, ogWebMessage.Payload);
        }

        internal static WebMessageBase<T> CreateWebMessage<T>(SenderType sender, object payload)
            where T : BasePayload
        {
            return new WebMessageBase<T>(sender, payload);
        }
    }
}
