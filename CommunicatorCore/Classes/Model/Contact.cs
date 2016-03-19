using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Model
{
    public class Contact
    {
        public readonly string From;
        public readonly string To;

        public Contact(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}
