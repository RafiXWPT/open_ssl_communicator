using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
using Server.Classes.DbAccess;
using System.Timers;

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
        private readonly System.Timers.Timer clearingUsers = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);

        ISet<User> users = new HashSet<User>();

        public UserControll()
        {
            instance = this;
            _usersDao = new UsersDao();
            LoadUsersFromDatabase();

            clearingUsers.Elapsed += ClearUsersInApplication;
            clearingUsers.AutoReset = true;
            clearingUsers.Enabled = true;
        }

        void LoadUsersFromDatabase()
        {
            users = new HashSet<User>(_usersDao.LoadUsers());
        }

        public int getUsersOnline()
        {
            return users.Count;
        }

        public void AddUserToDatabase(UserPasswordData userPasswordData)
        {
            _usersDao.InsertUser(userPasswordData);
        }

        public void AddTemporaryUserToApplication(User user)
        {
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

        public void AddUserToApplication(string tempUserName, string loggedUserName)
        {
            User newUser = new User(loggedUserName, false);
            users.Add(newUser);
        }

        void ClearUsersInApplication(object source, ElapsedEventArgs e)
        {
            ServerLogger.LogMessage("Cleaning Users...");
            Parallel.ForEach(users, (obj) =>
            {
                if((obj.IsTemporary && (DateTime.Now - obj.LastConCheck()).TotalMinutes > 1) || (!obj.IsTemporary && (DateTime.Now - obj.LastConCheck()).TotalMinutes > 3))
                {
                    ServerLogger.LogMessage("Clearing User: " + obj.Name + " because of idle time.");
                    users.Remove(obj);
                }
            });

            ServerLogger.LogMessage("Cleaning compleet. Total Users in system after clean: " + users.Count);
        }
    }
}
