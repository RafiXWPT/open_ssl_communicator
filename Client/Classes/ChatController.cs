using CommunicatorCore.Classes.Model;
using System.Collections.Generic;
using System.Windows;

namespace Client
{
    public static class ChatController
    {
        static List<ChatWindow> chatWindows = new List<ChatWindow>();
        static CryptoRSA RSA = new CryptoRSA();

        static ChatController()
        {
            RSA.loadRSAFromPrivateKey(Config.ConfigurationHandler.GetValueFromKey("PATH_TO_PRIVATE_KEY"));
        }

        public static void AddNewWindow(ChatWindow window)
        {
            chatWindows.Add(window);
        }

        public static void CloseWindow(ChatWindow window)
        {
            chatWindows.Remove(window);
        }

        public static void DeliverToChatWindow(ControlMessage message)
        {
            Message msg = new Message();
            string messageContent = RSA.PrivateDecrypt(message.MessageContent, RSA.PrivateRSA);
            msg.LoadJson(messageContent);
            DeliverSTA(msg);
        }

        static void DeliverSTA(Message msg)
        {
            Application.Current.Dispatcher.Invoke(() => deliverSTA(msg));
        }

        static void deliverSTA(Message msg)
        {
            ChatWindow targetWindow = chatWindows.Find(x => x.TargetID == msg.MessageSender);
            if (targetWindow == null)
            {
                ChatWindow window = new ChatWindow(msg.MessageSender);
                window.Show();
                window.DeliverMessage(msg);
            }
        }
    }
}
