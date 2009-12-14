using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Core.Store
{
    class DocumentPositionListSimpleSerialization
    {
        static public void Serialize(Stream stream, Entity.DocumentPositionList docPositionList)
        {
            byte[] docIdBuf = BitConverter.GetBytes(docPositionList.DocumentId);

            int zeroCount = 0;

            for (int i = 7; i>= 0; i--)
            {
                if (docIdBuf[i] == 0)
                {
                    zeroCount++;
                }
                else
                {
                    break;
                }
            }

            //head
            //0-3 Count
            //4 flag, if count >= 16, flag = 1
            //5-7 zero count
            byte head = (byte)(zeroCount << 5);

            if (docPositionList.Count < 16)
            {
                head |= (byte)docPositionList.Count;
                stream.WriteByte(head);
            }
            else
            {
                if (docPositionList.Count >= 4096) //2^12
                {
                    docPositionList.Count = 4095;
                }

                int count = docPositionList.Count;

                head |= (byte)(count & 0x0000000F);
                head |= 0x10;

                count >>= 4;

                stream.WriteByte(head);
                stream.WriteByte((byte)(count & 0x000000FF));
            }

            stream.Write(docIdBuf, 0, 8 - zeroCount);
        }

        static public Entity.DocumentPositionList Derialize(Stream stream)
        {
            byte head = (byte)stream.ReadByte();

            if (head == 0)
            {
                return null;
            }

            int zeroCount = head >> 5;

            int count;

            if ((head & 0x10) != 0)
            {
                count = stream.ReadByte();
                count <<= 4;
                count += head & 0x0F;
            }
            else
            {
                count = head & 0x0F;
            }

            byte[] docIdBuf = new byte[8];

            long docid;

            if (zeroCount == 0)
            {
                docid = 0;
            }
            else
            {
                stream.Read(docIdBuf, 0, 8 - zeroCount);
                docid = BitConverter.ToInt64(docIdBuf, 0);
            }

            return new Hubble.Core.Entity.DocumentPositionList(count, docid, 5);
        }
    }
}
