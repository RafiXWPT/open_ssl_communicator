using System;
using System.Net;
using System.Threading;
using System.Windows;
using CommunicatorCore.Classes.Model;
using Config;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Uri _contactsUriString =
            new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
                    ConfigurationHandler.GetValueFromKey("CONTACTS_API") + "/");

        private readonly Uri _historyUriString =
            new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
                    ConfigurationHandler.GetValueFromKey("HISTORY_API") + "/");

        private static MainWindow instance;

        public static MainWindow Instance { get { return instance; } }

        private ConnectionChecker connectionChecker;
        private NetworkController networkController;

        public MainWindow(ConnectionChecker connectionChecker, NetworkController networkController,
            string loggedUserName)
        {
            instance = this;

            InitializeComponent();
            this.connectionChecker = connectionChecker;
            this.networkController = networkController;
            this.Title = "Crypto Talk - " + loggedUserName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            networkController.StartChatListener();
        }

        private void GetHistory(object sender, RoutedEventArgs e)
        {
            Contact contact = new Contact(ConnectionInfo.Sender, "other@admin.com");
            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "HISTORY_GET",
                contact.GetJsonString());
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = NetworkController.Instance.SendMessage(_historyUriString, client, contactsRequestMessage);
                HandleContactsResponse(reply);
            }
        }

        private void importKeys_Click(object sender, RoutedEventArgs e)
        {
            KeyImportWindow keyImporter = new KeyImportWindow();
            keyImporter.Show();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chat = new ChatWindow(ConnectionInfo.Sender);
            chat.Show();
        }

        private void Contacts_OnClick_Click(object sender, RoutedEventArgs e)
        {
            if (!AreKeysInitialized())
            {
                MessageBox.Show("Load keys at first");
            }
            else
            {
                ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "CONTACT_GET",
                    string.Empty);
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(_contactsUriString, client,
                        contactsRequestMessage);
                    HandleContactsResponse(reply);
                }
            }
        }

        private void HandleContactsResponse(string reply)
        {
            CryptoRSA decoder = new CryptoRSA();
            decoder.loadRSAFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));

            BatchControlMessage returnedBatchMessage = new BatchControlMessage();
            ControlMessage returnedMessage = new ControlMessage();
            returnedBatchMessage.LoadJson(reply);
            returnedMessage = returnedBatchMessage.ControlMessage;

            SymmetricCipher cipher = new SymmetricCipher();
            string AESKey = decoder.PrivateDecrypt(returnedBatchMessage.CipheredKey, decoder.PrivateRSA);
            string decryptedContent = cipher.Decode(returnedMessage.MessageContent, AESKey, string.Empty);
            
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(returnedMessage.MessageContent))
            {
                MessageBox.Show("Bad contacts checksum");
            }
            else if (returnedMessage.MessageType == "CONTACT_GET_OK")
            {
                ContactAggregator aggregator = new ContactAggregator();
                aggregator.LoadJson(decryptedContent);
                aggregator.Contacts.ForEach(addContactToList);
            }
        }

        public void AddContactToList(Contact contact)
        {
            Application.Current.Dispatcher.Invoke( () => addContactToList(contact));
        }

        void addContactToList(Contact contact)
        {
            // We have to exclude already displayed users somehow!
            contactsData.Items.Add(new Contact (contact.To, contact.DisplayName));
        }

        private void options_Click(object sender, RoutedEventArgs e)
        {
            Options options = new Options();
            options.Show();
        }

        private void AddContact_OnClick(object sender, RoutedEventArgs e)
        {
            if (AreKeysInitialized())
            {
                AddContactWindow addNewContact = new AddContactWindow();
                addNewContact.Show();
            }
            else
            {
                MessageBox.Show("Load you keys at first");
            }
        }

        private bool AreKeysInitialized()
        {
            return ConfigurationHandler.HasValueOnKey("PATH_TO_PRIVATE_KEY");
        }

        class Test
        {
            public string To { get; set; }
            public string DisplayName { get; set; }
        }
    }
}