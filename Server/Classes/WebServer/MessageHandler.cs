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
            string response = string.Empty;
            try
            {
                Console.WriteLine(messageToHandle.Request.Headers["messageContent"]);
                Message message = new Message();
                message.loadJson(messageToHandle.Request.Headers["messageContent"]);
                Console.WriteLine(message.MessageType);
                Console.WriteLine(message.MessageSender);
                Console.WriteLine(message.MessageDestination);
                Console.WriteLine(message.MessageContent);
                if (wantedUrl == "/connectionCheck/")
                {
                    response = "spr. polaczenie";
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
                    response = "wyslij chat message";
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
