using System;
using SystemMessage;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public Uri talkingTo { get; }
        NameValueCollection headers = new NameValueCollection();
        NameValueCollection data = new NameValueCollection();

        public ChatWindow(string id)
        {
            talkingTo = new Uri(id);

            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sendMessage();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void sendMessage()
        {
            Message message = new Message("THATS ME", "CHAT_MESSAGE", "WHATEVER", chatText.Text);
            using (var wb = new WebClient())
            {
                wb.Proxy = null;

                headers["messageContent"] = message.getJsonString();
                wb.Headers.Add(headers);
                data["DateTime"] = DateTime.Now.ToShortDateString();
                byte[] response;

                response = wb.UploadValues(talkingTo, "POST", data);
                listBox.Items.Add(Encoding.UTF8.GetString(response));
                chatText.Text = string.Empty;
            }
        }
    }
}
