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
        private RSA _publicRSA;
        public RSA PublicRSA
        {
            get
            {
                return _publicRSA;
            }
        }

        private RSA _privateRSA;
        public RSA PrivateRSA
        {
            get
            {
                return _privateRSA;
            }
        }

        public void LoadRsaFromPublicKey(string pathToPublicKey)
        {
            string publicKey;
            using (StreamReader sr = new StreamReader(pathToPublicKey))
            {
                publicKey = sr.ReadToEnd();
            }
            using (var key = CryptoKey.FromPublicKey(publicKey, string.Empty))
            {
                _publicRSA = key.GetRSA();
            }
        }

        public void LoadRsaFromPrivateKey(string pathToPrivateKey)
        {
            string privateKey;
            using (StreamReader sr = new StreamReader(pathToPrivateKey))
            {
                privateKey = sr.ReadToEnd();
            }
            using (var key = CryptoKey.FromPrivateKey(privateKey, string.Empty))
            {
                _privateRSA = key.GetRSA();
            }
        }

        public string PublicEncrypt(string textToEncrypt)
        {
            return PublicEncrypt(textToEncrypt, _publicRSA);
        }

        public string PublicEncrypt(string textToEncrypt, RSA rsa)
        {
            try
            {
                byte[] plain = Encoding.UTF8.GetBytes(textToEncrypt);
                byte[] output = rsa.PublicEncrypt(plain, RSA.Padding.PKCS1);

                return Convert.ToBase64String(output);
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public string PrivateDecrypt(string textToDecrypt)
        {
            return PrivateDecrypt(textToDecrypt, _privateRSA);
        }

        public string PrivateDecrypt(string textToDecrypt, RSA rsa)
        {
            try
            {
                byte[] cipher = Convert.FromBase64String(textToDecrypt);
                byte[] output = rsa.PrivateDecrypt(cipher, RSA.Padding.PKCS1);

                return Encoding.UTF8.GetString(output);
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
