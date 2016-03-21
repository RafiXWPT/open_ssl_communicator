using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Model
{
    public class Contact: INetworkMessage
    {
        public string From;
        public string To;
        public string DisplayName;
        public string ContactChecksum;

        public Contact()
        {
        }

        public Contact(string from, string to) : this(from, to, null) { }

        public Contact(string from, string to, string displayName)
        {
            From = from;
            To = to;
            DisplayName =  string.IsNullOrWhiteSpace(displayName) ? to : displayName;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(From + To + DisplayName);
                byte[] hash = md5.ComputeHash(inputBytes);
                ContactChecksum = Encoding.ASCII.GetString(hash);
            }
            
        }

        public void LoadJson(string jsonString)
        {
            try
            {
                Contact tmp = JsonConvert.DeserializeObject<Contact>(jsonString);
                this.From = tmp.From;
                this.To = tmp.To;
                this.DisplayName = tmp.DisplayName;
            }
            catch
            {

            }
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return "From: " + From + ", To: " + To + ", Display name: " + DisplayName;
        }
    }


}
