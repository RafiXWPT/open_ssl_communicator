using OpenSSL.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Model
{
    public class CryptoRSA
    {
        private RSA publicRSA;
        public RSA PublicRSA
        {
            get
            {
                return publicRSA;
            }
        }

        private RSA privateRSA;
        public RSA PrivateRSA
        {
            get
            {
                return privateRSA;
            }
        }

        public CryptoRSA()
        {
        }

        public void loadRSAFromPublicKey(string pathToPublicKey)
        {
            string publicKey = string.Empty;
            using (StreamReader sr = new StreamReader(pathToPublicKey))
            {
                publicKey = sr.ReadToEnd();
            }
            using (var key = CryptoKey.FromPublicKey(publicKey, string.Empty))
            {
                publicRSA = key.GetRSA();
            }
        }

        public void loadRSAFromPrivateKey(string pathToPrivateKey)
        {
            string privateKey = string.Empty;
            using (StreamReader sr = new StreamReader(pathToPrivateKey))
            {
                privateKey = sr.ReadToEnd();
            }
            using (var key = CryptoKey.FromPrivateKey(privateKey, string.Empty))
            {
                privateRSA = key.GetRSA();
            }
        }

        public string PublicEncrypt(string textToEncrypt, RSA rsa)
        {
            try
            {
                byte[] plain = Encoding.UTF8.GetBytes(textToEncrypt);
                byte[] output;


                output = rsa.PublicEncrypt(plain, RSA.Padding.PKCS1);
                return Convert.ToBase64String(output);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string PrivateDecrypt(string textToDecrypt, RSA rsa)
        {
            try
            {
                byte[] cipher = Convert.FromBase64String(textToDecrypt);
                byte[] output;

                output = rsa.PrivateDecrypt(cipher, RSA.Padding.PKCS1);

                return Encoding.UTF8.GetString(output);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
