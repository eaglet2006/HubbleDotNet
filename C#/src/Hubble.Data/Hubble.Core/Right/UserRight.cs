using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Right
{
    [Serializable]
    public class DatabaseRight
    {
        public string DatabaseName;
        public RightItem Right;

        public DatabaseRight(string databaseName, RightItem right)
        {
            DatabaseName = databaseName;
            Right = right;
        }
    }

    [Serializable]
    public class UserRight
    {
        public string UserName;
        public byte[] PasswordHash;
        public DatabaseRight ServerRangeRight;
        public List<DatabaseRight> DatabaseRights;
    }
}
