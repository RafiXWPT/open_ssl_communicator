using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserControll
    {
        private static UserControll instance;
        public static UserControll Instance
        {
            get { return instance; }
        }

        List<User> users = new List<User>();
        // NIBYDATABASE!!
        User user1 = new User("1", "Rafał Palej");
        User user2 = new User("2", "Mateusz Flis");
        User user3 = new User("3", "Szymon Dąbrowski");
        //

        public UserControll()
        {
            instance = this;
            LoadUsersFromDatabase();
        }

        void LoadUsersFromDatabase()
        {
            // NIBYLADOWANIE
            users.Add(user1);
            users.Add(user2);
            users.Add(user3);
        }

        public void AddUserToDatabase(User user)
        {
            //DODAWANIE DO BAZY
            users.Add(user);
        }

        public void AddTemporaryUserToDatabase(User user)
        {
            //DODAWANIE TYLKO DO LOKALNEGO OBIEKTU
            users.Add(user);
        }

        public User GetUserFromDatabase(string id)
        {
            return users.Find(user => user.ID == id);
        }
    }
}
