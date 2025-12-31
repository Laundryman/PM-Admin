using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dplo_system.Exceptions
{
    public class DuplicatePartException : Exception
    {
        public DuplicatePartException()
        {
        }

        public DuplicatePartException(string message)
            : base(message)
        {
        }

        public DuplicatePartException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}