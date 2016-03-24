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
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SystemSecurity;
using CommunicatorCore.Classes.Model;
using Config;

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

        private readonly System.Windows.Threading.DispatcherTimer checkingEmailPreTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer checkingPasswordPreTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer checkingEmailTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer checkingPasswordTimer = new System.Windows.Threading.DispatcherTimer();

        private readonly BitmapImage okeyImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\" + "okey.png", UriKind.RelativeOrAbsolute));
        private readonly BitmapImage badImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\" + "bad.png", UriKind.RelativeOrAbsolute));

        private bool isEmailValid = false;
        private bool isPasswordValid = false;

        public RegisterWindow(DiffieHellmanTunnel tunnel, DiffieHellmanTunnelCreator creator)
        {
            InitializeComponent();
            this.tunnel = tunnel;
            this.creator = creator;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkingEmailPreTimer.Tick += emailBoxPreTimer_Tick;
            checkingPasswordPreTimer.Tick += passwordBoxPreTimer_Tick;
            checkingEmailTimer.Tick += emailBoxTimer_Tick;
            checkingPasswordTimer.Tick += passwordBoxTimer_Tick;

            passwordBox.IsEnabled = false;
            confirmPasswordBox.IsEnabled = false;
            sendBtn.IsEnabled = false;
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
                UserPasswordData userPasswordData = new UserPasswordData(emailTextBox.Text, passwordBox.Password);
                string toSend = userPasswordData.GetJsonString();
                ControlMessage registrationMessage = new ControlMessage(ConnectionInfo.Sender, "REGISTER_ME", toSend,  tunnel.DiffieEncrypt(toSend));
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    string reply = NetworkController.Instance.SendMessage(uriString, client, registrationMessage);
                    HandleRegisterResponse(reply);
                }
            }
            else
            {
                MessageBox.Show("Diffie Hellman Tunnel is not established. Establishing new one.");
                tunnel = creator.EstablishTunnel();
            }
        }

        private void HandleRegisterResponse(string reply)
        {
            ControlMessage returnedControlMessage = new ControlMessage();
            returnedControlMessage.LoadJson(reply);

            if (returnedControlMessage.MessageType == "REGISTER_INFO")
            {
                string messageContent = tunnel.DiffieDecrypt(returnedControlMessage.MessageContent);
                if (messageContent == "REGISTER_OK")
                {
                    MessageBox.Show("Registration Succesfull.");
                    this.Close();
                }
                else if (messageContent == "REGISTER_INVALID")
                {  
                    MessageBox.Show("Registration unsuccesfull. Use different username or password.");
                }
                else
                {
                }
            }
        }

        private void emailTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            emailImageValid.Visibility = Visibility.Hidden;
            sendBtn.IsEnabled = false;

            checkingEmailPreTimer.Start();
        }

        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            passwordImageValid.Visibility = Visibility.Hidden;
            sendBtn.IsEnabled = false;

            checkingPasswordPreTimer.Start();
        }

        private void emailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ResetEmailTimers();

            checkingEmail.Visibility = Visibility.Hidden;

            sendBtn.IsEnabled = false;
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            ResetPasswordTimers();

            checkingPassword.Visibility = Visibility.Hidden;

            sendBtn.IsEnabled = false;
        }

        void ResetEmailTimers()
        {
            checkingEmailPreTimer.Stop();
            checkingEmailPreTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
            checkingEmailTimer.Stop();
            checkingEmailTimer.Interval = new TimeSpan(0, 0, 0, 1, 500);
        }

        void ResetPasswordTimers()
        {
            checkingPasswordPreTimer.Stop();
            checkingPasswordPreTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
            checkingPasswordTimer.Stop();
            checkingPasswordTimer.Interval = new TimeSpan(0, 0, 0, 1, 500);
        }

        private void emailBoxPreTimer_Tick(object sender, EventArgs e)
        {
            if(emailTextBox.Text.Length > 0)
            {
                checkingEmail.Visibility = Visibility.Visible;
                checkingEmailTimer.Start();
            }
        }

        private void passwordBoxPreTimer_Tick(object sender, EventArgs e)
        {
            if(confirmPasswordBox.Password.Length > 0)
            {
                checkingPassword.Visibility = Visibility.Visible;
                checkingPasswordTimer.Start();
            }
        }

        private void emailBoxTimer_Tick(object sender, EventArgs e)
        {
            if (!(emailTextBox.Text.Length > 0))
            {
                return;
            }

            checkingEmail.Visibility = Visibility.Hidden;
            checkingEmailPreTimer.Stop();
            checkingEmailTimer.Stop();
            if(RegisterFormatValidator.IsEmailValid(emailTextBox.Text))
            {
                emailImageValid.Source = okeyImage;
                isEmailValid = true;
                passwordBox.IsEnabled = true;
                confirmPasswordBox.IsEnabled = true;
                if (isEmailValid && isPasswordValid)
                    sendBtn.IsEnabled = true;
            }
            else
            {
                passwordBox.IsEnabled = false;
                confirmPasswordBox.IsEnabled = false;
                passwordBox.Password = string.Empty;
                confirmPasswordBox.Password = string.Empty;
                emailImageValid.Source = badImage;
                passwordImageValid.Visibility = Visibility.Hidden;
            }
            emailImageValid.Visibility = Visibility.Visible;
        }

        private void passwordBoxTimer_Tick(object sender, EventArgs e)
        {
            if (!(confirmPasswordBox.Password.Length > 0))
            {
                return;
            }

            checkingPassword.Visibility = Visibility.Hidden;
            checkingPasswordPreTimer.Stop();
            checkingPasswordTimer.Stop();
            if (RegisterFormatValidator.IsPasswordValid(confirmPasswordBox.Password) && passwordBox.Password == confirmPasswordBox.Password)
            {
                passwordImageValid.Source = okeyImage;
                isPasswordValid = true;
                if (isEmailValid && isPasswordValid)
                    sendBtn.IsEnabled = true;
            }
            else
            {
                passwordImageValid.Source = badImage;
            }
            passwordImageValid.Visibility = Visibility.Visible;
        }
    }
}
