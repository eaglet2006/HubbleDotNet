using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;

namespace TestFramework.Cases
{
    class TestLinkedSegmentFileStream : TestCaseBase
    {
        private System.IO.FileStream InitFile(string fileName, int segmentSize, int reserverSegment)
        {
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.CreateNew,
                 System.IO.FileAccess.ReadWrite);

            fs.SetLength(segmentSize * reserverSegment);

            return fs;
        }

        private void TestWrite(string fileName, int segmentSize, int reserveSegment, int autoIncBytes,
            int segment, int positionInSeg, int lastReserveSegment)
        {
            byte[] _CurrentSegment = new byte[segmentSize];

            using (LinkedSegmentFileStream _SegmentFielStream = new LinkedSegmentFileStream(fileName, true, segmentSize, autoIncBytes,
                lastReserveSegment))
            {

                for (int i = 0; i < segmentSize; i++)
                {
                    _CurrentSegment[i] = (byte)i;
                }

                for (int i = 0; i < 10; i++)
                {
                    _SegmentFielStream.Write(_CurrentSegment, 0, _CurrentSegment.Length);
                }

                byte[] buf = new byte[segmentSize];

                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                int relCount = 0;
                _SegmentFielStream.Seek(1, 0);

                do
                {
                    relCount = _SegmentFielStream.Read(buf, 0, buf.Length);

                    if (relCount > 0)
                    {
                        ms.Write(buf, 0, relCount);
                    }

                } while (relCount > 0);

                ms.Position = 0;

                int times = 0;

                do
                {
                    relCount = ms.Read(buf, 0, buf.Length);

                    if (relCount > 0)
                    {
                        times++;

                        for (int j = 0; j < buf.Length; j++)
                        {
                            AssignEquals(_CurrentSegment[j], buf[j], "Test read");
                        }
                    }

                } while (relCount > 0);

                AssignEquals(times, 10, "Test times");
            }

            if (System.IO.File.Exists("test.db"))
            {
                System.IO.File.Delete("test.db");
            }
        }

        public override void Test()
        {
            TestWrite("test.db", 2048, 32, 1 * 1024 * 1024, 1, 0, 2);
        }
    }
}
