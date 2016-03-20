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
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SystemSecurity;
using CommunicatorCore.Classes.Model;

namespace Client
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        DiffieHellmanTunnel tunnel;
        DiffieHellmanTunnelCreator creator;
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("REGISTER_API") + "/");

        public RegisterWindow(DiffieHellmanTunnel tunnel, DiffieHellmanTunnelCreator creator)
        {
            InitializeComponent();
            this.tunnel = tunnel;
            this.creator = creator;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SEND_Click(object sender, RoutedEventArgs e)
        {
            if(!ConnectionInfo.Connected)
            {
                MessageBox.Show("Server is unavailable.");
                return;
            }

            if(creator.isTunnelActive(tunnel))
            {
                // We should validate input somehow
                UserPasswordData userPasswordData = new UserPasswordData(emailTextBox.Text, passwordBox.Password);
                string toSend = userPasswordData.GetJsonString();
                ControlMessage registrationMessage = new ControlMessage(ConnectionInfo.Sender, "REGISTER_ME", tunnel.DiffieEncrypt(toSend));
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(uriString, client, registrationMessage);
                    MessageBox.Show(reply);
                }
            }
            else
            {
                MessageBox.Show("Diffie Hellman Tunnel is not established. Establishing new one.");
                tunnel = creator.EstablishTunnel();
            }
        }
    }
}
