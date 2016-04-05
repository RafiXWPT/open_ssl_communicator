using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Timer _clearingUsers = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);

        private readonly ISet<User> _users = new HashSet<User>();

        public UserControll()
        {
            instance = this;
            _usersDao = new UsersDao();
            
            _clearingUsers.Elapsed += ClearUsersInApplication;
            _clearingUsers.AutoReset = true;
            _clearingUsers.Enabled = true;
        }

        public int GetUsersOnline()
        {
            return _users.Count;
        }

        public UserStatus GetUserStatus(string username)
        {
            User user = new User(username, false);
            if (_users.Contains(user))
            {
                return _users.First(u => u.Equals(user)).Status;
            }
            return UserStatus.Offline;
        }

        public bool IsTokenValid(UserTokenDto userTokenDto)
        {
            return _usersDao.ValidateUserToken(userTokenDto);
        }

        public void AddUserToDatabase(UserPasswordData userPasswordData, string token)
        {
            _usersDao.InsertUser(userPasswordData, token);
        }

        public void AddTemporaryUserToApplication(User user)
        {
            _users.Add(user);
        }

        public User GetUserFromApplication(string username)
        {
            return _users.ToList().Find(user => user.Name == username);
        }

        public bool CheckIfUserExist(string username)
        {
            return _usersDao.IsUserExist(username);
        }

        public void UpdateUser(UserPasswordData user)
        {
            _usersDao.UpdatePassword(user);
        }

        public bool IsUserValid(UserPasswordData userPassword)
        {
            return _usersDao.ValidateUserCredentials(userPassword);
        }

        public void AddUserToApplication(string tempUserName, string loggedUserName)
        {
            User newUser = new User(loggedUserName, false);
            newUser.UpdateStatus(UserStatus.Offline);
            _users.Add(newUser);
        }

        void ClearUsersInApplication(object source, ElapsedEventArgs e)
        {
            ServerLogger.LogMessage("Cleaning Users...");
            Parallel.ForEach(_users, (obj) =>
            {
                if((obj.IsTemporary && (DateTime.Now - obj.LastConCheck()).TotalMinutes > 1) || (!obj.IsTemporary && (DateTime.Now - obj.LastConCheck()).TotalSeconds > 20))
                {
                    ServerLogger.LogMessage("Clearing User: " + obj.Name + " because of idle time.");
                    _users.Remove(obj);
                }
            });

            ServerLogger.LogMessage("Cleaning compleet. Total Users in system after clean: " + _users.Count);
        }

    }
}
