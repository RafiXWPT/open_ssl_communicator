using System;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class Message: INetworkMessage
    {
        public string MessageUID { get; set; }
        public string MessageSender { get; set; }
        public string MessageDestination { get; set; }
        public string MessageContent { get; set; }
        public DateTime MessageDate { get; set; }

        public Message(string UID, string sender, string destionation, string content)
        {
            this.MessageUID = UID;
            this.MessageSender = sender;
            this.MessageDestination = destionation;
            this.MessageContent = content;
            this.MessageDate = DateTime.Now;
        }

        public Message() { }

        public void LoadJson(string jsonString)
        {
            Message tmp = JsonConvert.DeserializeObject<Message>(jsonString);
            this.MessageUID = tmp.MessageUID;
            this.MessageSender = tmp.MessageSender;
            this.MessageDestination = tmp.MessageDestination;
            this.MessageContent = tmp.MessageContent;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
