using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using CommunicatorCore.Classes.Model;
using System.Threading;
using System.Net.Sockets;
using System.Windows;

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

        private readonly HttpListener _listener = new HttpListener();

        string GetIPv4Address()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress i in ips)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                {
                    return i.ToString();
                }
            }
            return "localhost";
        }

        public bool IsChatListening { get; set; }
        ManualResetEvent listenintChat_event;
        Thread chatListener;

        public NetworkController()
        {
            _instance = this;
        }

        public void StartChatListener()
        {
            listenintChat_event = new ManualResetEvent(false);
            chatListener = new Thread(Listening_thread);
            chatListener.IsBackground = true;
            IsChatListening = true;
            chatListener.Start();
        }

        public void StopChatListener()
        {
            IsChatListening = false;
            listenintChat_event.Set();
        }

        void Listening_thread()
        {
            _listener.Prefixes.Add("http://"+GetIPv4Address()+":11070/chatMessage/");
            _listener.Start();
            RunChatService(); 
        }

        void RunChatService()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            HttpListenerContext ctx = c as HttpListenerContext;
                            HandleIncommingMessage(ctx);

                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

        void HandleIncommingMessage(HttpListenerContext serverMessage)
        {
            string wantedUrl = serverMessage.Request.RawUrl;
            string messageContent = serverMessage.Request.Headers["messageContent"];
            MessageBox.Show(messageContent);
            try
            {
                if (wantedUrl == "/chatMessage/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    HandleChat(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            SendResponse(serverMessage, "RECEIVED");
        }

        void SendResponse(HttpListenerContext context, string response)
        {
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch (Exception ex)
            {

            }

            CloseResponseStream(context);
        }

        void CloseResponseStream(HttpListenerContext context)
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private ControlMessage ParseMessageContent(string messageContent)
        {
            ControlMessage message = new ControlMessage();
            message.LoadJson(messageContent);
            return message;
        }

        void HandleChat(ControlMessage message)
        {
            ChatController.DeliverToChatWindow(message);
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
