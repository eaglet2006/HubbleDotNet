using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Store
{
    class IndexFileStreamCache
    {
        static object _LockObj = new object();

        static Dictionary<string, System.IO.Stream> _Cache = new Dictionary<string,System.IO.Stream>();

        public static System.IO.Stream GetIndexFile(string filePath)
        {
            lock (_LockObj)
            {
                System.IO.Stream stream;
                if (_Cache.TryGetValue(filePath.ToLower(), out stream))
                {
                    return stream;
                }

                return null;
            }
        }

        public static void AddIndexFile(string filePath, System.IO.Stream stream)
        {
            lock (_LockObj)
            {
                _Cache.Add(filePath.ToLower(), stream);
            }
        }

        public static void RemoveIndexFile(string filePath)
        {
            lock (_LockObj)
            {
                _Cache.Remove(filePath.ToLower());
            }
        }
    }
}
