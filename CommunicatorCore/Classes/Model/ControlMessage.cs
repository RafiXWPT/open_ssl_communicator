using System;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class ControlMessage: INetworkMessage
    {
        public string MessageSender { get; set; }
        public string MessageType { get; set; }
        public string MessageContent { get; set; }
        public string Checksum { get; set; }

        public ControlMessage(string messageSender, string messageType = "NO_TYPE", string plainMessageContent = "NO_CONTENT",
            string encryptedMessage = null)
        {
            MessageSender = messageSender;
            MessageType = messageType;
            MessageContent = encryptedMessage ?? plainMessageContent;
            Checksum = Sha1Util.CalculateSha(plainMessageContent);
        }

        public ControlMessage()
        {}

        public void LoadJson(string jsonString)
        {
            ControlMessage tmp = JsonConvert.DeserializeObject<ControlMessage>(jsonString);
            this.MessageSender = tmp.MessageSender;
            this.MessageType = tmp.MessageType;
            this.MessageContent = tmp.MessageContent;
            this.Checksum = tmp.Checksum;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}