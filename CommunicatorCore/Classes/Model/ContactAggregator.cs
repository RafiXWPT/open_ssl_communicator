﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommunicatorCore.Classes.Model
{
    public class ContactAggregator: INetworkMessage
    {
        public List<Contact> Contacts = new List<Contact>();

        public ContactAggregator()
        {
        }

        public ContactAggregator(List<Contact> contacts)
        {
            this.Contacts = contacts;
        }

        public void LoadJson(string jsonString)
        {
            ContactAggregator tmp = JsonConvert.DeserializeObject<ContactAggregator>(jsonString);
            this.Contacts = tmp.Contacts;
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
