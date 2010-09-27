using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Right
{
    public struct DatabaseRight
    {
        public string DatabaseName;
        public RightItem Right;

        public DatabaseRight(string databaseName, RightItem right)
        {
            DatabaseName = databaseName;
            Right = right;
        }
    }

    public class UserRight
    {
        public string UserName;
        public byte[] PasswordHash;
        public DatabaseRight ServerRangeRight;
        public List<DatabaseRight> DatabaseRights;
    }
}
