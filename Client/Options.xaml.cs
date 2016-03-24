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
            inComingMessageCheckBox.IsChecked = LoadOptions.IsIncomingMessageSoundEnabled();
            outComingMessageCheckBox.IsChecked = LoadOptions.IsOutcomingMessageSoundEnabled();
            flashWindowCheckBox.IsChecked = LoadOptions.IsBlinkChatEnabled();
        }

        private void saveOptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveOptions.Save(inComingMessageCheckBox.IsChecked.Value, outComingMessageCheckBox.IsChecked.Value, flashWindowCheckBox.IsChecked.Value);
            this.Close();
        }
    }
}
