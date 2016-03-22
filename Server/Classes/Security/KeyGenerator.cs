using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSSL.Crypto;
using System.IO;

namespace Server
{
    public static class KeyGenerator
    {
        public static void GenerateKeyPair(string userName)
        {
            RSA rsa = new RSA();
            rsa.GenerateKeys(4096, 65537, null, null);

            string[] keys = { string.Empty, string.Empty };

            File.WriteAllText("keys/" + userName + "_Private.pem", rsa.PrivateKeyAsPEM);
            File.WriteAllText("keys/" + userName + "_Public.pem", rsa.PublicKeyAsPEM);           
        }
    }
}
