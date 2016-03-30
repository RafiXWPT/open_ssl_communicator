using System;

namespace Client
{
    public class DisplayMessage
    {
        public string UID { get; set; }
        public string UserName { get; }
        public string MessageContent { get; }
        public string MessageStatus { get; set; }
        public bool IsFromSelf { get; }

        public DisplayMessage(string UID, string userName, string messageContent, bool isFromSelf)
        {
            this.UID = UID;
            UserName = "[" + DateTime.Now + "] " + userName + " says:";
            MessageContent = messageContent;
            IsFromSelf = isFromSelf;
            UpdateMessageStatus("SENDED");
        }

        public void UpdateMessageStatus(string status)
        {
            string uri = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\";
            if(status == "SENDED")
            {
                uri += "chatImageSended.png";
            }
            else if (status == "SENDED_ACK")
            {
                uri += "chatImageSendedAck.png";
            }
            else if (status == "DELIVERED")
            {
                uri += "chatImageDelivered.png";
            }

            MessageStatus = new Uri(uri, UriKind.RelativeOrAbsolute).ToString();
        }

    }
}
