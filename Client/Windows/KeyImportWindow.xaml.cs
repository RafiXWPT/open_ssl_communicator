using System.Windows;
using System.IO;
using CommunicatorCore.Classes.Model;
using System;
using Config;

namespace Client
{
    /// <summary>
    /// Interaction logic for KeyImportWindow.xaml
    /// </summary>
    public partial class KeyImportWindow : Window
    {
        private string pathToPrivateKey;
        private string pathToPublicKey;

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
                pathToPrivateKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY");
                PrivateKeyPath.Text = Path.GetFileName(pathToPrivateKey);
            }

            if (ConfigurationHandler.HasValueOnKey("PATH_TO_PUBLIC_KEY"))
            {
                pathToPublicKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PUBLIC_KEY");
                PublicKeyPath.Text = Path.GetFileName(pathToPublicKey);
            }
            if (ConfigurationHandler.HasValueOnKey("TOKEN_VALUE"))
            {
                TokenValue.Text = ConfigurationHandler.GetValueFromKey("TOKEN_VALUE");
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
            pathToPublicKey = SelectFile();
            PublicKeyPath.Text = Path.GetFileName(pathToPublicKey);
        }

        private void SearchPrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            pathToPrivateKey = SelectFile();
            PrivateKeyPath.Text = Path.GetFileName(pathToPrivateKey);
        }

        private void CheckKeysButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CryptoRSA cryptoService = new CryptoRSA();
                cryptoService.LoadRsaFromPrivateKey(pathToPrivateKey);
                cryptoService.LoadRsaFromPublicKey(pathToPublicKey);

                string checker = "CHECK_ME";
                string encryptedCheck = cryptoService.PublicEncrypt(checker, cryptoService.PublicRSA);
                string decryptedCheck = cryptoService.PrivateDecrypt(encryptedCheck, cryptoService.PrivateRSA);

                if (checker == decryptedCheck)
                {
                    MessageBox.Show("TEST_PASSED");
                    SaveButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("TEST_FAILED");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationHandler.SetValueOnKey("PATH_TO_PRIVATE_KEY", pathToPrivateKey);
            ConfigurationHandler.SetValueOnKey("PATH_TO_PUBLIC_KEY", pathToPublicKey);
            ConfigurationHandler.SetValueOnKey("TOKEN_VALUE", TokenValue.Text);
            this.Close();
        }
    }
}
