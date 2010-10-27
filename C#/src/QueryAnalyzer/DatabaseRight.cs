using System;
using System.Collections.Generic;
using System.Text;

namespace QueryAnalyzer
{
    class DatabaseRight
    {
        public string DatabaseName;
        public int Right;

        public DatabaseRight()
        {
            DatabaseName = null;
            Right = 0;
        }

        public DatabaseRight(string databaseName, int right)
        {
            DatabaseName = databaseName;
            Right = right;
        }

        public override string ToString()
        {
            return this.DatabaseName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return ((DatabaseRight)obj).DatabaseName == this.DatabaseName;
        }

        public override int GetHashCode()
        {
            return this.DatabaseName.GetHashCode();
        }
    }
}
