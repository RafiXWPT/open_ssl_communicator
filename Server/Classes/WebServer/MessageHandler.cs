using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SystemMessage;
using SystemSecurity;

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
                    register(message, out response);
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
                    response = string.Empty;
                }

                sendResponse(messageToHandle, response);
            }
            catch { }

            closeResponseStream(messageToHandle);
        }

        void connectionCheck(Message message, string sender, out string response)
        {
            response = string.Empty;
            User user = UserControll.Instance.getUserFromDatabase(message.MessageSender);
            if (user != null)
            {
                user.updateLastConnectionCheck(DateTime.Now);
                user.updateAddress(sender);
            }

            if (message.MessageType == "CHECK_CONNECTION" && message.MessageContent == "CONN_CHECK")
            {
                response = "CONN_AVAIL";
            }
            else
            {
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
                    user.Tunnel.Status = DiffieHellmanTunnelStatus.ESTABLISHED;
                }
                else
                {
                    response = "BAD_TUNNEL";
                }
            }
            else
            {
                response = string.Empty;
            }
        }

        void register(Message message, out string response)
        {
            response = string.Empty;
            User user = null;

            user = UserControll.Instance.getUserFromDatabase(message.MessageSender);

            if (user != null)
            {
                if(message.MessageType == "REGISTER_ME")
                {
                    Console.WriteLine(message.MessageSender + " is about to register.");
                    string[] registrationInfo = user.Tunnel.diffieDecrypt(message.MessageContent).Split('|');
                    string login = registrationInfo[0];
                    string password = registrationInfo[1];
                    string email = registrationInfo[2];

                    Console.WriteLine("Login: " + login);
                    Console.WriteLine("Password: " + password);
                    Console.WriteLine("Mail: " + email);
                    response = "ECHO: " + login + "," + password + "," + email ;
                }
            }
            else
                return;
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
