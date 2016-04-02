using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class MessageAggregator: INetworkMessage
    {
        public List<Message> Messages = new List<Message>();

        public MessageAggregator(List<Message> messages)
        {
            Messages = messages;
        }

        public MessageAggregator()
        {
        }

        public void LoadJson(string jsonString)
        {
            MessageAggregator tmp = JsonConvert.DeserializeObject<MessageAggregator>(jsonString);
            this.Messages = tmp.Messages;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
