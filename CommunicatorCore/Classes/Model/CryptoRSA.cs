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
        private readonly string pathToEncryptFile;
        private readonly string pathToDecryptFile;

        private readonly CryptoKey privateKey;
        private readonly CryptoKey publicKey;

        public CryptoRSA(string pathToEncryptFile, string pathToDecryptFile)
        {
            this.pathToEncryptFile = pathToEncryptFile;
            this.pathToDecryptFile = pathToDecryptFile;
            CreateKeys();
        }

        void CreateKeys()
        {

        }

        public string Encrypt(string textToEncrypt)
        {
            try
            {
                string publicKey = string.Empty;
                using (StreamReader sr = new StreamReader(pathToEncryptFile))
                {
                    publicKey = sr.ReadToEnd();
                }

                byte[] stringtoByte = Encoding.UTF8.GetBytes(textToEncrypt);
                byte[] output;

                using (var key = CryptoKey.FromPublicKey(publicKey, string.Empty))
                {
                    using (var rsa = key.GetRSA())
                    {

                        output = rsa.PublicEncrypt(stringtoByte, RSA.Padding.PKCS1);
                    }
                }

                return Convert.ToBase64String(output);
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }

        public string Decrypt(string textToDecrypt)
        {
            try
            {
                byte[] payLoad = Convert.FromBase64String(textToDecrypt);
                string privateKey = string.Empty;

                using (StreamReader sr = new StreamReader(pathToDecryptFile))
                {
                    privateKey = sr.ReadToEnd();
                }

                using (var key = CryptoKey.FromPrivateKey(privateKey, string.Empty))
                {
                    using (var rsa = key.GetRSA())
                    {
                        payLoad = rsa.PrivateDecrypt(payLoad, RSA.Padding.PKCS1);
                    }
                }

                return Encoding.UTF8.GetString(payLoad);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
