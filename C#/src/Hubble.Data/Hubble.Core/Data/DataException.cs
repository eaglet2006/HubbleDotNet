using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    public class DataException : Exception
    {
        public DataException(string message)
            : base(message)
        {
        }
    }
}
