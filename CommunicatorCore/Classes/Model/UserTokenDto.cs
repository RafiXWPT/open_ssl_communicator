using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class UserTokenDto: INetworkMessage
    {
        public string Username { get; set; }
        public string HashedToken { get; set; }

        public UserTokenDto()
        {
        }

        public UserTokenDto(string username, string token)
        {
            Username = username;
            HashedToken = Sha1Util.CalculateSha(token);
        }

        public void LoadJson(string jsonString)
        {
            UserTokenDto userTokenDto = JsonConvert.DeserializeObject<UserTokenDto>(jsonString);
            Username = userTokenDto.Username;
            HashedToken = userTokenDto.HashedToken;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
