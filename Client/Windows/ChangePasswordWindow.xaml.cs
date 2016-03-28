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
            if (!AreKeysInitialized())
            {
                MessageBox.Show("Load your keys at first!");
            }
            else { 
                // We should validate passwords!
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

        }

        private bool AreKeysInitialized()
        {
            return ConfigurationHandler.HasValueOnKey("PATH_TO_PRIVATE_KEY");
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
