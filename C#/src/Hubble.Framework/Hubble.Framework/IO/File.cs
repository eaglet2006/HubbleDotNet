using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.IO
{
    public class File
    {
        /// <summary>
        /// Get file length
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public long GetFileLength(string fileName)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            return fileInfo.Length;
        }
    }
}
