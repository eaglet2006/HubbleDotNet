using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Hubble.Framework.IO;

namespace FrameworkUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestLinkedSegmentFileStream
    {
        public TestLinkedSegmentFileStream()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        #region Private methods
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

        private void TestWrite(System.IO.FileStream fs, int segmentSize, int reserveSegment, int autoIncBytes,
            int segment, int positionInSeg, int lastReserveSegment)
        {
            byte[] _CurrentSegment = new byte[segmentSize];

            LinkedSegmentFileStream _SegmentFielStream = new LinkedSegmentFileStream(fs, segmentSize, autoIncBytes,
                lastReserveSegment);

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
                        Assert.AreEqual(_CurrentSegment[j], buf[j]);
                        //AssignEquals(_CurrentSegment[j], buf[j], "Test read");
                    }
                }

            } while (relCount > 0);

            Assert.AreEqual(times, 10);
            //AssignEquals(times, 10, "Test times");

        }

        void FillBuf(LinkedSegmentFileStream stream, byte[] buf)
        {
            int len = 0;
            int offset = 0;
            do
            {
                len = stream.Read(buf, offset, buf.Length - len);
                offset += len;

            } while (len > 0);
        }

        void ClearBuf(byte[] buf)
        {
            Array.Clear(buf, 0, buf.Length);
        }

        private void CompareBuf(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(src[srcOffset + i], dest[destOffset + i]);
            }
        }

        private void CompareBuf(byte[] src, byte[] dest, int count)
        {
            CompareBuf(src, 0, dest, 0, count);
        }

        private void CompareBuf(byte[] src, byte[] dest)
        {
            Assert.AreEqual(src.Length, dest.Length);
            CompareBuf(src, 0, dest, 0, src.Length);
        }


        #endregion

        //[TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic	here
            //
            using (System.IO.FileStream fs = InitFile("test.db", 2048, 3))
            {
                TestWrite(fs, 2048, 32, 1 * 1024 * 1024, 1, 0, 2);
            }

            if (System.IO.File.Exists("test.db"))
            {
                System.IO.File.Delete("test.db");
            }
        }

        [TestMethod]
        public void TestReserveSegment()
        {
            byte[] writeBuf = {
                0,4,7,3,
                8,5,2,1,
                0,4,0,5,
                34,56,12,0,
                2,5,7,11,
                0,4,8,13,
                45,6,2,1,
                0,2,4,5,
                              };

            byte[] readBuf = new byte[writeBuf.Length]; 
            int segmentSize = 8;
            int lastReserveSegment = 3;


            //Test both boundary in one segment
            using (System.IO.FileStream fs = InitFile("test.db", segmentSize, lastReserveSegment + 1))
            {
                LinkedSegmentFileStream stream = new LinkedSegmentFileStream(fs, segmentSize,
                    segmentSize * 2, lastReserveSegment);

                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 4);

                stream.Seek(1, 0);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf, 4);

                Assert.AreEqual(stream.CurSegment, 1);
                Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);
            }

            ClearBuf(readBuf);

            //Test end boundary in one segment
            using (System.IO.FileStream fs = InitFile("test.db", segmentSize, lastReserveSegment + 1))
            {
                LinkedSegmentFileStream stream = new LinkedSegmentFileStream(fs, segmentSize,
                    segmentSize * 2, lastReserveSegment);

                stream.Seek(1, 4);
                stream.Write(writeBuf, 0, 4);

                stream.Seek(1, 4);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf, 4);

                Assert.AreEqual(stream.CurSegment, 2);
                Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);

                ClearBuf(readBuf);
                stream.Seek(2, 0);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf, 4);

                Assert.AreEqual(stream.CurSegment, 2);
                Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);
            }

            ClearBuf(readBuf);

            //Test over boundary but some segment that is not reserve segment has been used already
            using (System.IO.FileStream fs = InitFile("test.db", segmentSize, lastReserveSegment + 1))
            {
                LinkedSegmentFileStream stream = new LinkedSegmentFileStream(fs, segmentSize,
                    segmentSize * 2, lastReserveSegment);

                int newSegment = stream.AllocSegment();

                Assert.AreEqual(newSegment, lastReserveSegment + 1);


                stream.Seek(newSegment, 0);
                stream.Write(writeBuf, 0, 8);

                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, writeBuf.Length);

                stream.Seek(1, 0);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf);

                stream.Seek(newSegment, 0);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf, 8);

            
            }

            ClearBuf(readBuf);

            //Test over boundary 
            using (System.IO.FileStream fs = InitFile("test.db", segmentSize, lastReserveSegment + 1))
            {
                LinkedSegmentFileStream stream = new LinkedSegmentFileStream(fs, segmentSize,
                    segmentSize * 2, lastReserveSegment);

                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, writeBuf.Length);
                
                stream.Seek(1, 0);
                FillBuf(stream, readBuf);

                CompareBuf(writeBuf, readBuf);

                if (readBuf.Length % (segmentSize - 4) == 0)
                {
                    Assert.AreEqual(stream.CurSegment, readBuf.Length / (segmentSize - 4));
                    Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);
                }
                else
                {
                    Assert.AreEqual(stream.CurSegment, readBuf.Length / (segmentSize - 4) + 1);
                    Assert.AreEqual(stream.CurPositionInSegment, readBuf.Length % (segmentSize - 4));
                }
            }

            if (System.IO.File.Exists("test.db"))
            {
                System.IO.File.Delete("test.db");
            }
        }
    }
}
