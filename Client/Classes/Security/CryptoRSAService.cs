using Config;
using CommunicatorCore.Classes.Model;

namespace Client
{
    class CryptoRSAService
    {
        private static CryptoRSA _cryptoService;
        public static CryptoRSA CryptoService
        {
            get
            {
                if(_cryptoService == null)
                {
                    _cryptoService = GetCryptoService();
                }
                return _cryptoService;
            }
        }

        private static CryptoRSA GetCryptoService()
        {
            CryptoRSA service = new CryptoRSA();
            service.LoadRsaFromPublicKey("SERVER_Public.pem");
            service.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            return service;
        }
    }
}

