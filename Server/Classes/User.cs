using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes.Model;
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
        public UserStatus Status { get; set; }

        // DIFFIE HELLMAN PART
        public DiffieHellmanTunnel Tunnel { get; }
        public RSA FromPublicKey { get; }
        //

        public User(string name, bool isTemporary = true, RSA publicKey = null)
        {
            Name = name;
            Tunnel = new DiffieHellmanTunnel();
            IsTemporary = isTemporary;
            FromPublicKey = publicKey;
        }

        public void UpdateAddress(string address)
        {
            Address = address;
        }

        public string GetAddress()
        {
            return Address;
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
