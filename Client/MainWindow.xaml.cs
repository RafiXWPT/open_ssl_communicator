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

        public MainWindow(ConnectionChecker connectionChecker, NetworkController networkController, string loggedUserName)
        {
            InitializeComponent();
            this.connectionChecker = connectionChecker;
            this.networkController = networkController;
            this.Title = "Crypto Talk - " + loggedUserName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Example how to add contacts to list
            //contactsData.Items.Add(new TestClass("matflis@protonmail.com", "Mateusz Flis"));
        }
    }
}