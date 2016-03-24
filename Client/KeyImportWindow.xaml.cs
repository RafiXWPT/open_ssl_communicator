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
            save.IsEnabled = false;

            LoadConfiguration();
        }

        void LoadConfiguration()
        {
            if (ConfigurationHandler.HasValueOnKey("PATH_TO_PRIVATE_KEY"))
            {
                pathToPrivateKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY");
                privateKeyPath.Text = Path.GetFileName(pathToPrivateKey);
            }

            if (ConfigurationHandler.HasValueOnKey("PATH_TO_PUBLIC_KEY"))
            {
                pathToPublicKey = ConfigurationHandler.GetValueFromKey("PATH_TO_PUBLIC_KEY");
                publicKeyPath.Text = Path.GetFileName(pathToPublicKey);
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

        private void searchPublicKey_Click(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = false;
            pathToPublicKey = SelectFile();
            publicKeyPath.Text = Path.GetFileName(pathToPublicKey);
        }

        private void searchPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = false;
            pathToPrivateKey = SelectFile();
            privateKeyPath.Text = Path.GetFileName(pathToPrivateKey);
        }

        private void checkKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CryptoRSA cryptoService = new CryptoRSA();
                cryptoService.loadRSAFromPrivateKey(pathToPrivateKey);
                cryptoService.loadRSAFromPublicKey(pathToPublicKey);

                string checker = "CHECK_ME";
                string encryptedCheck = cryptoService.PublicEncrypt(checker, cryptoService.PublicRSA);
                string decryptedCheck = cryptoService.PrivateDecrypt(encryptedCheck, cryptoService.PrivateRSA);

                if (checker == decryptedCheck)
                {
                    MessageBox.Show("TEST_PASSED");
                    save.IsEnabled = true;
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

        private void save_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationHandler.SetValueOnKey("PATH_TO_PRIVATE_KEY", pathToPrivateKey);
            ConfigurationHandler.SetValueOnKey("PATH_TO_PUBLIC_KEY", pathToPublicKey);
            this.Close();
        }
    }
}
