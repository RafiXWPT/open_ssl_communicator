using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class RegisterFormatValidator
    {
        static bool IsEmailSyntaxValid(string email)
        {
            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        static bool IsEmailServerValid(string email)
        {
            string emailServer = email.Split('@')[1];
            return emailServer == "protonmail.com";
        }

        public static bool IsEmailValid(string email)
        {
            if (!IsEmailSyntaxValid(email))
                return false;

            if (!IsEmailServerValid(email))
                return false;

            return true;
        }

        public static bool IsPasswordValid(string password)
        {
            if (password.Length < 8)
                return false;

            HashSet<char> specialCharacters = new HashSet<char>() { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+', '=', '.', ',', '/', '\\', '<', '>', '?', '|' };
            if ( password.Any(char.IsLower) &&
                 password.Any(char.IsUpper) &&
                 password.Any(char.IsDigit) &&
                 password.Any(specialCharacters.Contains))
            {
                return true;
            }
            return false;
        }
    }
}
