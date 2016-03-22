using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffieHellman;
using OpenSSL.Crypto;

namespace Server
{
    public enum UserStatus
    {
        Offline,
        AFK,
        Online
    }

    public class User
    {
        public string Name { get; }
        public bool IsTemporary { get; }
        string Address { get; set; }
        DateTime LastConnectionCheck { get; set; }
        UserStatus Status { get; set; }

        // DIFFIE HELLMAN PART
        public DiffieHellmanTunnel Tunnel { get; }
        //public CryptoKey PublicKey { get; }
        //

        public User(string name)
        {
            Name = name;
            IsTemporary = true;
            Tunnel = new DiffieHellmanTunnel();
        }

        public User(string name, bool isTemporary)
        {
            Name = name;
            IsTemporary = isTemporary;
        }

        public void UpdateAddress(string address)
        {
            Address = address;
        }

        public void UpdateLastConnectionCheck(DateTime time)
        {
            LastConnectionCheck = time;
        }

        public void UpdateStatus(UserStatus status)
        {
            Status = status;
        }

        public DateTime LastConCheck()
        {
            return LastConnectionCheck;
        }

        public bool IsActive()
        {
            return DateTime.Now.AddMinutes(-10).CompareTo(LastConnectionCheck) > 0;
        }


     // Risky methods?
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            User other = (User) obj;
            return Name.Equals(other.Name);
        }

        
        public override int GetHashCode()
        {
            return 31 + Name.GetHashCode();
        }
    }
}
