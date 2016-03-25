using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class DisplayMessage
    {
        public string UserName { get; }
        public string MessageContent { get; }
        public bool IsFromSelf { get; }

        public DisplayMessage(string userName, string messageContent, bool isFromSelf)
        {
            UserName = "[" + DateTime.Now + "] " + userName + " says:";
            MessageContent = messageContent;
            IsFromSelf = isFromSelf;
        }
    }
}
