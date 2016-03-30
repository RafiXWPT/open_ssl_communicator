using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class UserConnectionStatus: INetworkMessage
    {
        public string Username { get; set; }
        public string ConnectionStatus { get; set; }

        public UserConnectionStatus(string username)
        {
            this.Username = username;
        }

        public UserConnectionStatus()
        {
        }

        public void LoadJson(string jsonString)
        {
            UserConnectionStatus connectionStatus = JsonConvert.DeserializeObject<UserConnectionStatus>(jsonString);
            Username = connectionStatus.Username;
            ConnectionStatus = connectionStatus.ConnectionStatus;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(object obj)
        {
            return Username == ((UserConnectionStatus)obj).Username;
        }

        public override int GetHashCode()
        {
            return 31 + 17 * ( Username == null ? 0: Username.GetHashCode());
        }
    }
}
