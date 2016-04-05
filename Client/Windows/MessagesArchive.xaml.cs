using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunicatorCore.Classes.Model;
using CommunicatorCore.Classes.Service;
using Config;

namespace Client
{
    /// <summary>
    /// Interaction logic for MessagesArchive.xaml
    /// </summary>
    public partial class MessagesArchive : Window
    {

        private readonly Uri _messageArchiveUri = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("MESSAGE_HISTORY_API") + "/");

        private readonly string _token;
        private readonly SymmetricCipher _cipher;
        private readonly List<DisplayContact> contactsList = new List<DisplayContact>();


        public MessagesArchive(ItemCollection contacts)
        {
            InitializeComponent();
            foreach (var contact in contacts)
            {
                contactsList.Add(contact as DisplayContact);
            }

            ArchiveContactsData.Items.Clear();
            contactsList.ForEach( contact => ArchiveContactsData.Items.Add(contact));

            _token = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
            _cipher = new SymmetricCipher();
        }

        private void ArchiveContactsData_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayContact contactView = ArchiveContactsData.SelectedItem as DisplayContact;
            if( contactView != null ) {
                ArchiveMessagesList.Items.Clear();
                Contact contact = new Contact(ConnectionInfo.Sender, contactView.ContactID);
                BatchControlMessage archiveDataControlMessage = ControlMessageParser.CreateResponseBatchMessage(CryptoRSAService.CryptoService, _cipher, ConnectionInfo.Sender, "MESSAGE_GET", contact);
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(_messageArchiveUri, client, archiveDataControlMessage);
                    HandleMessageHistoryResponse(reply);
                }
            }
        }

        private void HandleMessageHistoryResponse(string reply)
        {
            BatchControlMessage returnedMessage = new BatchControlMessage();
            returnedMessage.LoadJson(reply);

            ControlMessage returnedControlMessage = returnedMessage.ControlMessage;
            string decryptedAes = CryptoRSAService.CryptoService.PrivateDecrypt(returnedMessage.CipheredKey);

            string decryptedContent = _cipher.Decode(returnedControlMessage.MessageContent, decryptedAes, string.Empty);
            if (returnedControlMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Bad checksum message" + decryptedContent);
            }
            else if (returnedControlMessage.MessageType == "MESSAGE_GET_OK")
            {
                MessageAggregator messageAggregator = new MessageAggregator();
                messageAggregator.LoadJson(decryptedContent);
                
                List<DisplayMessage> filteredMessages = new List<DisplayMessage>();
                // Now we have to iterate over received data and discard records which does not belong to us - their checksum is invalid
                foreach (var message in messageAggregator.Messages)
                {
                    string decryptedMessageContent = _cipher.Decode(message.MessageCipheredContent, _token, string.Empty);
                    if (Sha1Util.CalculateSha(decryptedMessageContent) == message.Checksum)
                    {
                        filteredMessages.Add(new DisplayMessage(string.Empty, message.MessageSender, decryptedMessageContent, message.MessageSender == ConnectionInfo.Sender ? true : false, message.MessageDate));
                    }
                }

                foreach (DisplayMessage message in filteredMessages)
                {
                    ArchiveMessagesList.Items.Add(message);
                }
            }
        }
    }
}
