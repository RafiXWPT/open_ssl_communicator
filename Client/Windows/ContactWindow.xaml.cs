using System.Windows;
using System.IO;
using CommunicatorCore.Classes.Model;
using System;
using System.Net;
using Config;

namespace Client
{
    /// <summary>
    /// Interaction logic for KeyImportWindow.xaml
    /// </summary>
    public partial class ContactWindow : Window
    {

        private readonly Uri _contactsUriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
            ConfigurationHandler.GetValueFromKey("CONTACTS_API") + "/");

        private readonly bool _isEdit;
        
        public ContactWindow()
        {
            InitializeComponent();
            Title = Title + " Add new contact.";
            _isEdit = false;
        }

        public ContactWindow(string to, string displayName)
        {
            _isEdit = true;

            InitializeComponent();
            Title = Title + " Edit contact.";
            contactName.IsReadOnly = true;
            contactName.Text = to;
            contactDisplayName.Text = displayName;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            cryptoService.LoadRsaFromPublicKey("SERVER_Public.pem");

            SymmetricCipher cipher = new SymmetricCipher();
            string token = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");

            Contact contact = new Contact(ConnectionInfo.Sender, contactName.Text, contactDisplayName.Text);
            contact.CipheredTo = cipher.Encode(contactName.Text, token, string.Empty);
            contact.DisplayName = cipher.Encode(contactDisplayName.Text, token, string.Empty);

            string plainMessage = contact.GetJsonString();
            string encryptedMessage = cryptoService.PublicEncrypt(plainMessage, cryptoService.PublicRSA);
            string messageType = _isEdit ? "CONTACT_UPDATE" : "CONTACT_INSERT";

            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, messageType, plainMessage, encryptedMessage);
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = NetworkController.Instance.SendMessage(_contactsUriString, client, contactsRequestMessage);
                HandleContactsResponse(reply, cryptoService, contact);
            }
        }

        private void HandleContactsResponse(string reply, CryptoRSA cryptoService, Contact contact)
        {
            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            string decryptedContent = cryptoService.PrivateDecrypt(returnedMessage.MessageContent, cryptoService.PrivateRSA);
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Bad checksum message");
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_SUCCESS")
            {
                string message = _isEdit ? "Contact added successfully!" : "Contact updated successfully";
                MessageBox.Show(message);
                MainWindow.Instance.AddContactToList(contact, true);
                this.Close();
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_ALREADY_EXIST")
            {
                MessageBox.Show("Contact already exists");
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_USER_NOT_EXIST")
            {
                MessageBox.Show("Requested user does not exists");
            }
        }
    }
}
