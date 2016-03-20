using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using Server.Classes.DbAccess;

namespace Server
{
    class UserControll
    {
        private static UserControll instance;
        public static UserControll Instance
        {
            get { return instance; }
        }

        private readonly UsersDao _usersDao;

        ISet<User> users = new HashSet<User>();

        public UserControll()
        {
            instance = this;
            _usersDao = new UsersDao();
            LoadUsersFromDatabase();
        }

        void LoadUsersFromDatabase()
        {
            users = new HashSet<User>(_usersDao.LoadUsers());
        }

        public void AddUserToDatabase(UserPasswordData userPasswordData)
        {
            _usersDao.InsertUser(userPasswordData);

        }

        public void AddTemporaryUserToApplication(User user)
        {
            //DODAWANIE TYLKO DO LOKALNEGO OBIEKTU
            users.Add(user);
        }

        public User GetUserFromApplication(string username)
        {
            return users.ToList().Find(user => user.Name == username);
        }

        public bool CheckIsUserExist(string username)
        {
            return _usersDao.IsUserExist(username);
        }

        public bool IsUserValid(UserPasswordData userPassword)
        {
            return _usersDao.ValidateUserCredentials(userPassword);
        }

//        Do we really need to copy tunnel value?
        public void AddUserToApplication(string tempUserName, string loggedUserName)
        {
            User tempUser = users.ToList().Find(user => user.Name == tempUserName);
            User newUser = new User(loggedUserName, tempUser.Tunnel);
            users.Add(newUser);
        }
        
    }
}
