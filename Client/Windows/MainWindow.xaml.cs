using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Windows;
using CommunicatorCore.Classes.Model;
using Config;
using System.Windows.Media.Imaging;
using Client.Classes;

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
        private StatusController _statusController;

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

        private void ImportKeys_Click(object sender, RoutedEventArgs e)
        {
            KeyImportWindow keyImporter = new KeyImportWindow();
            keyImporter.Show();
        }

        private void Contacts_Click(object sender, RoutedEventArgs e)
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
            returnedBatchMessage.LoadJson(reply);
            ControlMessage returnedMessage = returnedBatchMessage.ControlMessage;

            SymmetricCipher cipher = new SymmetricCipher();
            string AESKey = decoder.PrivateDecrypt(returnedBatchMessage.CipheredKey, decoder.PrivateRSA);
            string decryptedContent = cipher.Decode(returnedMessage.MessageContent, AESKey, string.Empty);
            
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Bad contacts checksum. " + decryptedContent + " chechsum ");
            }
            else if (returnedMessage.MessageType == "CONTACT_GET_OK")
            {
                if (_statusController == null)
                {
                    _statusController = new StatusController();
                }
                else
                {
                    _statusController.ClearUsers();
                }
                ContactAggregator aggregator = new ContactAggregator();
                aggregator.LoadJson(decryptedContent);
                aggregator.Contacts.ForEach(contact => addContactToList(contact, true));
            }
        }

        public void UpdateLatency(double latency, BitmapImage image)
        {
            Application.Current.Dispatcher.Invoke(() => updateLatency(latency, image));
        }

        void updateLatency(double latency, BitmapImage image)
        {
            latencyLabel.Content = "Latency: " + Math.Round(latency, 2) + "ms";
            latencyStatus.Source = image;
        }

        public void AddContactToList(Contact contact, bool isEncrypted = false)
        {
            Application.Current.Dispatcher.Invoke( () => addContactToList(contact, isEncrypted));
        }

        void addContactToList(Contact contact, bool isEncrypted)
        {
            // We have to exclude already displayed users somehow!
            if (isEncrypted)
            {
                SymmetricCipher decoder = new SymmetricCipher();
                string key = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
                string decodedTo = decoder.Decode(contact.CipheredTo, key, string.Empty);
                string decodedDisplayName = decoder.Decode(contact.DisplayName, key, string.Empty);
                if (Sha1Util.CalculateSha(ConnectionInfo.Sender + decodedTo) != contact.ContactChecksum)
                {
                    ContactsData.Items.Add(new Contact { To = "INVALID", DisplayName = "INVALID" });
                }
                else
                {
                    AddItemToContactGrid(decodedTo, decodedDisplayName);
                }
            }
            else
            {
                AddItemToContactGrid(contact.To, contact.DisplayName);
            }
        }

        private void AddItemToContactGrid(string to, string displayName)
        {
            if (!ItemAlreadyExist(to))
            {
                ContactsData.Items.Add(new Contact { To = to, DisplayName = displayName });
                if (_statusController == null)
                {
                    _statusController = new StatusController();
                }
                _statusController.AddUser(to);
            }
            else
            {
                SetContactDisplayName(to, displayName);
            }
        }

        private void SetContactDisplayName(string to, string decodedDisplayName)
        {
            for (int i = 0; i < ContactsData.Items.Count; i++)
            {
                Contact newContact = ContactsData.Items[i] as Contact;
                if( newContact != null && newContact.To == to) {
                    ContactsData.Items[i] = new Contact { To = to, DisplayName = decodedDisplayName};
                }
            }
        }

        private bool ItemAlreadyExist(string to)
        {
            for(int i = 0; i < ContactsData.Items.Count; i++) 
            {
                Contact contact = ContactsData.Items[i] as Contact;
                if (contact.To == to)
                    return true;
            }
            return false;
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options options = new Options();
            options.Show();
        }

        private void AddContact_Click(object sender, RoutedEventArgs e)
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
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes) { 
                Contact selectedContact = ContactsData.SelectedItem as Contact;
                int selectedItemIndex = ContactsData.SelectedIndex;
                if( selectedContact != null ) {
                    CryptoRSA cryptoService = new CryptoRSA();
                    cryptoService.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
                    cryptoService.LoadRsaFromPublicKey("SERVER_Public.pem");

                    SymmetricCipher cipher = new SymmetricCipher();
                    string token = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");

                    Contact contact = new Contact(ConnectionInfo.Sender, selectedContact.To, selectedContact.DisplayName);
                    contact.CipheredTo = cipher.Encode(selectedContact.To, token, string.Empty);
                    contact.DisplayName = cipher.Encode(selectedContact.DisplayName, token, string.Empty);

                    string plainMessage = contact.GetJsonString();
                    string encryptedMessage = cryptoService.PublicEncrypt(plainMessage, cryptoService.PublicRSA);

                    ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "CONTACT_DELETE", plainMessage, encryptedMessage);
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        string reply = NetworkController.Instance.SendMessage(_contactsUriString, client, contactsRequestMessage);
                        HandleContactsResponse(reply, cryptoService, contact.To, selectedItemIndex);
                    }
                }
            }
        }

        private void HandleContactsResponse(string reply, CryptoRSA cryptoService,string username, int index)
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
                _statusController.RemoveUser(username);
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

        private void ChangePasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow window = new ChangePasswordWindow();
            window.Show();
        }

        private void ClearContacts_Click(object sender, RoutedEventArgs e)
        {
            ContactsData.Items.Clear();
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            _networkController.StopChatListener();
            connectionChecker.StopCheckConnection();
            Close();
        }

        private void OnUnselectBtn_Click(object sender, RoutedEventArgs e)
        {
            ContactsData.UnselectAll();
            ContactActionPanel.Visibility = Visibility.Hidden;
        }
    }
}