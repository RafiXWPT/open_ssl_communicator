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
    public partial class AddContactWindow : Window
    {

        private readonly Uri _contactsUriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
            ConfigurationHandler.GetValueFromKey("CONTACTS_API") + "/");


        public AddContactWindow()
        {
            InitializeComponent();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.loadRSAFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            cryptoService.loadRSAFromPublicKey("SERVER_Public.pem");

            Contact contact = new Contact(ConnectionInfo.Sender, contactName.Text, contactDisplayName.Text);
            string plainMessage = contact.GetJsonString();
            string encryptedMessage = cryptoService.PublicEncrypt(plainMessage, cryptoService.PublicRSA);

            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "CONTACT_INSERT", plainMessage, encryptedMessage);
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
                MessageBox.Show("Zła checksuma wiadomosci");
            }
            else if (returnedMessage.MessageType == "CONTACT_INSERT_SUCCESS")
            {
                MessageBox.Show("Contact added successfully!");
                MainWindow.Instance.AddContactToList(contact);
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
