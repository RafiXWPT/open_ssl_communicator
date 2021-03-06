﻿using CommunicatorCore.Classes.Model;
using System.Collections.Generic;
using System.Windows;

namespace Client
{
    public static class ChatController
    {
        static List<ChatWindow> chatWindows = new List<ChatWindow>();

        public static void AddNewWindow(ChatWindow window)
        {
            chatWindows.Add(window);
        }

        public static void CloseWindow(ChatWindow window)
        {
            chatWindows.Remove(window);
        }

        public static void CloseWindows()
        {
            foreach(ChatWindow window in chatWindows)
            {
                window.Close();
            }
        }

        public static void DeliverToChatWindow(ControlMessage message)
        {
            Message msg = new Message();
            string messageContent = CryptoRSAService.CryptoService.PrivateDecrypt(message.MessageContent);
            msg.LoadJson(messageContent);
            DeliverSTA(msg);
        }

        static void DeliverSTA(Message msg)
        {
            Application.Current.Dispatcher.Invoke(() => deliverSTA(msg));
        }

        static void deliverSTA(Message msg)
        {
            ChatWindow targetWindow = chatWindows.Find(x => x.TargetId == msg.MessageSender);
            if (targetWindow == null)
            {
                targetWindow = new ChatWindow(msg.MessageSender);
            }

            targetWindow.Show();
            targetWindow.UpdateMessageStatus(msg);
        }
    }
}
