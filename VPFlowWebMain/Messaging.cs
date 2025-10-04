using Newtonsoft.Json;
using System;

namespace VPFlowWebMain
{
    internal class Messaging
    {
        static internal WebMessage<T> ProcessMessage<T>(string messageJson)
            where T : BasePayload
        {
            Logging.Log("Original messageJson: " + messageJson);

            var ogWebMessage = JsonConvert.DeserializeObject<WebMessage>(messageJson);
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

            return new WebMessage<T>(sender, ogWebMessage.Payload);
        }
    }
}
