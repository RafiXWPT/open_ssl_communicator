using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class UserPasswordData: INetworkMessage
    {
        public string Username;
        public string HashedPassword;

        public UserPasswordData(string username, string password)
        {
            this.Username = username;
            this.HashedPassword = Sha1Util.CalculateSha(password);
        }

        public UserPasswordData()
        {
        }

        public void LoadJson(string jsonString)
        {
            UserPasswordData tmp = JsonConvert.DeserializeObject<UserPasswordData>(jsonString);
            this.Username = tmp.Username;
            this.HashedPassword = tmp.HashedPassword;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
