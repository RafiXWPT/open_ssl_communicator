using System;
using System.Net;
using System.Threading;
using System.Windows;
using CommunicatorCore.Classes.Model;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Uri ContactsUriString =
            new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
                    ConfigurationHandler.GetValueFromKey("CONTACTS_API") + "/");

        private readonly Uri HistoryUriString =
            new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
                    ConfigurationHandler.GetValueFromKey("HISTORY_API") + "/");

        //private readonly string ServerKeyPath = "SERVER_Public.pem";

        private ConnectionChecker connectionChecker;
        private NetworkController networkController;

        public MainWindow(ConnectionChecker connectionChecker, NetworkController networkController,
            string loggedUserName)
        {
            InitializeComponent();
            this.connectionChecker = connectionChecker;
            this.networkController = networkController;
            this.Title = "Crypto Talk - " + loggedUserName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void GetHistory(object sender, RoutedEventArgs e)
        {
            Contact contact = new Contact(ConnectionInfo.Sender, "other@admin.com");
            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "HISTORY_GET",
                contact.GetJsonString());
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = NetworkController.Instance.SendMessage(HistoryUriString, client, contactsRequestMessage);
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
            ChatWindow chat = new ChatWindow();
            chat.Show();
        }

        private void Contacts_OnClick_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfigurationHandler.HasValueOnKey("PATH_TO_PRIVATE_KEY"))
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
                    string reply = NetworkController.Instance.SendMessage(ContactsUriString, client,
                        contactsRequestMessage);
                    HandleContactsResponse(reply);
                }
            }
        }

        private void HandleContactsResponse(string reply)
        {
            CryptoRSA decoder = new CryptoRSA();
            //null, ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY")
            decoder.loadRSAFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));

            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            string decryptedContent = decoder.PrivateDecrypt(returnedMessage.MessageContent, decoder.PrivateRSA);
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Zla checksuma wiadomosci");
            }
            else if (returnedMessage.MessageType == "CONTACT_GET_OK")
            {
                //              TODO: Content of the message should be decrypted
                ContactAggregator aggregator = new ContactAggregator();
                aggregator.LoadJson(returnedMessage.MessageContent);
                aggregator.Contacts.ForEach(c => contactsData.Items.Add(new {Name = c.To, DisplayName = c.DisplayName}));
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_SUCCESS")
            {
                //                Ok
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_ERROR")
            {
                //                E-e
            }
        }
    }
}