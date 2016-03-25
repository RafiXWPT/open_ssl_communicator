using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using Server.Classes.DbAccess;

namespace Server.Classes
{
    class ContactControl
    {
        private static ContactControl instance;
        public static ContactControl Instance
        {
            get { return instance; }
        }

        private readonly ContactsDao _contactDao;

        public ContactControl()
        {
            instance = this;
            _contactDao = new ContactsDao();
        }

        public void UpsertContact(Contact contact)
        {
            _contactDao.UpsertContact(contact);
        }

        public List<Contact> GetContacts(string contact)
        {
            return _contactDao.GetContacts(contact);
        }

        public bool CheckIfContactExist(Contact contact)
        {
            return _contactDao.CheckIfAlreadyExist(contact);
        }
    }
}
