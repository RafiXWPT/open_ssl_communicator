using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSecurity;
using OpenSSL.Crypto;

namespace CommunicatorCore.Classes.Model
{
    public class SymmetricCipher
    {
        private readonly CipherContext cc;

        public SymmetricCipher()
        {
            cc = new CipherContext(Cipher.AES_256_ECB);
        }

        public SymmetricCipher(CipherType type, CipherMode mode, CipherSize size)
        {
            if (type == CipherType.AES)
            {
                if (size == CipherSize.SIZE_128)
                {
                    if (mode == CipherMode.CBC)
                        cc = new CipherContext(Cipher.AES_128_CBC);
                    else
                        cc = new CipherContext(Cipher.AES_128_ECB);
                }
                else if (size == CipherSize.SIZE_192)
                {
                    if (mode == CipherMode.CBC)
                        cc = new CipherContext(Cipher.AES_192_CBC);
                    else
                        cc = new CipherContext(Cipher.AES_192_ECB);
                }
                else if (size == CipherSize.SIZE_256)
                {
                    if (mode == CipherMode.CBC)
                        cc = new CipherContext(Cipher.AES_256_CBC);
                    else
                        cc = new CipherContext(Cipher.AES_256_ECB);
                }
            }
            else if (type == CipherType.BLOWFISH)
            {
                if (mode == CipherMode.CBC)
                    cc = new CipherContext(Cipher.Blowfish_CBC);
                else
                    cc = new CipherContext(Cipher.Blowfish_ECB);
            }
            else if (type == CipherType.CAST5)
            {
                if (mode == CipherMode.CBC)
                    cc = new CipherContext(Cipher.Cast5_CBC);
                else
                    cc = new CipherContext(Cipher.Cast5_ECB);
            }
        }

        public string Encode(string message, string key, string iv)
        {
            try
            {
                byte[] bytePlain = Encoding.UTF8.GetBytes(message);
                byte[] password = Encoding.UTF8.GetBytes(key);
                byte[] inicializationVector = Encoding.UTF8.GetBytes(iv);

                return Convert.ToBase64String(cc.Crypt(bytePlain, password, inicializationVector, true));
            }
            catch
            {
                return string.Empty;
            }
        }

        public string Decode(string cipher, string key, string iv)
        {
            try
            {
                byte[] byteCipher = Convert.FromBase64String(cipher);
                byte[] password = Encoding.UTF8.GetBytes(key);
                byte[] inicializationVector = Encoding.UTF8.GetBytes(iv);

                return Encoding.UTF8.GetString(cc.Decrypt(byteCipher, password, inicializationVector));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
