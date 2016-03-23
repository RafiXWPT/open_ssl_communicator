﻿using Newtonsoft.Json;
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
            ContactChecksum = Sha1Util.CalculateSha(From + To);
        }

        public void LoadJson(string jsonString)
        {
            Contact tmp = JsonConvert.DeserializeObject<Contact>(jsonString);
            this.From = tmp.From;
            this.To = tmp.To;
            this.DisplayName = tmp.DisplayName;
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
