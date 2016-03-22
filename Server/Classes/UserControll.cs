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

        private ISet<User> users = new HashSet<User>();

        public UserControll()
        {
            instance = this;
            _usersDao = new UsersDao();
            
            clearingUsers.Elapsed += ClearUsersInApplication;
            clearingUsers.AutoReset = true;
            clearingUsers.Enabled = true;
        }

        public int GetUsersOnline()
        {
            return users.Count;
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

        public bool CheckIfUserExist(string username)
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
                //Console.WriteLine("Curr Name: " + obj.Name + " Curr Time: " + obj.LastConCheck().ToShortDateString() + " is Temporary: " + obj.IsTemporary);
                if((obj.IsTemporary && (DateTime.Now - obj.LastConCheck()).TotalMinutes > 3))
                {
                    ServerLogger.LogMessage("Clearing User: " + obj.Name);
                    users.Remove(obj);
                }
            });

            ServerLogger.LogMessage("Cleaning compleet. Total Users logged in: " + users.Count);
        }

        
    }
}
