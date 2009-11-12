using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Global
{
    [Serializable]
    public class ReEntryException : Exception
    {
        public ReEntryException(string message)
            : base(message)
        {
        }
    }
}
