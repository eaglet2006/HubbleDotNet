using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.IO
{
    public class Stream
    {
        static public void ReadToBuf(System.IO.Stream s, byte[] buf, int offset, int count)
        {
            int read_offset = offset;

            int len = s.Read(buf, read_offset, count - (read_offset - offset));

            read_offset += len;

            while (read_offset < offset + count && len > 0)
            {
                len = s.Read(buf, read_offset, count - (read_offset - offset));
                read_offset += len;
            }

        }
    }
}
