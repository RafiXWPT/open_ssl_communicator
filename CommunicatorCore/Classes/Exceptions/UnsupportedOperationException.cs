using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorCore.Classes.Exceptions
{
    public class UnsupportedOperationException: Exception
    {
        public UnsupportedOperationException()
        {
        }

        public UnsupportedOperationException(string message) : base(message)
        {
        }

        public UnsupportedOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
