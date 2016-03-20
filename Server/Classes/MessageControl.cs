using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using Server.Classes.DbAccess;

namespace Server.Classes
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

        public void InsertMessage(Message message)
        {
            _messageDao.InsertMessage(message);
        }

        public List<Message> GetMessages(Contact contact)
        {
            return _messageDao.GetMessages(contact);
        }

 
    }
}
