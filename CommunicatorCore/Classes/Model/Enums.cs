namespace CommunicatorCore.Classes.Model
{
    public enum CipherType
    {
        AES,
        BLOWFISH,
        CAST5
    }

    public enum CipherMode
    {
        CBC,
        ECB
    }

    public enum CipherSize
    {
        SIZE_128,
        SIZE_192,
        SIZE_256
    }
}
