using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using OpenSSL.Crypto;

namespace CommunicatorCore.Classes.Service
{
    public class ControlMessageParser
    {
        public static BatchControlMessage CreateInvalidBatchResponseMessage(CryptoRSA crypter, SymmetricCipher cipher, string messageSender)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            string aesKey = Guid.NewGuid().ToString();
            string encryptedKey = crypter.PublicEncrypt(aesKey);
            ControlMessage controlMessage = new ControlMessage(messageSender, "INVALID", invalidMessage, cipher.Encode(invalidMessage, aesKey, string.Empty));
            BatchControlMessage invalidBatchControlMessage = new BatchControlMessage(controlMessage, encryptedKey);
            return invalidBatchControlMessage;
        }

        public static BatchControlMessage CreateResponseBatchMessage(CryptoRSA crypter, SymmetricCipher cipher,
            string messageSender, string messageType, INetworkMessage message)
        {
            return CreateResponseBatchMessage(crypter, cipher, messageSender, messageType, message.GetJsonString());
        }

        public static BatchControlMessage CreateResponseBatchMessage(CryptoRSA crypter, SymmetricCipher cipher,
            string messageSender, string messageType, string plainMessage)
        {
            string aesKey = Guid.NewGuid().ToString();
            string encryptedAesKey = crypter.PublicEncrypt(aesKey);
            string encryptedMessage = cipher.Encode(plainMessage, aesKey, string.Empty);
            ControlMessage controlMessage = new ControlMessage(messageSender, messageType, plainMessage, encryptedMessage);
            BatchControlMessage batchControlMessage = new BatchControlMessage(controlMessage, encryptedAesKey);
            return batchControlMessage;
        }

        public static ControlMessage CreateInvalidResponseMessage(CryptoRSA transcoder,string messageSender)
        {
            const string invalidMessage = "INVALID_CHECKSUM";
            return new ControlMessage(messageSender, "INVALID", invalidMessage, transcoder.PublicEncrypt(invalidMessage));
        }

        public static ControlMessage CreateResponseControlMessage(CryptoRSA crypter, string messageSender,
            string messageType, INetworkMessage message)
        {
            return CreateResponseControlMessage(crypter, messageSender, messageType, message.GetJsonString());
        }
        public static ControlMessage CreateResponseControlMessage(CryptoRSA crypter, string messageSender,
            string messageType, string plainMessage)
        {
            string encryptedMessage = crypter.PublicEncrypt(plainMessage);
            ControlMessage controlMessage = new ControlMessage(messageSender, messageType, plainMessage, encryptedMessage);
            return controlMessage;
        }

        public static ControlMessage CreateResponseControlMessage(DiffieHellmanTunnel tunnel, string messageSender,
            string messageType, string plainMessage)
        {
            string encryptedMessage = tunnel.DiffieEncrypt(plainMessage);
            ControlMessage controlMessage = new ControlMessage(messageSender, messageType, plainMessage, encryptedMessage);
            return controlMessage;
        }
        

    }
}
