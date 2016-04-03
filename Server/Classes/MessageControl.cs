using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using Server.Classes.DbAccess;

namespace Server
{
    class MessageControl
    {
        private static MessageControl instance;
        public static MessageControl Instance
        {
            get { return instance; }
        }

        private readonly MessageDao _messageDao;

        public MessageControl()
        {
            instance = this;
            _messageDao = new MessageDao();            
        }

        public void InsertMessages(List<Message> messages)
        {
            _messageDao.InsertMessages(messages);
        }

        public List<Message> GetMessages(Contact contact)
        {
            return _messageDao.GetMessages(contact);
        }
    }
}
