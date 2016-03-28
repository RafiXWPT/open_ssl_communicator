using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class ChangePasswordDTO: INetworkMessage
    {
        public string Username;
        public string CurrentPassword;
        public string NewPassword;

        public ChangePasswordDTO()
        {
        }

        public ChangePasswordDTO(string username, string password, string newPassword)
        {
            this.Username = username;
            this.CurrentPassword = Sha1Util.CalculateSha(password);
            this.NewPassword = Sha1Util.CalculateSha(newPassword);
        }

        public void LoadJson(string jsonString)
        {
            ChangePasswordDTO changePasswordDto = JsonConvert.DeserializeObject<ChangePasswordDTO>(jsonString);
            Username = changePasswordDto.Username;
            CurrentPassword = changePasswordDto.CurrentPassword;
            NewPassword = changePasswordDto.NewPassword;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
