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
using SystemSecurity;
using System.Threading;
using CommunicatorCore.Classes.Model;
using WpfAnimatedGif;
using Config;


namespace Client
{
    /// <summary>
    /// Interaction logic for LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        private DiffieHellmanTunnel tunnel = new DiffieHellmanTunnel();
        private DiffieHellmanTunnelCreator tunnelCreator = new DiffieHellmanTunnelCreator();
        private ConnectionChecker connectionChecker;
        private NetworkController networkController;
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("LOGIN_API") + "/");

        public LogInWindow()
        {
            InitializeComponent();
        }

        void EstablishTunnelThread()
        {
            while (tunnel.Status != DiffieHellmanTunnelStatus.ESTABLISHED)
            {
                while (!ConnectionInfo.Connected)
                    Thread.Sleep(1000 * 1);

                tunnel = tunnelCreator.EstablishTunnel();

                if (tunnel.Status != DiffieHellmanTunnelStatus.ESTABLISHED)
                    Thread.Sleep(5 * 1000);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionChecker = new ConnectionChecker();
            networkController = new NetworkController();

            connectionChecker.StartCheckConnection();

            EstablishTunnel();
        }

        private void EstablishTunnel()
        {
            Thread tunnelCreator = new Thread(EstablishTunnelThread);
            tunnelCreator.IsBackground = true;
            tunnelCreator.Start();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if(CheckConnectionStatus())
            {
                RegisterWindow registerWindow = new RegisterWindow(tunnel, tunnelCreator);
                registerWindow.Show();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if(LoginBox.Text.Length == 0)
            {
                MessageBox.Show("Type your login first.");
                return;
            }

            if(PasswordBox.Password.Length == 0)
            {
                MessageBox.Show("Type your password first.");
                return;
            }

            if (CheckConnectionStatus())
            {
                UserPasswordData userPasswordData = new UserPasswordData(LoginBox.Text, PasswordBox.Password);
                string toSend = userPasswordData.GetJsonString();
                ControlMessage loginMessage = new ControlMessage(ConnectionInfo.Sender, "LOG_IN", toSend, tunnel.DiffieEncrypt(toSend));
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(uriString, client, loginMessage);
                    HandleLogInResponse(reply);
                }
            }
        }

        private void HandleLogInResponse(string reply)
        {
            ControlMessage returnedControlMessage = new ControlMessage();
            returnedControlMessage.LoadJson(reply);

            string messageType = returnedControlMessage.MessageType;
            if (messageType == "LOGIN_SUCCESFULL")
            {
                ConnectionInfo.Sender = tunnel.DiffieDecrypt(returnedControlMessage.MessageContent);
                MainWindow window = new MainWindow(connectionChecker, networkController, ConnectionInfo.Sender);
                window.Show();
                this.Close();
            }
            else if (messageType == "LOGIN_UNSUCCESFULL")
            {
                MessageBox.Show("Username or password is not correct."); 
            }
            else
            {
                // We should encourage user to try use different username etc
            }
        }

        bool CheckConnectionStatus()
        {
            if (!ConnectionInfo.Connected)
            {
                MessageBox.Show("Server is unavailable.");
                return false;
            }

            if(!tunnelCreator.isTunnelActive(tunnel))
            {
                MessageBox.Show("Diffie Hellman Tunnel is not established. Establishing new one.");
                EstablishTunnel();
                return false;
            }

            return true;
        }

        private void GetEnter_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(null,null);
            }
        }

        private void ForgotPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckConnectionStatus())
            {
                ResetPasswordWindow resetPasswordWindow = new ResetPasswordWindow(tunnel, tunnelCreator);
                resetPasswordWindow.Show();
            }
        }
    }
}
