using System;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class ControlMessage: INetworkMessage
    {
        public string MessageSender { get; set; }
        public string MessageType { get; set; }
        public string MessageContent { get; set; }
        public DateTime MessageDate;

        public ControlMessage(string messageSender, string messageType = "NO_TYPE", string messageContent = "NO_CONTENT")
        {
            MessageSender = messageSender;
            MessageType = messageType;
            MessageContent = messageContent;
            MessageDate = DateTime.Now;
        }

        public ControlMessage()
        {}

        public void LoadJson(string jsonString)
        {
            try
            {
                ControlMessage tmp = JsonConvert.DeserializeObject<ControlMessage>(jsonString);
                this.MessageSender = tmp.MessageSender;
                this.MessageType = tmp.MessageType;
                this.MessageContent = tmp.MessageContent;
            }
            catch
            {

            }
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}