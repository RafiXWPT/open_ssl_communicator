using System.Windows;
using System.IO;
using CommunicatorCore.Classes.Model;

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
            if (result == true)
            {
                return dlg.FileName;
            }
            else
            {
                return string.Empty;
            }
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
            CryptoRSA cryptoService = new CryptoRSA(pathToPublicKey, pathToPrivateKey);
            string checker = "CHECK_ME";
            string encryptedCheck = cryptoService.Encrypt(checker);
            string decryptedCheck = cryptoService.Decrypt(encryptedCheck);

            if(checker == decryptedCheck)
            {
                MessageBox.Show("TEST_PASSED");
                save.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("TEST_FAILED");
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
