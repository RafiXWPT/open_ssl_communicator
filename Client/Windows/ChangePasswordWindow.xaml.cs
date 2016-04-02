using System;
using System.Net;
using System.Windows;
using CommunicatorCore.Classes.Model;
using Config;

namespace Client.Windows
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private readonly Uri _passwordChangeUrl = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("PASSWORD_API") + "/");

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if(!RegisterFormatValidator.IsPasswordValid(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Password is not secure, try different one.");
                return;
            }
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords are different.");
                return;
            }

            ChangePasswordDTO changePasswordDto = new ChangePasswordDTO(ConnectionInfo.Sender, CurrentPasswordBox.Password, ConfirmPasswordBox.Password);

            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.LoadRsaFromPrivateKey(ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
            cryptoService.LoadRsaFromPublicKey("SERVER_Public.pem");

            string plainMessage = changePasswordDto.GetJsonString();
            string encryptedMessage = cryptoService.PublicEncrypt(plainMessage, cryptoService.PublicRSA);

            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "CHANGE_PASSWORD", plainMessage, encryptedMessage);
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = NetworkController.Instance.SendMessage(_passwordChangeUrl, client, contactsRequestMessage);
                HandlePasswordResponse(reply, cryptoService);
            }
        }

        private void HandlePasswordResponse(string reply, CryptoRSA cryptoService)
        {
            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            string decryptedContent = cryptoService.PrivateDecrypt(returnedMessage.MessageContent, cryptoService.PrivateRSA);
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                MessageBox.Show("Bad checksum message" + decryptedContent);
            }
            else if (decryptedContent == "CHANGE_OK")
            {
                MessageBox.Show("Password changed successfully");
                this.Close();
            }
            else if (decryptedContent == "CHANGE_INVALID_CURRENT")
            {
                MessageBox.Show("Invalid current password");
            }
        }
    }
}
