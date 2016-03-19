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

    public enum CiherType
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