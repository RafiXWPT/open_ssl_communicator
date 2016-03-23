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
using System.Media;

namespace Client
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("SEND_CHAT_MESSAGE_API") + "/");
        private readonly Uri incomingMessage = new Uri("Media/incoming.wav", UriKind.Relative);
        private readonly Uri outcomingMessage = new Uri("Media/outcoming.wav", UriKind.Relative);

        NameValueCollection headers = new NameValueCollection();
        NameValueCollection data = new NameValueCollection();
        MediaPlayer player = new MediaPlayer();

        private readonly CryptoRSA cryptoService;

        public ChatWindow()
        {
            InitializeComponent();

            cryptoService = new CryptoRSA();
            cryptoService.loadRSAFromPublicKey("SERVER_Public.pem");
            chatText.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(SendInitMessage);
            thread.IsBackground = true;
            thread.Start();
        }

        private void SendMsg()
        {
            if (string.IsNullOrWhiteSpace(chatText.Text))
                return;

            try
            {
                AddMessageToChatWindow(ConnectionInfo.Sender, chatText.Text, true);
                PrepareMessage(chatText.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void PlaySound(bool outcoming = false)
        {
            if(outcoming)
            {
                player.Open(outcomingMessage);
            }
            else
            {
                player.Open(incomingMessage);
            }
            player.Play();
        }

        void AddMessageToChatWindow(string userName, string messageContent, bool isFromSelf = false)
        {
            listBox.Items.Insert(0, new DisplayMessage(userName, messageContent, isFromSelf));
            PlaySound(isFromSelf);
        }

        void SendInitMessage()
        {
            ControlMessage message = new ControlMessage();
            try
            {
                Message chatMessage = new Message(ConnectionInfo.Sender, ConnectionInfo.Sender, "INIT");
                string encryptedChatMessage = cryptoService.PublicEncrypt(chatMessage.GetJsonString(), cryptoService.PublicRSA);
                message = new ControlMessage(ConnectionInfo.Sender, "CHAT_INIT", encryptedChatMessage);

                string responseString = string.Empty;
                using (var wb = new WebClient())
                {
                    wb.Proxy = null;
                    headers["messageContent"] = message.GetJsonString();
                    wb.Headers.Add(headers);
                    data["DateTime"] = DateTime.Now.ToShortDateString();
                    byte[] responseByte = wb.UploadValues(uriString, "POST", data);
                    responseString = Encoding.UTF8.GetString(responseByte);
                }

                if (responseString == "OK")
                {
                    listBox.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        AddMessageToChatWindow("TUNNEL CREATOR", "Encrypted channel has been established.");
                    }));
                    chatText.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        chatText.IsEnabled = true;
                    }));
                }
                else
                {
                    listBox.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        AddMessageToChatWindow("TUNNEL CREATOR", "Encrypted channel is not established.");
                    }));
                    chatText.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        chatText.IsEnabled = true;
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
                message = new ControlMessage(ConnectionInfo.Sender, "CHAT_MESSAGE", encryptedChatMessage);

                Thread SendReceiveMessage = StartThreadWithParam(message);
                chatText.Text = string.Empty;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public Thread StartThreadWithParam(ControlMessage messageToSend)
        {
            var t = new Thread(() => SendMessage(messageToSend));
            t.Start();
            return t;
        }


        void SendMessage(ControlMessage message)
        {
            try
            {
                using (var wb = new WebClient())
                {
                    wb.Proxy = null;
                    headers["messageContent"] = message.GetJsonString();
                    wb.Headers.Add(headers);
                    data["DateTime"] = DateTime.Now.ToShortDateString();
                    byte[] responseByte = wb.UploadValues(uriString, "POST", data);
                    string responseString = Encoding.UTF8.GetString(responseByte);
                    // Do The magic
                    // Handle Response
                }
            }
            catch
            {

            }
        }

        void HandleResponse(string response)
        {
            // When response will be computed
            // AddMessageToChatWindow(Sender, Content);
        }

        private void chatText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMsg();
        }
    }
}