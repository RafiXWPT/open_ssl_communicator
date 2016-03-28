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
    public partial class ResetPasswordWindow : Window
    {

        private DiffieHellmanTunnel tunnel;
        private DiffieHellmanTunnelCreator creator;
        
        private readonly Uri _registerUriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
            ConfigurationHandler.GetValueFromKey("REGISTER_API") + "/");


        public ResetPasswordWindow(DiffieHellmanTunnel tunnel, DiffieHellmanTunnelCreator creator)
        {
            InitializeComponent();
            this.tunnel = tunnel;
            this.creator = creator;
        }

        private void ResetPwdBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ConnectionInfo.Connected)
            {
                MessageBox.Show("Server is unavailable.");
                return;
            }

            if (creator.isTunnelActive(tunnel))
            {
                UserPasswordData userPasswordData = new UserPasswordData(EmailTextBox.Text, string.Empty);
                string toSend = userPasswordData.GetJsonString();
                ControlMessage registrationMessage = new ControlMessage(ConnectionInfo.Sender, "RESET_PASSWORD", toSend, tunnel.DiffieEncrypt(toSend));
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(_registerUriString, client, registrationMessage);
                    HandleResetResponse(reply);
                }
            }
            else
            {
                MessageBox.Show("Diffie Hellman Tunnel is not established. Establishing new one.");
                tunnel = creator.EstablishTunnel();
            }
        }

        private void HandleResetResponse(string reply)
        {
            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            if (returnedMessage.MessageType == "RESET_PASSWORD")
            {
                string messageContent = tunnel.DiffieDecrypt(returnedMessage.MessageContent);
                if (messageContent == "RESET_OK")
                {
                    MessageBox.Show("Reset password successful. Check your email.");
                    this.Close();
                }
                else if (messageContent == "RESET_INVALID")
                {
                    MessageBox.Show("User does not exist.");
                }
                // There new types of messages;
            }
            else if (returnedMessage.MessageType == "INVALID")
            {
                MessageBox.Show("Exception occurred: " + returnedMessage.MessageContent);

            }
        }
    }
}
