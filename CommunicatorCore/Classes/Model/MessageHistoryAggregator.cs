using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class MessageHistoryAggregator: INetworkMessage
    {
        public List<Message> Messages = new List<Message>();

        public MessageHistoryAggregator(List<Message> messages)
        {
            Messages = messages;
        }

        public void LoadJson(string jsonString)
        {
            MessageHistoryAggregator tmp = JsonConvert.DeserializeObject<MessageHistoryAggregator>(jsonString);
            this.Messages = tmp.Messages;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
