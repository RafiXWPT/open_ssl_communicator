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
        public string To { get; set; }
        public string DisplayName { get; set; }

        public string From;
        public string ContactChecksum;
        public string CipheredTo;

        public Contact()
        {
        }

        public Contact(string from, string to) : this(from, to, to) { }

        public Contact(string from, string to, string displayName)
        {
            this.From = from;
            To = to;
            DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName : to;
            this.ContactChecksum = Sha1Util.CalculateSha(From + To);
        }

        public void LoadJson(string jsonString)
        {
            Contact tmp = JsonConvert.DeserializeObject<Contact>(jsonString);
            From = tmp.From;
            To = tmp.To;
            DisplayName = tmp.DisplayName;
            CipheredTo = tmp.CipheredTo;
            ContactChecksum = tmp.ContactChecksum;
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
