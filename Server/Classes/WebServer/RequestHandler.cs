using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SystemSecurity;
﻿using CommunicatorCore.Classes.Model;
using MongoDB.Driver;
using Server.Classes;

namespace Server
{
    public class RequestHandler
    {
        private const string MESSAGE_SENDER = "SERVER";
        private const string SERVER_PRIVATE_KEY = "keys\\SERVER_Private.pem";

        public void HandleRequest(HttpListenerContext userToHandle)
        {
            string wantedUrl = userToHandle.Request.RawUrl;
            string sender = userToHandle.Request.RemoteEndPoint.Address.ToString();
            string response = string.Empty;
            string messageContent = userToHandle.Request.Headers["messageContent"];
            try
            {
                if (wantedUrl == "/connectionCheck/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    ConnectionCheck(message, sender, out response);
                }
                else if (wantedUrl == "/diffieTunnel/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    DiffieTunnel(message, out response);
                }
                else if (wantedUrl == "/register/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    Register(message, out response);
                }
                else if (wantedUrl == "/logIn/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    LogIn(message, out response);
                }
                else if (wantedUrl == "/sendChatMessage/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    SendChatMessage(message, out response);
                }
                else if (wantedUrl == "/contacts/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    HandleContactMessage(message, out response);
                }
                else if (wantedUrl == "/history/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    HandleMessageHistory(message, out response);
                }
                else
                {
                    response = string.Empty;
                }
                
            }
            catch (Exception ex)
            {
                ServerLogger.LogMessage(ex.ToString());
                response = ex.Message;
            }
            finally
            {
                SendResponse(userToHandle, response);
            }

            CloseResponseStream(userToHandle);
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
                string newGuid = "TMP_" + Guid.NewGuid();
                user = new User(newGuid);
                UserControll.Instance.AddTemporaryUserToApplication(user);
            }
            else
            {
                user = UserControll.Instance.GetUserFromApplication(message.MessageSender);
            }

            if(message.MessageType == "REQUEST_FOR_ID")
            {
                ServerLogger.LogMessage(message.MessageSender + " request for id.");
                response = user.Name;
            }
            else if(message.MessageType == "PUBLIC_KEY_EXCHANGE")
            {
                ServerLogger.LogMessage(message.MessageSender + " exchange pkey.");
                user.Tunnel.CreateKey(message.MessageContent);
                response = user.Tunnel.GetPublicPart();  
            }
            else if(message.MessageType == "IV_EXCHANGE")
            {
                ServerLogger.LogMessage(message.MessageSender + " exchange iv.");
                user.Tunnel.LoadIV(message.MessageContent);
                response = "CHECK";
            }
            else if(message.MessageType == "CHECK_TUNNEL")
            {
                ServerLogger.LogMessage(message.MessageSender + " checking tunnel.");
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
                if (message.MessageType == "REGISTER_ME")
                {
                    ServerLogger.LogMessage(message.MessageSender + " is about to register.");
                    UserPasswordData userPasswordData = new UserPasswordData();
                    userPasswordData.LoadJson(user.Tunnel.DiffieDecrypt(message.MessageContent));
                    ControlMessage controlMessage;

                    string responseContent;
                    if (UserControll.Instance.CheckIfUserExist(userPasswordData.Username))
                    {
                        ServerLogger.LogMessage("Requested user already exist.");
                        responseContent = "REGISTER_INVALID";
                    }
                    else
                    {
                        KeyGenerator.GenerateKeyPair(userPasswordData.Username);
                        EmailMessage emailMessage = new EmailMessage("OpenSSL Registration", userPasswordData.Username);
                        UserControll.Instance.AddUserToDatabase(userPasswordData);
                        responseContent = "REGISTER_OK";
                        emailMessage.Send(true);
                        ServerLogger.LogMessage("User added to database, registration succesfull");
                    }
                    string encryptedContent = user.Tunnel.DiffieEncrypt(responseContent);
                    controlMessage = new ControlMessage(MESSAGE_SENDER, "REGISTER_INFO", responseContent, encryptedContent);
                    response = controlMessage.GetJsonString();
                }
            }
        }

        void LogIn(ControlMessage message, out string response)
        {
            response = string.Empty;
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);

