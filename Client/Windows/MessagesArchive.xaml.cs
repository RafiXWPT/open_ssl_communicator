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

namespace Client.Windows
{
    /// <summary>
    /// Interaction logic for MessagesArchive.xaml
    /// </summary>
    public partial class MessagesArchive : Window
    {

        private readonly Uri _messageArchiveUri = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("MESSAGE_HISTORY_API") + "/");

        private readonly string _token;
        private readonly SymmetricCipher _cipher;
        private readonly CryptoRSA _cryptoService;


        public MessagesArchive(List<DisplayContact> contacts)
        {
            InitializeComponent();
            ArchiveContactsData.Items.Clear();
            contacts.ForEach( contact => ArchiveContactsData.Items.Add(contact));

            _token = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
            _cipher = new SymmetricCipher();
            _cryptoService = new CryptoRSA();
            _cryptoService.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            _cryptoService.LoadRsaFromPublicKey("SERVER_Public.pem");
        }

        private void ArchiveContactsData_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayContact contactView = ArchiveContactsData.SelectedItem as DisplayContact;
            if( contactView != null ) { 
                Contact contact = new Contact(ConnectionInfo.Sender, contactView.ContactID);
                BatchControlMessage archiveDataControlMessage = ControlMessageParser.CreateResponseBatchMessage(_cryptoService, _cipher, ConnectionInfo.Sender, "MESSAGE_GET", contact);
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
            string decryptedAes = _cryptoService.PrivateDecrypt(returnedMessage.CipheredKey);

            string decryptedContent = _cipher.Decode(returnedControlMessage.MessageContent, decryptedAes, String.Empty);
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
                        filteredMessages.Add(new DisplayMessage("" /* UID in this case does not matter */, message.MessageSender, decryptedMessageContent, false /* this should also does not matter */ ));
                    }
                }

                MessageBox.Show(decryptedContent + ". Total: " + filteredMessages.Count);
                
                // Now my Dear - it's your turn :* 
            }
        }
    }
}
