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
    public partial class KeyImportWindow : Window
    {
        private readonly Uri _passwordUriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" +
                ConfigurationHandler.GetValueFromKey("PASSWORD_API") + "/");


        private string _pathToPrivateKey;
        private string _pathToPublicKey;

        public KeyImportWindow()
        {
            InitializeComponent();
            SaveButton.IsEnabled = false;

            LoadConfiguration();
        }

        void LoadConfiguration()
        {
            if (ConfigurationHandler.HasValueOnKey("PATH_TO_PRIVATE_KEY"))
            {
                _pathToPrivateKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY");
                PrivateKeyPath.Text = Path.GetFileName(_pathToPrivateKey);
            }

            if (ConfigurationHandler.HasValueOnKey("PATH_TO_PUBLIC_KEY"))
            {
                _pathToPublicKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PUBLIC_KEY");
                PublicKeyPath.Text = Path.GetFileName(_pathToPublicKey);
            }
            if (ConfigurationHandler.HasValueOnKey("TOKEN_VALUE"))
            {
                TokenValueTextBox.Text = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
            }
        }

        string SelectFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".pem";
            dlg.Filter = "PEM Keys (*.pem)|*.pem";

            bool? result = dlg.ShowDialog();
            return result == true ? dlg.FileName : string.Empty;
        }

        private void SearchPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            _pathToPublicKey = SelectFile();
            PublicKeyPath.Text = Path.GetFileName(_pathToPublicKey);
        }

        private void SearchPrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            _pathToPrivateKey = SelectFile();
            PrivateKeyPath.Text = Path.GetFileName(_pathToPrivateKey);
        }

        private void ValidateKeysButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AreKeysValid() && IsTokenValid())
                {
                    MessageBox.Show("Test passed!");
                    SaveButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Test failed!");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool IsTokenValid()
        {
            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.LoadRsaFromPrivateKey(_pathToPrivateKey);
            cryptoService.LoadRsaFromPublicKey("SERVER_Public.pem");

            UserTokenDto userTokenDto = new UserTokenDto(ConnectionInfo.Sender, TokenValueTextBox.Text);
            string plainMessage = userTokenDto.GetJsonString();
            string encryptedMessage = cryptoService.PublicEncrypt(plainMessage);
            ControlMessage contactsRequestMessage = new ControlMessage(ConnectionInfo.Sender, "TOKEN_VALIDATE", plainMessage, encryptedMessage);
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = NetworkController.Instance.SendMessage(_passwordUriString, client, contactsRequestMessage);
                return HandleTokenResponse(reply, cryptoService);
            }
        }

        private bool HandleTokenResponse(string reply, CryptoRSA cryptoService)
        {
            ControlMessage returnedMessage = new ControlMessage();
            returnedMessage.LoadJson(reply);
            string decryptedContent = cryptoService.PrivateDecrypt(returnedMessage.MessageContent);
            if (returnedMessage.Checksum != Sha1Util.CalculateSha(decryptedContent))
            {
                return false;
            }
            return decryptedContent == "TOKEN_VALIDATE_OK";
        }

        private bool AreKeysValid()
        {
            CryptoRSA cryptoService = new CryptoRSA();
            cryptoService.LoadRsaFromPrivateKey(_pathToPrivateKey);
            cryptoService.LoadRsaFromPublicKey(_pathToPublicKey);

            string checker = "CHECK_ME";
            string encryptedCheck = cryptoService.PublicEncrypt(checker);
            string decryptedCheck = cryptoService.PrivateDecrypt(encryptedCheck);
            return checker == decryptedCheck;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationHandler.SetValueOnKey("PATH_TO_PRIVATE_KEY", _pathToPrivateKey);
            ConfigurationHandler.SetValueOnKey("PATH_TO_PUBLIC_KEY", _pathToPublicKey);
            ConfigurationHandler.SetValueOnKey("TOKEN_VALUE", TokenValueTextBox.Text);
            this.Close();
        }
    }
}
