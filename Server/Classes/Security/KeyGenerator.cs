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

        public static string[] GenerateKeyPair()
        {
            RSA rsa = new RSA();
            rsa.GenerateKeys(1024, 65537, null, null);

            string[] keys = { string.Empty, string.Empty };

            //File.WriteAllText("MasterPrivateKey.pem", rsa.PrivateKeyAsPEM);
            //File.WriteAllText("MasterPublicKey.pem", rsa.PublicKeyAsPEM);

            keys[0] = rsa.PrivateKeyAsPEM;
            keys[1] = rsa.PublicKeyAsPEM;

            return keys;
           
        }

    }
}
