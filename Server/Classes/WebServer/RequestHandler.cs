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
using OpenSSL.Crypto;

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
                HandleException(ex, userToHandle);
            }
        }

        // Temporary fix!
        private void HandleException(Exception exception, HttpListenerContext userToHandle) 
        {
            ControlMessage replyMessage = new ControlMessage(MESSAGE_SENDER, "INVALID", exception.Message);
            string response = replyMessage.GetJsonString();
            SendResponse(userToHandle, response);
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
                ServerLogger.LogMessage(message.MessageSender + " is about to register.");
                UserPasswordData userPasswordData = new UserPasswordData();
                userPasswordData.LoadJson(user.Tunnel.DiffieDecrypt(message.MessageContent));
                ControlMessage controlMessage;

                string responseContent;

                if (message.MessageType == "REGISTER_ME")
                {
                    if (UserControll.Instance.CheckIfUserExist(userPasswordData.Username))
                    {
                        ServerLogger.LogMessage("Requested user already exist.");
                        responseContent = user.Tunnel.DiffieEncrypt("REGISTER_INVALID");

                        controlMessage = new ControlMessage(MESSAGE_SENDER, "REGISTER_INFO", responseContent);
                        response = controlMessage.GetJsonString();
                   }
                    else
                    {
                        KeyGenerator.GenerateKeyPair(userPasswordData.Username);
                        UserControll.Instance.AddUserToDatabase(userPasswordData);
                        string userToken = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                        string emailContent = "Your unique token for contacts & history access: " + userToken +
                                              ". DO NOT LOOSE IT!!!";
                        EmailMessage emailMessage = new EmailMessage("Crypto Talk Registration", emailContent, userPasswordData.Username);
                        emailMessage.Send(true);

                        responseContent = user.Tunnel.DiffieEncrypt("REGISTER_OK");
                        controlMessage = new ControlMessage(MESSAGE_SENDER, "REGISTER_INFO", responseContent);
                        response = controlMessage.GetJsonString();

                        ServerLogger.LogMessage("User added to database, registration succesfull");
                    }
                }
                else if (message.MessageType == "RESET_PASSWORD")
                {
                    if (!UserControll.Instance.CheckIfUserExist(userPasswordData.Username))
                    {
                        ServerLogger.LogMessage(message.MessageSender + " is trying to reset password of user: " +
                                                userPasswordData.Username + " which does not exist");
                        responseContent = user.Tunnel.DiffieEncrypt("RESET_INVALID");
                        controlMessage = new ControlMessage(MESSAGE_SENDER, "RESET_PASSWORD", responseContent);
                        response = controlMessage.GetJsonString();
                    }
                    else
                    {
                        ServerLogger.LogMessage(userPasswordData.Username + " is trying to reset his password.");
                        string generatedPassword =  Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                        UserPasswordData generatedUserPasswordData = new UserPasswordData(userPasswordData.Username, generatedPassword);
                        UserControll.Instance.UpdateUser(generatedUserPasswordData);
                        string emailContent = "Your new generated password is: " + generatedPassword;
                        EmailMessage emailMessage = new EmailMessage("Crypto Talk Password Reset", emailContent, userPasswordData.Username);
                        emailMessage.Send();
                        responseContent = user.Tunnel.DiffieEncrypt("RESET_OK");
                        controlMessage = new ControlMessage(MESSAGE_SENDER, "RESET_PASSWORD", responseContent);
                        response = controlMessage.GetJsonString();
                        ServerLogger.LogMessage(userPasswordData.Username + " password reset ended successfully.");
                    }
                }
                else
                {
                    // Empty statement
                }
                SendResponse(userToHandle, response);

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
            transcoder.LoadRsaFromPrivateKey(SERVER_PRIVATE_KEY);

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
                response = "RECEIVED";
                /* Okey, here we are, now we must reply to Client1 about getting message */
                // SendReponse(...)
                /* Next, we have to search user by Message.Destination 
                   When we have it, send msg on http://Client2Ip:11070/chatMessage/ with same UID from Client1
                   UID will be our identifier for all chat transactions
                   When Client2 receive MSG and reply to server with "RECEIVED" we should reply back "DELIVERED"
                   to: http://Client1Ip:11070/chatMessage/ with same UID of the message
                */
            }

            SendResponse(userToHandle, response);
        }

        private void HandleContactMessage(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
            ControlMessage returnMessage;
            CryptoRSA transcoder = new CryptoRSA();
            transcoder.LoadRsaFromPrivateKey("keys/SERVER_Private.pem");
            transcoder.LoadRsaFromPublicKey("keys/" + message.MessageSender + "_Public.pem");

            string decryptedMessageContent = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
            if (Sha1Util.CalculateSha(decryptedMessageContent) != message.Checksum)
            {
                returnMessage = CreateInvalidResponseMessage(transcoder, transcoder.PublicRSA);
                response = returnMessage.GetJsonString();
            }
            else if (message.MessageType == "CONTACT_INSERT" || message.MessageType == "CONTACT_UPDATE")
            {
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessageContent);
                if (!UserControll.Instance.CheckIfUserExist(contact.To))
                {
                    string userDoesNotExistResponse = "User does not exist";
                    string encryptedMessage = transcoder.PublicEncrypt(userDoesNotExistResponse, transcoder.PublicRSA);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_USER_NOT_EXIST", userDoesNotExistResponse, encryptedMessage);
                }
                else if (ContactControl.Instance.CheckIfContactExist(contact) && message.MessageType == "CONTACT_INSERT")
                {
                    string contactAlreadyExists = "Contact already exists";
                    string encryptedMessage = transcoder.PublicEncrypt(contactAlreadyExists, transcoder.PublicRSA);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_ALREADY_EXIST", contactAlreadyExists, encryptedMessage);
                }
                else
                {
                    string userInsertSuccessfullyMessage = "Successfully added user to contacts";
                    ContactControl.Instance.UpsertContact(contact);
                    string encryptedMessage = transcoder.PublicEncrypt(userInsertSuccessfullyMessage, transcoder.PublicRSA);
                    returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_INSERT_SUCCESS", userInsertSuccessfullyMessage, encryptedMessage);
                }
                response = returnMessage.GetJsonString();
            }
            else if (message.MessageType == "CONTACT_GET")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " is trying to get his contacts");
                List<Contact> contacts = ContactControl.Instance.GetContacts(message.MessageSender);
                ContactAggregator aggregator = new ContactAggregator(contacts);
                string contactsJson = aggregator.GetJsonString();

                SymmetricCipher cipher = new SymmetricCipher();
                string oneTimePass = Guid.NewGuid().ToString();
                string contactsEncryptedByAes = cipher.Encode(contactsJson, oneTimePass, string.Empty);
                string encryptedAesKey = transcoder.PublicEncrypt(oneTimePass, transcoder.PublicRSA);
                
                returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_GET_OK", contactsJson, contactsEncryptedByAes);
                BatchControlMessage batchControlMessage = new BatchControlMessage(returnMessage, encryptedAesKey);
                response = batchControlMessage.GetJsonString();
            }
            else if (message.MessageType == "CONTACT_DELETE")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " is trying to remove one of his contacts");
                Contact contact = new Contact();
                contact.LoadJson(decryptedMessageContent);
                ContactControl.Instance.DeleteContact(contact);

                string userDeletedSuccessfullyMessage = "Contact deleted successfully";
                string cipheredMessage = transcoder.PublicEncrypt(userDeletedSuccessfullyMessage, transcoder.PublicRSA);
                returnMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_REMOVE_OK", userDeletedSuccessfullyMessage, cipheredMessage);
                response = returnMessage.GetJsonString();
            }
            else
            {
                throw new NotImplementedException("Message type not yet supported");
            }

            SendResponse(userToHandle, response);
        }

        private ControlMessage CreateInvalidResponseMessage(CryptoRSA transcoder, RSA RSA)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            return new ControlMessage(MESSAGE_SENDER, "INVALID", invalidMessage, transcoder.PublicEncrypt(invalidMessage, RSA));
        }

        void HandleMessageHistory(ControlMessage message, HttpListenerContext userToHandle)
        {
			string response = String.Empty;
            ControlMessage responseMessage;
            CryptoRSA transcoder = new CryptoRSA();

            transcoder.LoadRsaFromPrivateKey(SERVER_PRIVATE_KEY);
            transcoder.LoadRsaFromPublicKey("keys/" + message.MessageSender + "_Public.pem");

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