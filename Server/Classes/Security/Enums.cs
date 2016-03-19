namespace SystemSecurity
{
    public enum DiffieHellmanTunnelStatus
    {
        NOT_ESTABLISHED,
        ASKING_FOR_ID,
        EXCHANGING_PUBLIC_KEYS,
        EXCHANGING_IV,
        CHECKING_TUNNEL,
        ESTABLISHED,
        FAILURE
    }

    public enum CIPHER_TYPE
    {
        AES,
        BLOWFISH,
        CAST5
    }

    public enum CIPHER_MODE
    {
        CBC,
        ECB
    }

    public enum CIPHER_SIZE
    {
        SIZE_128,
        SIZE_192,
        SIZE_256
    }
}
