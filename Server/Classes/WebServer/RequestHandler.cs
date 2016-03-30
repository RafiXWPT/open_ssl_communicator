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
using System.Collections.Specialized;
using CommunicatorCore.Classes.Exceptions;

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
                if(wantedUrl ==  "/status/" )
                {
                    BatchControlMessage batchControlMessage = ParseBatchMessageContent(messageContent);
                    HandleContactStatusMessage(batchControlMessage, userToHandle);
                }
                else { 
                    ControlMessage message = ParseMessageContent(messageContent);
                    switch (wantedUrl)
                    {
                        case "/connectionCheck/":
                            ConnectionCheck(message, sender, userToHandle);
                            break;
                        case "/diffieTunnel/":
                            DiffieTunnel(message, userToHandle);
                            break;
                        case "/register/":
                            Register(message, userToHandle);
                            break;
                        case "/logIn/":
                            LogIn(message, userToHandle);
                            break;
                        case "/sendChatMessage/":
                            SendChatMessage(message, userToHandle);
                            break;
                        case "/contacts/":
                            HandleContactMessage(message, userToHandle);
                            break;
                        case "/history/":
                            HandleMessageHistory(message, userToHandle);
                            break;
                        case "/password/":
                            HandlePasswordMessage(message, userToHandle);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ServerLogger.LogMessage(ex.ToString());
                HandleException(ex, userToHandle);
            }
        }

        private void HandleContactStatusMessage(BatchControlMessage batchControlMessage, HttpListenerContext userToHandle)
        {
            BatchControlMessage returnMessage;
            ControlMessage message = batchControlMessage.ControlMessage;
            string returnMessageContent = string.Empty;
            CryptoRSA crypter = GetUserServerCrypter(message.MessageSender);
            string decryptedMessageKey = crypter.PrivateDecrypt(batchControlMessage.CipheredKey);
            SymmetricCipher cipher = new SymmetricCipher();
            string decryptedMessageContent = cipher.Decode(message.MessageContent, decryptedMessageKey,
                string.Empty);
            ServerLogger.LogMessage("Received request on contact status from: " + message.MessageSender);
            if (message.Checksum != Sha1Util.CalculateSha(decryptedMessageContent))
            {
                ServerLogger.LogMessage(decryptedMessageContent);
                returnMessage = CreateInvalidBatchResponseMessage(crypter, crypter.PublicRSA);
            }
            else if (message.MessageType == "CONTACT_STATUS")
            {
                UserConnectionStatusAggregator aggregator = new UserConnectionStatusAggregator();
                aggregator.LoadJson(decryptedMessageContent);

                foreach (var userConnectionStatus in aggregator.ConnectionStatus)
                {
                    userConnectionStatus.ConnectionStatus =
                        UserControll.Instance.GetUserStatus(userConnectionStatus.Username).ToString();
                }
                string messageContent = aggregator.GetJsonString();
                string cipheredReturnMessageContent = cipher.Encode(messageContent, decryptedMessageKey, string.Empty);
                ControlMessage returnControlMessage = new ControlMessage(MESSAGE_SENDER, "CONTACT_STATUS_OK", messageContent, cipheredReturnMessageContent);
                string cipheredKey = crypter.PublicEncrypt(decryptedMessageKey);
                returnMessage = new BatchControlMessage(returnControlMessage, cipheredKey);
            }
            else
            {
                throw new UnsupportedOperationException("Operation type: " + message.MessageType +
                                                        " is not yet supported");
            }
            SendResponse(userToHandle, returnMessage.GetJsonString());
        }

        private BatchControlMessage CreateInvalidBatchResponseMessage(CryptoRSA crypter, RSA publicRsa)
        {
            BatchControlMessage invalidBatchControlMessage = new BatchControlMessage();
            invalidBatchControlMessage.ControlMessage = CreateInvalidResponseMessage(crypter, publicRsa);
            invalidBatchControlMessage.CipheredKey = string.Empty;
            return invalidBatchControlMessage;
        }

        private BatchControlMessage ParseBatchMessageContent(string messageContent)
        {
            BatchControlMessage batchControlMessage = new BatchControlMessage();
            batchControlMessage.LoadJson(messageContent);
            return batchControlMessage;
        }

        private void HandlePasswordMessage(ControlMessage message, HttpListenerContext userToHandle)
        {
            ControlMessage returnMessage;
            string returnMessageContent = string.Empty;
            CryptoRSA crypter = GetUserServerCrypter(message.MessageSender);
            string decryptedMessageContent = crypter.PrivateDecrypt(message.MessageContent);
            ServerLogger.LogMessage("Received request on password service from: " + message.MessageSender);
            if (message.Checksum != Sha1Util.CalculateSha(decryptedMessageContent))
            {
                ServerLogger.LogMessage(decryptedMessageContent);
                returnMessage = CreateInvalidResponseMessage(crypter, crypter.PublicRSA);
            }
            else if (message.MessageType == "CHANGE_PASSWORD")
            {
                ServerLogger.LogMessage(message.MessageSender + " is about to change his password.");
                ChangePasswordDTO userPasswordData = new ChangePasswordDTO();
                userPasswordData.LoadJson(decryptedMessageContent);
                string messageContent;
                if (
                    !UserControll.Instance.IsUserValid(new UserPasswordData
                    {
                        Username = userPasswordData.Username,
                        HashedPassword = userPasswordData.CurrentPassword
                    }))
                {
                    messageContent = "CHANGE_INVALID_CURRENT";
                }
                else
                {
                    UserPasswordData updatedUserPassword = new UserPasswordData();
                    updatedUserPassword.Username = userPasswordData.Username;
                    updatedUserPassword.HashedPassword = userPasswordData.NewPassword;

                    UserControll.Instance.UpdateUser(updatedUserPassword);
                    messageContent = "CHANGE_OK";
                }
                string encryptedMessageContent = crypter.PublicEncrypt(messageContent, crypter.PublicRSA);
                returnMessage = new ControlMessage(MESSAGE_SENDER, "CHANGE_PASSWORD", messageContent, encryptedMessageContent);
            }
            else if (message.MessageType == "TOKEN_VALIDATE")
            {
                ServerLogger.LogMessage("User : " + message.MessageSender + " is validating his token");
                UserTokenDto userTokenDto = new UserTokenDto();
                userTokenDto.LoadJson(decryptedMessageContent);
                string messageContent = UserControll.Instance.IsTokenValid(userTokenDto)
                    ? "TOKEN_VALIDATE_OK"
                    : "TOKEN_INVALID";
                string encryptedMessageContent = crypter.PublicEncrypt(messageContent);
                returnMessage = new ControlMessage(MESSAGE_SENDER, "TOKEN_VALIDATE", messageContent,
                    encryptedMessageContent);
            }
            else
            {
                throw new UnsupportedOperationException("Operation type: " + message.MessageType +
                                                        " is not yet supported");
            }
            SendResponse(userToHandle, returnMessage.GetJsonString());
        }

        private CryptoRSA GetUserServerCrypter(string messageSender)
        {
            CryptoRSA crypter = new CryptoRSA();
            crypter.LoadRsaFromPrivateKey(SERVER_PRIVATE_KEY);
            crypter.LoadRsaFromPublicKey("keys/" + messageSender + "_Public.pem");
            return crypter;
        }

        private void HandleException(Exception exception, HttpListenerContext userToHandle) 
        {
            ControlMessage replyMessage = new ControlMessage(MESSAGE_SENDER, "INVALID", exception.Message);
            string response = replyMessage.GetJsonString();
            SendResponse(userToHandle, response);
        }

        void ConnectionCheck(ControlMessage message, string sender, HttpListenerContext userToHandle)
        {
            string response;
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);

            if (user != null)
            {
                user.UpdateLastConnectionCheck(DateTime.Now);
                user.UpdateAddress(sender);
            }

            if (message.MessageType == "CHECK_CONNECTION" && message.MessageContent == "CONN_CHECK")
            {
                response = "CONN_AVAIL";
                if(user != null)
                    user.UpdateStatus(UserStatus.Online);
            }
            else if(message.MessageType == "CHECK_CONNECTION" && message.MessageContent == "IDLE_CHECK")
            {
                response = "CONN_AVAIL";
                if(user != null)
                    user.UpdateStatus(UserStatus.AFK);
            }
            else
            {
                response = "BAD_CONTENT";
            }

            SendResponse(userToHandle, response);
        }

        void DiffieTunnel(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response;
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
                        string userToken = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                        UserControll.Instance.AddUserToDatabase(userPasswordData, Sha1Util.CalculateSha(userToken));
                        string emailContent = "Your unique token for contacts and history access: " + userToken +
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
                SendResponse(userToHandle, response);
            }
        }

        void LogIn(ControlMessage message, HttpListenerContext userToHandle)
        {
            User user = UserControll.Instance.GetUserFromApplication(message.MessageSender);

            if (user != null)
            {
                ServerLogger.LogMessage(message.MessageSender + " is trying to log in.");
                UserPasswordData userPasswordData = new UserPasswordData();
                string responseContent;
                string responseType = "LOGIN_UNSUCCESFULL";

                string decryptedMessage = user.Tunnel.DiffieDecrypt(message.MessageContent);

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
                string response = replyMessage.GetJsonString();
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
            Console.WriteLine("sender: " + userToHandle.Request.RemoteEndPoint);
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

                string target = chatMessage.MessageDestination;

                if(chatMessage.MessageContent == "INIT")
                {
                    User user = UserControll.Instance.GetUserFromApplication(target);
                    if(user == null)
                    {
                        response = "OFFLINE";
                        ServerLogger.LogMessage("Tunnel for: " + message.MessageSender + " has not been initialized.");
                    }
                    else if (user.Status == UserStatus.Online)
                    {
                        response = "OK";
                        ServerLogger.LogMessage("Tunnel for: " + message.MessageSender + " has been initialized.");
                    }
                }
                else
                {
                    response = "BAD";
                    ServerLogger.LogMessage("Tunnel for: " + message.MessageSender + " has not been initialized.");
                }

                SendResponse(userToHandle, response);
            }
            else if (message.MessageType == "CHAT_MESSAGE")
            {
                ServerLogger.LogMessage("User: " + message.MessageSender + " sends chat message.");

                messageContent = transcoder.PrivateDecrypt(message.MessageContent, transcoder.PrivateRSA);
                chatMessage.LoadJson(messageContent);

                string destination = chatMessage.MessageDestination;
                User user = UserControll.Instance.GetUserFromApplication(destination);

                if(user == null)
                {
                    response = "OFFLINE";
                    SendResponse(userToHandle, response);
                }
                else if (user.Status == UserStatus.Online)
                {
                    response = "RECEIVED";
                    SendResponse(userToHandle, response);

                    bool status = TunnelChatMessage(chatMessage, user.GetAddress());
                    if(status)
                    {
                        Message delvmsg = new Message(chatMessage.MessageUID, destination, message.MessageSender, "DELIVERED");
                        TunnelChatMessage(delvmsg, UserControll.Instance.GetUserFromApplication(message.MessageSender).GetAddress());
                    }

                }
            }
        }

        bool TunnelChatMessage(Message message, string destination)
        {
            Uri uri = new Uri("http://" + destination + ":11070/chatMessage/");
            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.LoadRsaFromPublicKey("keys/" + message.MessageDestination+"_Public.pem");
            string encryptedChatMessage = cryptoService.PublicEncrypt(message.GetJsonString(), cryptoService.PublicRSA);
            ControlMessage msg = new ControlMessage("SERVER", "CHAT_MESSAGE", encryptedChatMessage);

            string responseString = string.Empty;
            NameValueCollection headers = new NameValueCollection();
            NameValueCollection data = new NameValueCollection();

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    headers["messageContent"] = msg.GetJsonString();
                    client.Headers.Add(headers);
                    data["DateTime"] = DateTime.Now.ToShortDateString();
                    byte[] responseByte = client.UploadValues(uri, "POST", data);
                    responseString = Encoding.UTF8.GetString(responseByte);
                }

                return responseString == "RECEIVED";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private void HandleContactMessage(ControlMessage message, HttpListenerContext userToHandle)
        {
            string response = string.Empty;
            ControlMessage returnMessage;
            CryptoRSA transcoder = GetUserServerCrypter(message.MessageSender);

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

        private ControlMessage CreateInvalidResponseMessage(CryptoRSA transcoder, RSA publicRsa)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            return new ControlMessage(MESSAGE_SENDER, "INVALID", invalidMessage, transcoder.PublicEncrypt(invalidMessage, publicRsa));
        }

        void HandleMessageHistory(ControlMessage message, HttpListenerContext userToHandle)
        {
			string response = String.Empty;
            ControlMessage responseMessage;
            CryptoRSA transcoder = GetUserServerCrypter(message.MessageSender);

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