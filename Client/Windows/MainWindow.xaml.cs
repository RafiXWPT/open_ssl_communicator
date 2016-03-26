using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private static MainWindow _instance;

        public static MainWindow Instance { get { return _instance; } }

        private ConnectionChecker connectionChecker;
        private readonly NetworkController _networkController;

        public MainWindow(ConnectionChecker connectionChecker, NetworkController networkController,
            string loggedUserName)
        {
            _instance = this;

            InitializeComponent();
            this.connectionChecker = connectionChecker;
            this._networkController = networkController;
            this.Title = "Crypto Talk - " + loggedUserName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _networkController.StartChatListener();
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
            decoder.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));

            BatchControlMessage returnedBatchMessage = new BatchControlMessage();
            ControlMessage returnedMessage = new ControlMessage();
            returnedBatchMessage.LoadJson(reply);
            returnedMessage = returnedBatchMessage.ControlMessage;

            SymmetricCipher cipher = new SymmetricCipher();
            string AESKey = decoder.PrivateDecrypt(returnedBatchMessage.CipheredKey, decoder.PrivateRSA);
            string decryptedContent = cipher.Decode(returnedMessage.MessageContent, AESKey, string.Empty);
            
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Bad contacts checksum. " + decryptedContent + " chechsum ");
            }
            else if (returnedMessage.MessageType == "CONTACT_GET_OK")
            {
                ContactAggregator aggregator = new ContactAggregator();
                aggregator.LoadJson(decryptedContent);
                aggregator.Contacts.ForEach(contact => addContactToList(contact, true));
            }
        }

        public void AddContactToList(Contact contact, bool isEncrypted = false)
        {
            Application.Current.Dispatcher.Invoke( () => addContactToList(contact, isEncrypted));
        }

        void addContactToList(Contact contact, bool isEncrypted)
        {
            // We have to exclude already displayed users somehow!
            string to;
            string displayName;
            if (isEncrypted)
            {
                SymmetricCipher decoder = new SymmetricCipher();
                string key = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
                string decodedTo = decoder.Decode(contact.CipheredTo, key, string.Empty);
                string decodedDisplayName = decoder.Decode(contact.DisplayName, key, string.Empty);
                if (Sha1Util.CalculateSha(ConnectionInfo.Sender + decodedTo) != contact.ContactChecksum)
                {
                    to = "INVALID";
                    displayName = "INVALID";
                }
                else
                {
                    to = decodedTo;
                    displayName = decodedDisplayName;
                }
            }
            else
            {
                to = contact.To;
                displayName = contact.DisplayName;
            }
            ContactsData.Items.Add(new Contact { To = to, DisplayName = displayName });

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
                ContactWindow newContact = new ContactWindow();
                newContact.Show();
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

        private void ContactsData_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedContact = ContactsData.SelectedItem as Contact;
            if (selectedContact != null)
            {
                ChatWindow chatWindow = new ChatWindow(selectedContact.To, selectedContact.DisplayName);
                chatWindow.Show();
            }
        }

        private void OnEditContactBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedContact = ContactsData.SelectedItem as Contact;
            if (selectedContact != null)
            {
                ContactWindow editContactWindow = new ContactWindow(selectedContact.To, selectedContact.DisplayName);
                editContactWindow.Show();
            }
        }

        private void OnDeleteContactBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes) { 
                Contact selectedContact = ContactsData.SelectedItem as Contact;
                int selectedItemIndex = ContactsData.SelectedIndex;
                if( selectedContact != null ) {
                    CryptoRSA cryptoService = new CryptoRSA();
                    cryptoService.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
                    cryptoService.LoadRsaFromPublicKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PUBLIC_KEY"));

                    SymmetricCipher cipher = new SymmetricCipher();
                    string token = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");

                    Contact contact = new Contact(ConnectionInfo.Sender, selectedContact.To, selectedContact.DisplayName);
                    contact.CipheredTo = cipher.Encode(selectedContact.To, token, string.Empty);
                    contact.DisplayName = cipher.Encode(selectedContact.DisplayName, token, string.Empty);

                    string plainMessage = contact.GetJsonString();
                    string encryptedMessage = cryptoService.PublicEncrypt(plainMessage, cryptoService.PublicRSA);

                    ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "CONTACT_INSERT", plainMessage, encryptedMessage);
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        string reply = NetworkController.Instance.SendMessage(_contactsUriString, client, contactsRequestMessage);
                        HandleContactsResponse(reply, cryptoService, selectedItemIndex);
                    }
                }
            }
        }

        private void HandleContactsResponse(string reply, CryptoRSA cryptoService, int index)
        {
            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            string decryptedContent = cryptoService.PrivateDecrypt(returnedMessage.MessageContent, cryptoService.PrivateRSA);
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Z³a checksuma wiadomosci");
            }
            else if (returnedMessage.MessageType == "CONTACT_REMOVE_OK")
            {
                MessageBox.Show("Contact removed successfully!");
                ContactsData.Items.RemoveAt(index);
            }
            else
            {
                MessageBox.Show(decryptedContent);
            }
        }

        private void ContactsData_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContactActionPanel.Visibility = ContactsData.SelectedItem == null ? Visibility.Hidden : Visibility.Visible;
        }

        private void ChangePasswordMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // We should implement changing password behavior
        }
    }
}