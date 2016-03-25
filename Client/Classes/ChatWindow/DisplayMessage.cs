using System;

namespace Client
{
    class DisplayMessage
    {
        public string UID { get; set; }
        public string UserName { get; }
        public string MessageContent { get; }
        public bool IsFromSelf { get; }
        public string TripStatus { get; set; }

        public DisplayMessage(string UID, string userName, string messageContent, bool isFromSelf)
        {
            this.UID = UID;
            UserName = "[" + DateTime.Now + "] " + userName + " says:";
            MessageContent = messageContent;
            IsFromSelf = isFromSelf;
        }
    }
}
