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
using System.Net;
using SystemMessage;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SystemSecurity;

namespace Client
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        DiffieHellmanTunnel tunnel;
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.getValueFromKey("REGISTER_API") + "/");

        public RegisterWindow(DiffieHellmanTunnel tunnel)
        {
            InitializeComponent();
            this.tunnel = tunnel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SEND_Click(object sender, RoutedEventArgs e)
        {
            if(tunnel.Status == DiffieHellmanTunnelStatus.ESTABLISHED)
            {
                string toSend = loginTextBox.Text + "|" +
                                passwordBox.Password + "|" +
                                emailTextBox.Text;
                Message registrationMessage = new Message(ConnectionInfo.Sender, "REGISTER_ME", "NO_DESTINATION", tunnel.diffieEncrypt(toSend));
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.sendMessage(uriString, client, registrationMessage);
                    MessageBox.Show(reply);
                }

            }
            else
            {
                MessageBox.Show("Diffie Hellman Tunnel is not established.");
            }
        }
    }
}
