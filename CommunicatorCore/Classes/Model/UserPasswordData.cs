using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Model
{
    public class UserPasswordData
    {
        public readonly string Username;
        public readonly string HashedPassword;

        public UserPasswordData(string username, string password)
        {
            this.Username = username;
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                this.HashedPassword = Encoding.UTF8.GetString(hashBytes);
            }
        }
    }
}
