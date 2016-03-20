using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SystemSecurity;
﻿using CommunicatorCore.Classes.Model;
using Server.Classes;

namespace Server
{
    public class RequestHandler
    {
        public void HandleRequest(HttpListenerContext messageToHandle)
        {
            string wantedUrl = messageToHandle.Request.RawUrl;
            string sender = messageToHandle.Request.RemoteEndPoint.Address.ToString();
            string response = string.Empty;
            string messageContent = messageToHandle.Request.Headers["messageContent"];
            try
            {
                ControlMessage message = ParseMessageContent(messageContent);
                if (wantedUrl == "/connectionCheck/")
                {
                    ConnectionCheck(message, sender, out response);
                }
                else if (wantedUrl == "/diffieTunnel/")
                {
                    DiffieTunnel(message, out response);
                }
                else if (wantedUrl == "/register/")
                {
                    Register(message, out response);
                }
                else if (wantedUrl == "/logIn/")
                {
                    LogIn(message, out response);
                }
                else if(wantedUrl == "/sendChatMessage/")
                {
                    SendChatMessage(message, out response);
                }
                else if (wantedUrl == "/contacts/")
                {
                    HandleContactMessage(message, out response);
                }
                else if (wantedUrl == "/history/")
                {
                    HandleMessageHistory(message, out response);
                }
                else
                {
                    response = string.Empty;
                }

                SendResponse(messageToHandle, response);
            }
            catch { }

            CloseResponseStream(messageToHandle);
        }

        void ConnectionCheck(ControlMessage message, string sender, out string response)
        {
            response = string.Empty;
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);
            if (user != null)
            {
                user.UpdateLastConnectionCheck(DateTime.Now);
                user.UpdateAddress(sender);
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

        void DiffieTunnel(ControlMessage message, out string response)
        {
            response = string.Empty;
            User user = null;

            if (message.MessageSender == "UNKNOWN")
            {
                string newGuid = "TMPUSER_" + Guid.NewGuid();
                user = new User(newGuid);
                UserControll.Instance.AddTemporaryUserToApplication(user);
            }
            else
            {
                user = UserControll.Instance.GetUserFromApplication(message.MessageSender);
            }

            if(message.MessageType == "REQUEST_FOR_ID")
            {
                Console.WriteLine(message.MessageSender + " request for id.");
                response = user.Name;
            }
            else if(message.MessageType == "PUBLIC_KEY_EXCHANGE")
            {
                Console.WriteLine(message.MessageSender + " exchange pkey.");
                user.Tunnel.CreateKey(message.MessageContent);
                response = user.Tunnel.GetPublicPart();  
            }
            else if(message.MessageType == "IV_EXCHANGE")
            {
                Console.WriteLine(message.MessageSender + " exchange iv.");
                user.Tunnel.LoadIV(message.MessageContent);
                response = "CHECK";
            }
            else if(message.MessageType == "CHECK_TUNNEL")
            {
                Console.WriteLine(message.MessageSender + " checking tunnel.");
                if(user.Tunnel.DiffieDecrypt(message.MessageContent) == "OK")
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

        void Register(ControlMessage message, out string response)
        {
            response = string.Empty;
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);

            if (user != null)
            {
                if(message.MessageType == "REGISTER_ME")
                {
                    Console.WriteLine(message.MessageSender + " is about to register.");
                    UserPasswordData userPasswordData = new UserPasswordData();
                    userPasswordData.LoadJson(user.Tunnel.DiffieDecrypt(message.MessageContent));
                    if (UserControll.Instance.CheckIsUserExist(userPasswordData.Username))
                    {
                        response = "ECHO: Fuck off, taki user już istnieje";
                    }
                    else
                    {
                        UserControll.Instance.AddUserToDatabase(userPasswordData);
                        response = "ECHO: " + userPasswordData.Username + "," + userPasswordData.HashedPassword;
                    }
                }
            }
        }

        void LogIn(ControlMessage message, out string response)
        {
            response = string.Empty;
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);

            if (user != null)
            {
                Console.WriteLine(message.MessageSender + " is trying to log in.");
                UserPasswordData userPasswordData = new UserPasswordData();
                userPasswordData.LoadJson(user.Tunnel.DiffieDecrypt(message.MessageContent));
                if (!UserControll.Instance.CheckIsUserExist(userPasswordData.Username))
                {
                    response = "ECHO: taki user nie istnieje";
                }
                else
                {
                    UserControll.Instance.AddUserToApplication(message.MessageSender, userPasswordData.Username);
                    response = "ECHO: " + userPasswordData.Username + "," + userPasswordData.HashedPassword;
                }
            }
        }

        private ControlMessage ParseMessageContent(string messageContent)
        {
            ControlMessage message = new ControlMessage();
            message.LoadJson(messageContent);
            return message;
        }

        private void SendChatMessage(ControlMessage message, out string response)
        {
            response = string.Empty;
            //TODO: We should apply some kind of decryption here
            Message chatMessage = new Message();
            chatMessage.LoadJson(message.MessageContent);
            //            We should now send message somehow
            MessageControl.Instance.InsertMessage(chatMessage);
        }

        private void HandleContactMessage(ControlMessage message, out string response)
        {
            //TODO: We should decrypt value somehow
            Contact contact = new Contact();
            contact.LoadJson(message.MessageContent);
            response = string.Empty;
            if (message.MessageType == "CONTACT_INSERT")
            {
                ContactControl.Instance.InsertContact(contact);
            }
            else if (message.MessageType == "CONTACT_UPDATE")
            {
                ContactControl.Instance.UpdateContact(contact);
            }
            else if (message.MessageType == "CONTACT_GET")
            {
                List<Contact> contacts = ContactControl.Instance.GetContacts(contact);
                StringBuilder builder = new StringBuilder();
                contacts.ForEach( c => builder.Append( c.From + "|" + c.To + "|" + c.DisplayName + "\n"));
                response = builder.ToString();
//                What now??
            }
            else
            {
                throw new NotImplementedException("Message type not yet supported");
            }
        }

        void HandleMessageHistory(ControlMessage message, out string response)
        {
            Contact contact = new Contact();
            contact.LoadJson(message.MessageContent);
            List<Message> messagesHistory = MessageControl.Instance.GetMessages(contact);
            // TODO:  We should return this value somehow
            StringBuilder builder = new StringBuilder();
            messagesHistory.ForEach(
                messageHistory =>
                    builder.Append(messageHistory.MessageSender + "|" + messageHistory.MessageDestination + "|" +
                                   messageHistory.MessageContent + "|" + messageHistory.MessageDate + "\n"));
            response = builder.ToString();
        }
            

        void SendResponse(HttpListenerContext context, string response)
        {
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch { }
        }

        void CloseResponseStream(HttpListenerContext context)
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch { }
        }

    }
}