using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Store
{
    public class StoreException : Exception
    {
        public StoreException(string message)
            : base(message)
        {
        }
    }
}
