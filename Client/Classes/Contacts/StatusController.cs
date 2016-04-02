using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunicatorCore.Classes.Model;
using CommunicatorCore.Classes.Service;
using Config;

namespace Client
{
    public class StatusController
    {
        private readonly Uri _contactsUriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
            ConfigurationHandler.GetValueFromKey("STATUS_API") + "/");
        private readonly Timer _timer;
        private readonly List<UserConnectionStatus> _contactStatuses = new List<UserConnectionStatus>();
        private readonly CryptoRSA _crypter;
        private readonly SymmetricCipher _cipher = new SymmetricCipher();

        private static StatusController instance;
        public static StatusController Instance { get { return instance; } }

        // FIXME: We have to put path to server key in one place
        public StatusController()
        {
            instance = this;
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            _timer = new Timer(CheckStatus, autoResetEvent, 0, 10000);
            _crypter = new CryptoRSA();
            _crypter.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            _crypter.LoadRsaFromPublicKey("SERVER_Public.pem");
        }

        // This method is called by the _timer delegate.
        public void CheckStatus(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            autoEvent.Set();

            if( _contactStatuses.Count > 0 ) { 
                UserConnectionStatusAggregator userConnectionStatusAggregator = new UserConnectionStatusAggregator(_contactStatuses);

                BatchControlMessage batchMessage = ControlMessageParser.CreateResponseBatchMessage(_crypter, _cipher,
                    ConnectionInfo.Sender, "CONTACT_STATUS", userConnectionStatusAggregator);

                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(_contactsUriString, client, batchMessage);
                    HandleContactsStatusResponse(reply);
                }

                MainWindow.Instance.UpdateContactsStatus();
            }

        }

        private void HandleContactsStatusResponse(string reply)
        {
            BatchControlMessage returnedBatchMessage = new BatchControlMessage();
            returnedBatchMessage.LoadJson(reply);
            ControlMessage returnedControlMessage = returnedBatchMessage.ControlMessage;
            string decryptedKey = _crypter.PrivateDecrypt(returnedBatchMessage.CipheredKey);
            string decryptedContent = _cipher.Decode(returnedControlMessage.MessageContent, decryptedKey, string.Empty);
            UserConnectionStatusAggregator connectionStatusAggregator = new UserConnectionStatusAggregator();
            connectionStatusAggregator.LoadJson(decryptedContent);
            // Decrypted content contains jsons about user statuses
            foreach(UserConnectionStatus status in connectionStatusAggregator.ConnectionStatus)
            {
                _contactStatuses.Find(x => x.Username == status.Username).ConnectionStatus = status.ConnectionStatus;
            }
        }

        public void RemoveUser(string username)
        {
            _contactStatuses.Remove(new UserConnectionStatus(username));
        }

        public void AddUser(string username)
        {
            _contactStatuses.Add(new UserConnectionStatus(username));
        }

        public string GetUserStatus(string username)
        {
            string status = "Offline";
            string getStatus = _contactStatuses.Find(x => x.Username == username).ConnectionStatus;
            if (getStatus != null) { 
                status = getStatus;
            }

            return status;
        }

        public void ClearUsers()
        {
            _contactStatuses.Clear();
        }
        
    }
}
