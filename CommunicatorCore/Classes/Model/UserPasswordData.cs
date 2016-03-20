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
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                HashedPassword = Encoding.UTF8.GetString(hashBytes);
            }
        }

        public UserPasswordData()
        {
        }

        public void LoadJson(string jsonString)
        {
            try
            {
                UserPasswordData tmp = JsonConvert.DeserializeObject<UserPasswordData>(jsonString);
                this.Username = tmp.Username;
                this.HashedPassword = tmp.HashedPassword;
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
