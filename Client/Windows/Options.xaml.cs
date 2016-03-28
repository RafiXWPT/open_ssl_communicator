using System;
using System.Collections.Generic;
using System.Linq;
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
using OptionsWindow;

namespace Client
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            LoadUserSettings();
        }

        void LoadUserSettings()
        {
            IncomingMessageCheckBox.IsChecked = LoadOptions.IsIncomingMessageSoundEnabled();
            OutcomingMessageCheckBox.IsChecked = LoadOptions.IsOutcomingMessageSoundEnabled();
            FlashWindowCheckBox.IsChecked = LoadOptions.IsBlinkChatEnabled();
        }

        private void SaveOptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveOptions.Save(IncomingMessageCheckBox.IsChecked.Value, OutcomingMessageCheckBox.IsChecked.Value, FlashWindowCheckBox.IsChecked.Value);
            this.Close();
        }
    }
}
