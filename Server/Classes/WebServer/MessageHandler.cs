using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SystemMessage;

namespace Server
{
    public class MessageHandler
    {
        public void handleMessage(HttpListenerContext messageToHandle)
        {
            string wantedUrl = messageToHandle.Request.RawUrl;
            string sender = messageToHandle.Request.RemoteEndPoint.Address.ToString();
            string response = string.Empty;
            try
            {
                Message message = new Message();
                message.loadJson(messageToHandle.Request.Headers["messageContent"]);

                if (wantedUrl == "/connectionCheck/")
                {
                    connectionCheck(message, sender, out response);
                }
                else if (wantedUrl == "/diffieTunnel/")
                {
                    diffieTunnel(message, out response);
                }
                else if (wantedUrl == "/register/")
                {

                }
                else if (wantedUrl == "/logIn/")
                {
                    response = "loguj sie";
                }
                else if(wantedUrl == "/sendChatMessage/")
                {
                    if(message.MessageContent != null)
                    {
                        response = "ECHO: " + message.MessageContent;
                    }
                }
                else
                {
                    response = "404";
                }

                sendResponse(messageToHandle, response);
            }
            catch { }

            closeResponseStream(messageToHandle);
        }

        void connectionCheck(Message message, string sender, out string response)
        {
            response = string.Empty;
            User usr = UserControll.Instance.getUserFromDatabase(message.MessageSender);
            if(usr != null)
            {
                usr.updateLastConnectionCheck(DateTime.Now);
                usr.updateAddress(sender);
            }

            Console.WriteLine("Client: " + sender + " is checking connection.");
            if (message.MessageType == "CHECK_CONNECTION" && message.MessageContent == "CONN_CHECK")
            {
                Console.WriteLine("Connection check was confirmed for: " + sender);
                response = "CONN_AVAIL";
            }
            else
            {
                Console.WriteLine("Connection check message of " + sender + " has bad content");
                response = "BAD_CONTENT";
            }
        }

        void diffieTunnel(Message message, out string response)
        {
            response = string.Empty;
            User user = null;

            if (message.MessageSender == "UNKNOWN")
            {
                string newGuid = "TMPUSER_" + Guid.NewGuid().ToString();
                user = new User(newGuid, "UNKNOWN");
                UserControll.Instance.addTemporaryUserToDatabase(user);
            }
            else
            {
                user = UserControll.Instance.getUserFromDatabase(message.MessageSender);
            }

            if(message.MessageType == "REQUEST_FOR_ID")
            {
                Console.WriteLine(message.MessageSender + " request for id.");
                response = user.ID;
            }
            else if(message.MessageType == "PUBLIC_KEY_EXCHANGE")
            {
                Console.WriteLine(message.MessageSender + " exchange pkey.");
                user.Tunnel.CreateKey(message.MessageContent);
                response = user.Tunnel.getPublicPart();  
            }
            else if(message.MessageType == "IV_EXCHANGE")
            {
                Console.WriteLine(message.MessageSender + " exchange iv.");
                user.Tunnel.loadIV(message.MessageContent);
                response = "CHECK";
            }
            else if(message.MessageType == "CHECK_TUNNEL")
            {
                Console.WriteLine(message.MessageSender + " checking tunnel.");
                if(user.Tunnel.diffieDecrypt(message.MessageContent) == "OK")
                {
                    response = "RDY";
                    user.Tunnel.Established = true;
                }
                else
                {
                    response = "BAD_TUNNEL";
                }
            }
            else
            {
                response = "404";
            }
        }

        void sendResponse(HttpListenerContext context, string response)
        {
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch { }
        }

        void closeResponseStream(HttpListenerContext context)
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch { }
        }

    }
}
