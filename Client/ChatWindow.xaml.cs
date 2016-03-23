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
using OpenSSL.Crypto;
using System.Threading;

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

        private readonly CryptoRSA cryptoService;

        public ChatWindow()
        {
            InitializeComponent();

            cryptoService = new CryptoRSA();
            cryptoService.loadRSAFromPublicKey("SERVER_Public.pem");

            sendMsg.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(SendInitMessage);
            thread.IsBackground = true;
            thread.Start();
        }

        private void sendMsg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrepareMessage(chatText.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void SendInitMessage()
        {
            ControlMessage message = new ControlMessage();
            try
            {
                Message chatMessage = new Message(ConnectionInfo.Sender, ConnectionInfo.Sender, "INIT");
                string encryptedChatMessage = cryptoService.PublicEncrypt(chatMessage.GetJsonString(), cryptoService.PublicRSA);
                message = new ControlMessage(ConnectionInfo.Sender, "CHAT_INIT", encryptedChatMessage);

                string response = SendMessage(message);
                if(response == "OK")
                {
                    listBox.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        listBox.Items.Add("[" + DateTime.Now + "]:" + " Encrypted channel established.");
                    }));
                    sendMsg.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        sendMsg.IsEnabled = true;
                    }));
                    
                }
                else
                {
                    listBox.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        listBox.Items.Add("[" + DateTime.Now + "]:" + " Encrypted Channel is not established.");
                    }));
                    sendMsg.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        sendMsg.IsEnabled = true;
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void PrepareMessage(string textBoxContent)
        {
            ControlMessage message = new ControlMessage();
            try
            {
                Message chatMessage = new Message(ConnectionInfo.Sender, ConnectionInfo.Sender, textBoxContent);
                string encryptedChatMessage = cryptoService.PublicEncrypt(chatMessage.GetJsonString(), cryptoService.PublicRSA);
                message = new ControlMessage(ConnectionInfo.Sender, "CHAT_ECHO", encryptedChatMessage);

                string response = SendMessage(message);

                listBox.Items.Add("[" + DateTime.Now + "]:" + response);
                chatText.Text = string.Empty;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        string SendMessage(ControlMessage message)
        {
            using (var wb = new WebClient())
            {
                wb.Proxy = null;
                headers["messageContent"] = message.GetJsonString();
                wb.Headers.Add(headers);
                data["DateTime"] = DateTime.Now.ToShortDateString();
                byte[] response = wb.UploadValues(uriString, "POST", data);
                return Encoding.UTF8.GetString(response);
            }
        }
    }
}