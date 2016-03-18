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
                    Console.WriteLine("Client: " + sender + " is checking connection.");
                    if(message.MessageType == "CHECK_CONNECTION" && message.MessageContent == "CONN_CHECK")
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
                else if (wantedUrl == "/register/")
                {
                    response = "rejestruj sie";
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
