using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.IO;

namespace Client
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("SEND_CHAT_MESSAGE_API") + "/");
        NameValueCollection headers = new NameValueCollection();
        NameValueCollection data = new NameValueCollection();

        private readonly CryptoRSA rsa;

        public ChatWindow()
        {
            rsa = new CryptoRSA("SERVER_Public.pem", ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMessage();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void SendMessage()
        {

            ControlMessage message = new ControlMessage();
            try
            {
                Message chatMessage = new Message(ConnectionInfo.Sender, ConnectionInfo.Sender, chatText.Text);
                string encryptedChatMessage = rsa.Encrypt(chatMessage.GetJsonString());
                message = new ControlMessage(ConnectionInfo.Sender, "CHAT_ECHO", encryptedChatMessage);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            //Message message = new Message("THATS ME", "TO ANYONE", chatText.Text);
            using (var wb = new WebClient())
            {
                wb.Proxy = null;
                headers["messageContent"] = message.GetJsonString();
                wb.Headers.Add(headers);
                data["DateTime"] = DateTime.Now.ToShortDateString();
                byte[] response = wb.UploadValues(uriString, "POST", data);

                listBox.Items.Add(Encoding.UTF8.GetString(response));
                chatText.Text = string.Empty;
            }
        }
    }
}