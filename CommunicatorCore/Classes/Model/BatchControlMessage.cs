using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class BatchControlMessage: INetworkMessage
    {
        public ControlMessage ControlMessage { get; set; }
        public string CipheredKey { get; set; }

        public BatchControlMessage()
        {
            
        }

        public BatchControlMessage(ControlMessage message, string key)
        {
            ControlMessage = message;
            CipheredKey = key;
        }

        public void LoadJson(string jsonString)
        {
            BatchControlMessage tmp = JsonConvert.DeserializeObject<BatchControlMessage>(jsonString);
            this.ControlMessage = tmp.ControlMessage;
            this.CipheredKey = tmp.CipheredKey;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
