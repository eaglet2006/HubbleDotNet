using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.Parse
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException(string message)
            :base(message)
        {
        }
    }
}
