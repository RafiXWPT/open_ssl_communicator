using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Model
{
    public class Sha1Util
    {
        public static string CalculateSha(string text)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hashBytes);
            }
        }
    } 
}
