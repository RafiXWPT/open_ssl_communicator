using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SystemSecurity;
using CommunicatorCore.Classes.Model;
using MongoDB.Driver;
using Server.Classes;
using System.Threading;

namespace Server
{
    public class RequestHandler
    {
        private const string MESSAGE_SENDER = "SERVER";
        private const string SERVER_PRIVATE_KEY = "keys/SERVER_Private.pem";

        public void HandleRequest(HttpListenerContext userToHandle)
        {
            string wantedUrl = userToHandle.Request.RawUrl;
            string sender = userToHandle.Request.RemoteEndPoint.Address.ToString();
            string messageContent = userToHandle.Request.Headers["messageContent"];
            try
            {
                if (wantedUrl == "/connectionCheck/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    ConnectionCheck(message, sender, userToHandle);
                }
                else if (wantedUrl == "/diffieTunnel/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    DiffieTunnel(message, userToHandle);
                }
                else if (wantedUrl == "/register/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    Register(message, userToHandle);
                }
                else if (wantedUrl == "/logIn/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    LogIn(message, userToHandle);
                }
                else if (wantedUrl == "/sendChatMessage/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    SendChatMessage(message, userToHandle);
                }
                else if (wantedUrl == "/contacts/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    HandleContactMessage(message, userToHandle);
                }
                else if (wantedUrl == "/history/")
                {
                    ControlMessage message = ParseMessageContent(messageContent);
                    HandleMessageHistory(message, userToHandle);
                }              
            }
            catch (Exception ex)
            {
                ServerLogger.LogMessage(ex.ToString());
            }
        }

        void ConnectionCheck(ControlMessage message, string sender, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
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

            SendResponse(userToHandle, response);
        }

        void DiffieTunnel(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
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

            SendResponse(userToHandle, response);
        }

