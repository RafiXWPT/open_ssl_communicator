using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using CommunicatorCore.Classes.Model;

namespace Client
{
    public class NetworkController
    {
        private static NetworkController _instance;
        public static NetworkController Instance
        {
            get
            {
                return _instance;
            }
        }

        public NetworkController()
        {
            _instance = this;
        }

        public string SendMessage(Uri address, WebClient client, ControlMessage message)
        {
            NameValueCollection headers = new NameValueCollection();

            if (client.Headers.Count > 0)
                client.Headers.Clear();

            headers["messageContent"] = message.GetJsonString();
            client.Headers.Add(headers);

            return ReadMessage(address, client);
        }

        string ReadMessage(Uri address, WebClient client)
        {
            NameValueCollection data = new NameValueCollection();
            data["data"] = DateTime.Now.ToShortDateString();

            byte[] response = client.UploadValues(address, "POST", data);
            return Encoding.UTF8.GetString(response);
        }
    }
}
