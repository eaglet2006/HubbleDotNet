using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;

namespace Hubble.Core.Service
{
    public class ConnectionInformation 
    {
        private string _DatabaseName;

        internal string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
        }

        public ConnectionInformation(string databaseName)
        {
            _DatabaseName = databaseName;
        }
    }
}
