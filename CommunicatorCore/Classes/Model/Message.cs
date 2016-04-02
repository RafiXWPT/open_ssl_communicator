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
        public string MessageCipheredContent { get; set; }
        public DateTime MessageDate { get; set; }
        public string Checksum { get; set; }

        public Message(string uid, string sender, string destination, string content): this(uid, sender, destination, content, content) { }

        public Message(string uid, string sender, string destination, string content, string messageCipheredContent)
            : this(uid, sender, destination, content, messageCipheredContent, DateTime.Now)
        {
        }


        public Message(string uid, string sender, string destination, string content, string messageCipheredContent,
            DateTime datetime)
        {
            this.MessageUID = uid;
            this.MessageSender = sender;
            this.MessageDestination = destination;
            this.MessageContent = content;
            this.MessageCipheredContent = messageCipheredContent;
            this.MessageDate = datetime;
            this.Checksum = Sha1Util.CalculateSha(content);
        }

        public Message() { }

        public void LoadJson(string jsonString)
        {
            Message tmp = JsonConvert.DeserializeObject<Message>(jsonString);
            this.MessageUID = tmp.MessageUID;
            this.MessageSender = tmp.MessageSender;
            this.MessageDestination = tmp.MessageDestination;
            this.MessageContent = tmp.MessageContent;
            this.MessageCipheredContent = tmp.MessageCipheredContent;
            this.MessageDate = tmp.MessageDate;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
