using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Exceptions
{
    public class UnsuccessfulQueryException: Exception
    {
        public UnsuccessfulQueryException()
        {
        }

        public UnsuccessfulQueryException(string message): base(message)
        {
        }

        public UnsuccessfulQueryException(string message, Exception inner): base(message, inner)
        {
        }
    }
}
