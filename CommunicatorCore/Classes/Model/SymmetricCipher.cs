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

        public byte[] encode(byte[] message, string key, string iv)
        {
            try
            {
                return cc.Crypt(message, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(iv), true);
            }
            catch
            {
                return null;
            }
        }

        public byte[] decode(byte[] cipher, string key, string iv)
        {
            try
            {
                return cc.Decrypt(cipher, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(iv));
            }
            catch
            {
                return null;
            }
        }
    }
}
