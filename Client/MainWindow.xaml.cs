using System;
using System.Threading;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectionChecker connectionChecker;
        private NetworkController networkController;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionChecker = new ConnectionChecker();
            networkController = new NetworkController();

            connectionChecker.startCheckConnection();

            Thread test = new Thread(testThread);
            test.Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow wnd = new RegisterWindow();
            wnd.ShowDialog();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow("http://localhost:11069/sendChatMessage/");
            chatWindow.Show();
        }

        void testThread()
        {
            
        }


    }
}