            if (user != null)
            {
                ServerLogger.LogMessage(message.MessageSender + " is trying to log in.");
                UserPasswordData userPasswordData = new UserPasswordData();
                string responseContent;
                string responseType = "LOGIN_UNSUCCESFULL";

                string decryptedMessage = user.Tunnel.DiffieDecrypt(message.MessageContent);
                Console.WriteLine(decryptedMessage);
                if (Sha1Util.CalculateSha(decryptedMessage) != message.Checksum)
                {
                    responseContent = "INVALID";
                }
                else
                {
                    userPasswordData.LoadJson(decryptedMessage);
                    if (!UserControll.Instance.CheckIfUserExist(userPasswordData.Username))
                    {
                        ServerLogger.LogMessage("User does not exist");
                        responseContent = "USER_NOT_EXIST";
                    }
                    else if (!UserControll.Instance.IsUserValid(userPasswordData))
                    {
                        ServerLogger.LogMessage("badpass");
                        responseContent = "BAD_LOGIN_OR_PASSWORD";
                    }
                    else
                    {
                        responseContent = userPasswordData.Username;
                        responseType = "LOGIN_SUCCESFULL";
                        UserControll.Instance.AddUserToApplication(message.MessageSender, userPasswordData.Username);
                        ServerLogger.LogMessage("User: " + userPasswordData.Username +
                                                " has logged in! Total users logged: " +
                                                UserControll.Instance.GetUsersOnline());
                    }
                }
                string encryptedContent = user.Tunnel.DiffieEncrypt(responseContent);
                ControlMessage replyMessage = new ControlMessage(MESSAGE_SENDER, responseType, responseContent, encryptedContent);
                response = replyMessage.GetJsonString();
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
            if (message.MessageType == "CHAT_ECHO")
            {
                //Console.WriteLine(message.MessageSender + "," + message.MessageType + "," + message.MessageContent);

                CryptoRSA rsa = new CryptoRSA(null, "keys/SERVER_Private.pem");
                string messageContent = rsa.Decrypt(message.MessageContent);
                if (Sha1Util.CalculateSha(messageContent) != message.Checksum)
                {
                    //                    Ktoś napsioczyl
                    response = "INVALID";
                }
                else { 
                    Message chatMessage = new Message();
                    chatMessage.LoadJson(messageContent);
                    //            We should now send message somehow
                    //MessageControl.Instance.InsertMessage(chatMessage);
                    response = "ECHO: " + chatMessage.MessageContent;
                }
            }
        }

        private void HandleContactMessage(ControlMessage message, out string response)
        {
            ControlMessage returnMessage = new ControlMessage();
            CryptoRSA transcoder = new CryptoRSA("keys\\SERVER_Private.pem", "keys\\" + message.MessageSender + "_Public.pem");
            string decryptedMessageContent = transcoder.Decrypt(message.MessageContent);
            if (Sha1Util.CalculateSha(decryptedMessageContent) != message.Checksum)
            {
                returnMessage = CreateInvalidResponseMessage(transcoder);
            }
            else if (message.MessageType == "CONTACT_INSERT" || message.MessageType == "CONTACT_UPDATE")
            {
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessageContent);
                if (!UserControll.Instance.CheckIfUserExist(contact.To))
                {
                    string userDoesNotExistResponse = "User does not exist";
                    string encryptedMessage = transcoder.Encrypt(userDoesNotExistResponse);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_ERROR", userDoesNotExistResponse, encryptedMessage);
                }
                else
                {
                    string userInsertSuccessfullyMessage = "Successfully added user to contacts";
                    ContactControl.Instance.UpsertContact(contact);
                    string encryptedMessage = transcoder.Encrypt(userInsertSuccessfullyMessage);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_SUCCESS", userInsertSuccessfullyMessage, encryptedMessage);
                }
            }
            else if (message.MessageType == "CONTACT_GET")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " is trying to get his contacts");
                List<Contact> contacts = ContactControl.Instance.GetContacts(message.MessageSender);
                ContactAggregator aggregator = new ContactAggregator(contacts);
                string contactsJson = aggregator.GetJsonString();
                string encryptedContacts = transcoder.Encrypt(contactsJson);
                returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_GET_OK", contactsJson, encryptedContacts);
            }
            else
            {
                throw new NotImplementedException("Message type not yet supported");
            }
            response = returnMessage.GetJsonString();
        }

        private ControlMessage CreateInvalidResponseMessage(CryptoRSA transcoder)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            return new ControlMessage(MESSAGE_SENDER, "INVALID", invalidMessage, transcoder.Encrypt(invalidMessage));
        }

        void HandleMessageHistory(ControlMessage message, out string response)
        {
            ControlMessage responseMessage;
            CryptoRSA transcoder = new CryptoRSA(SERVER_PRIVATE_KEY, "keys\\" + message.MessageSender + "_Public.pem");
            string decryptedMessage = transcoder.Decrypt(message.MessageContent);
            if (decryptedMessage != message.Checksum)
            {
                responseMessage = CreateInvalidResponseMessage(transcoder);
            }
            else { 
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessage);
                List<Message> messagesHistory = MessageControl.Instance.GetMessages(contact);
                MessageHistoryAggregator historyAggregator = new MessageHistoryAggregator(messagesHistory);
                string plainMessageContent = historyAggregator.GetJsonString();
                string encryptedMessageContent = transcoder.Encrypt(plainMessageContent);
                responseMessage = new ControlMessage(MESSAGE_SENDER, "HISTORY_OK", plainMessageContent, encryptedMessageContent);
            }
            response = responseMessage.GetJsonString();
        }
        
        

        void SendResponse(HttpListenerContext context, string response)
        {
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch(Exception ex)
            {
                ServerLogger.LogMessage(ex.ToString());
            }
        }

        void CloseResponseStream(HttpListenerContext context)
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch(Exception ex)
            {
                ServerLogger.LogMessage(ex.ToString());
            }
        }
    }
}