        void Register(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
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
                        responseContent = user.Tunnel.DiffieEncrypt("REGISTER_INVALID");

                        controlMessage = new ControlMessage(MESSAGE_SENDER, "REGISTER_INFO", responseContent);
                        response = controlMessage.GetJsonString();
                        SendResponse(userToHandle, response);
                    }
                    else
                    {
                        KeyGenerator.GenerateKeyPair(userPasswordData.Username);
                        EmailMessage emailMessage = new EmailMessage("Crypto Talk Registration", userPasswordData.Username);
                        UserControll.Instance.AddUserToDatabase(userPasswordData);

                        responseContent = user.Tunnel.DiffieEncrypt("REGISTER_OK");
                        controlMessage = new ControlMessage(MESSAGE_SENDER, "REGISTER_INFO", responseContent);
                        response = controlMessage.GetJsonString();
                        SendResponse(userToHandle, response);

                        emailMessage.Send(true);
                        ServerLogger.LogMessage("User added to database, registration succesfull");
                    }
                }
            }
        }

        void LogIn(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
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
                SendResponse(userToHandle, response);
            }
        }

        private ControlMessage ParseMessageContent(string messageContent)
        {
            ControlMessage message = new ControlMessage();
            message.LoadJson(messageContent);
            return message;
        }

        private void SendChatMessage(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
            string messageContent = string.Empty;
            Message chatMessage = new Message();
            CryptoRSA transcoder = new CryptoRSA();
            transcoder.loadRSAFromPrivateKey(SERVER_PRIVATE_KEY);

            if (message.MessageType == "CHAT_INIT")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " initializing asymetric tunnel.");
                messageContent = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
                chatMessage.LoadJson(messageContent);
                if(chatMessage.MessageContent == "INIT")
                {
                    response = "OK";
                    ServerLogger.LogMessage("Tunnel for: " + message.MessageSender + " has been initialized.");
                }
                else
                {
                    response = "BAD";
                    ServerLogger.LogMessage("Tunnel for: " + message.MessageSender + " has not been initialized.");
                }
            }
            else if (message.MessageType == "CHAT_MESSAGE")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " sends chat message.");
                messageContent = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
                chatMessage.LoadJson(messageContent);
                
                // Check to who and send that message
                // First we have to create method to detect incomming messages on clients!
            }

            SendResponse(userToHandle, response);
        }

        private void HandleContactMessage(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
            ControlMessage returnMessage = new ControlMessage();
            CryptoRSA transcoder = new CryptoRSA();
            transcoder.loadRSAFromPrivateKey("keys/SERVER_Private.pem");
            transcoder.loadRSAFromPublicKey("keys/" + message.MessageSender + "_Public.pem");

            string decryptedMessageContent = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
            if (Sha1Util.CalculateSha(decryptedMessageContent) != message.Checksum)
            {
                returnMessage = CreateInvalidResponseMessage(transcoder, transcoder.PublicRSA);
            }
            else if (message.MessageType == "CONTACT_INSERT" || message.MessageType == "CONTACT_UPDATE")
            {
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessageContent);
                if (!UserControll.Instance.CheckIfUserExist(contact.To))
                {
                    string userDoesNotExistResponse = "User does not exist";
                    string encryptedMessage = transcoder.PublicEncrypt(userDoesNotExistResponse, transcoder.PublicRSA);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_ERROR", userDoesNotExistResponse, encryptedMessage);
                }
                else
                {
                    string userInsertSuccessfullyMessage = "Successfully added user to contacts";
                    ContactControl.Instance.UpsertContact(contact);
                    string encryptedMessage = transcoder.PublicEncrypt(userInsertSuccessfullyMessage, transcoder.PublicRSA);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_SUCCESS", userInsertSuccessfullyMessage, encryptedMessage);
                }
            }
            else if (message.MessageType == "CONTACT_GET")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " is trying to get his contacts");
                List<Contact> contacts = ContactControl.Instance.GetContacts(message.MessageSender);
                ContactAggregator aggregator = new ContactAggregator(contacts);
                string contactsJson = aggregator.GetJsonString();
                string encryptedContacts = transcoder.PublicEncrypt(contactsJson, transcoder.PublicRSA);
                returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_GET_OK", contactsJson, encryptedContacts);
            }
            else
            {
                throw new NotImplementedException("Message type not yet supported");
            }

            response = returnMessage.GetJsonString();
            SendResponse(userToHandle, response);
        }

        private ControlMessage CreateInvalidResponseMessage(CryptoRSA transcoder, OpenSSL.Crypto.RSA RSA)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            return new ControlMessage(MESSAGE_SENDER, "INVALID", invalidMessage, transcoder.PublicEncrypt(invalidMessage, RSA));
        }

        void HandleMessageHistory(ControlMessage message, HttpListenerContext userToHandle)
        {
			string response = String.Empty;
            ControlMessage responseMessage;
            CryptoRSA transcoder = new CryptoRSA();

            transcoder.loadRSAFromPrivateKey(SERVER_PRIVATE_KEY);
            transcoder.loadRSAFromPublicKey("keys/" + message.MessageSender + "_Public.pem");

            string decryptedMessage = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
            if (decryptedMessage != message.Checksum)
            {
                responseMessage = CreateInvalidResponseMessage(transcoder, transcoder.PrivateRSA);
            }
            else { 
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessage);
                List<Message> messagesHistory = MessageControl.Instance.GetMessages(contact);
                MessageHistoryAggregator historyAggregator = new MessageHistoryAggregator(messagesHistory);
                string plainMessageContent = historyAggregator.GetJsonString();
                string encryptedMessageContent = transcoder.PublicEncrypt(plainMessageContent, transcoder.PublicRSA);
                responseMessage = new ControlMessage(MESSAGE_SENDER, "HISTORY_OK", plainMessageContent, encryptedMessageContent);
            }
            response = responseMessage.GetJsonString();
            SendResponse(userToHandle, response);
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

            CloseResponseStream(context);
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