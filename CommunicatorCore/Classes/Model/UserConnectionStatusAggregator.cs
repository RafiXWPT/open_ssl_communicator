using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class UserConnectionStatusAggregator: INetworkMessage
    {
        public List<UserConnectionStatus> ConnectionStatus = new List<UserConnectionStatus>();

        public UserConnectionStatusAggregator()
        {
        }

        public UserConnectionStatusAggregator(List<UserConnectionStatus> connectionStatus)
        {
            this.ConnectionStatus = connectionStatus;
        }

        public void LoadJson(string jsonString)
        {
            UserConnectionStatusAggregator tmp = JsonConvert.DeserializeObject<UserConnectionStatusAggregator>(jsonString);
            this.ConnectionStatus = tmp.ConnectionStatus;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
