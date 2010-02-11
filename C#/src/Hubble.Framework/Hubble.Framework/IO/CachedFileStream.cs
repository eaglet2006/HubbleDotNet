using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.IO
{
    public class CachedFileStream : BlockedMemoryStream
    {
        public CachedFileStream(System.IO.FileStream fs) 
            : base(128 * 1024)
        {
            byte[] buf = new byte[8192];

            fs.Seek(0, System.IO.SeekOrigin.Begin);

            int len = fs.Read(buf, 0, buf.Length);

            while (len > 0)
            {
                Write(buf, 0, len);

                len = fs.Read(buf, 0, buf.Length);
            }
        }
    }
}
