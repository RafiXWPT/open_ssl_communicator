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
using DiffieHellman;
using SystemSecurity;
using System.Threading;
using CommunicatorCore.Classes.Model;
using WpfAnimatedGif;


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

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            if(CheckConnectionStatus())
            {
                RegisterWindow registerWindow = new RegisterWindow(tunnel, tunnelCreator);
                registerWindow.Show();
            }
        }

        private void confirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckConnectionStatus())
            {
                UserPasswordData userPasswordData = new UserPasswordData(loginBox.Text, passwordBox.Password);
                string toSend = userPasswordData.GetJsonString();
                ControlMessage loginMessage = new ControlMessage(ConnectionInfo.Sender, "LOG_IN", tunnel.DiffieEncrypt(toSend));
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
            MessageBox.Show(tunnel.DiffieDecrypt(returnedControlMessage.MessageContent));
            string messageType = returnedControlMessage.MessageType;
            if (messageType == "REGISTER_OK")
            {
                // We should dispose of this window
            }
            else if (messageType == "REGISTER_INVALID")
            {
                // As below i think   
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
    }
}
