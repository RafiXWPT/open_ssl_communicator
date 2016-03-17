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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SystemMessage;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow("http://localhost:11069/connectionCheck/");
            chatWindow.Show();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow("http://localhost:11069/register/");
            chatWindow.Show();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow("http://localhost:11069/logIn/");
            chatWindow.Show();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow("http://localhost:11069/sendChatMessage/");
            chatWindow.Show();
        }
    }
}
