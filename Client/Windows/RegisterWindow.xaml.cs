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

        private readonly BitmapImage _okeyImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\" + "okey.png", UriKind.RelativeOrAbsolute));
        private readonly BitmapImage _badImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\" + "bad.png", UriKind.RelativeOrAbsolute));

        private bool _isEmailValid = false;
        private bool _isPasswordValid = false;

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

            PasswordBox.IsEnabled = false;
            ConfirmPasswordBox.IsEnabled = false;
            SendBtn.IsEnabled = false;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if(!ConnectionInfo.Connected)
            {
                MessageBox.Show("Server is unavailable.");
                return;
            }

            if(creator.isTunnelActive(tunnel))
            {
                UserPasswordData userPasswordData = new UserPasswordData(EmailTextBox.Text, PasswordBox.Password);
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
            else if (returnedControlMessage.MessageType == "INVALID")
            {
                MessageBox.Show("Exception occurred: " + returnedControlMessage.MessageContent);

            }
        }

        private void emailTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            EmailImageValid.Visibility = Visibility.Hidden;
            SendBtn.IsEnabled = false;

            checkingEmailPreTimer.Start();
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            PasswordImageValid.Visibility = Visibility.Hidden;
            SendBtn.IsEnabled = false;

            checkingPasswordPreTimer.Start();
        }

        private void EmailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ResetEmailTimers();

            CheckingEmail.Visibility = Visibility.Hidden;

            SendBtn.IsEnabled = false;
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            ResetPasswordTimers();

            CheckingPassword.Visibility = Visibility.Hidden;

            SendBtn.IsEnabled = false;
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
            if(EmailTextBox.Text.Length > 0)
            {
                CheckingEmail.Visibility = Visibility.Visible;
                checkingEmailTimer.Start();
            }
        }

        private void passwordBoxPreTimer_Tick(object sender, EventArgs e)
        {
            if(ConfirmPasswordBox.Password.Length > 0)
            {
                CheckingPassword.Visibility = Visibility.Visible;
                checkingPasswordTimer.Start();
            }
        }

        private void emailBoxTimer_Tick(object sender, EventArgs e)
        {
            if (!(EmailTextBox.Text.Length > 0))
            {
                return;
            }

            CheckingEmail.Visibility = Visibility.Hidden;
            checkingEmailPreTimer.Stop();
            checkingEmailTimer.Stop();
            if(RegisterFormatValidator.IsEmailValid(EmailTextBox.Text))
            {
                EmailImageValid.Source = _okeyImage;
                _isEmailValid = true;
                PasswordBox.IsEnabled = true;
                ConfirmPasswordBox.IsEnabled = true;
                if (_isEmailValid && _isPasswordValid)
                    SendBtn.IsEnabled = true;
            }
            else
            {
                PasswordBox.IsEnabled = false;
                ConfirmPasswordBox.IsEnabled = false;
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                EmailImageValid.Source = _badImage;
                PasswordImageValid.Visibility = Visibility.Hidden;
            }
            EmailImageValid.Visibility = Visibility.Visible;
        }

        private void passwordBoxTimer_Tick(object sender, EventArgs e)
        {
            if (!(ConfirmPasswordBox.Password.Length > 0))
            {
                return;
            }

            CheckingPassword.Visibility = Visibility.Hidden;
            checkingPasswordPreTimer.Stop();
            checkingPasswordTimer.Stop();
            if (RegisterFormatValidator.IsPasswordValid(ConfirmPasswordBox.Password) && PasswordBox.Password == ConfirmPasswordBox.Password)
            {
                PasswordImageValid.Source = _okeyImage;
                _isPasswordValid = true;
                if (_isEmailValid && _isPasswordValid)
                    SendBtn.IsEnabled = true;
            }
            else
            {
                PasswordImageValid.Source = _badImage;
            }
            PasswordImageValid.Visibility = Visibility.Visible;
        }
    }
}
