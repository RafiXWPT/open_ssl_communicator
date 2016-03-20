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
using DiffieHellman;
using SystemSecurity;
using System.Threading;

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
