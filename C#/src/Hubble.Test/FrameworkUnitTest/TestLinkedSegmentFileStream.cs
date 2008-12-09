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
                            Assert.AreEqual(_CurrentSegment[j], buf[j]);
                            //AssignEquals(_CurrentSegment[j], buf[j], "Test read");
                        }
                    }

                } while (relCount > 0);

                Assert.AreEqual(times, 10);
                //AssignEquals(times, 10, "Test times");
            }
        }

        int FillBuf(LinkedSegmentFileStream stream, byte[] buf)
        {
            int len = 0;
            int offset = 0;
            do
            {
                len = stream.Read(buf, offset, buf.Length - len);
                offset += len;

            } while (len > 0);

            return offset;
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

        #region Exception test


        //Test exceptions
        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void TestGetLastSegmentNumberFromExcetpion1()
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

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                int newSegment = stream.AllocSegment();

                stream.GetLastSegmentNumberFrom(4);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestGetLastSegmentNumberFromExcetpion2()
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

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                stream.GetLastSegmentNumberFrom(4);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestGetLastSegmentNumberFromExcetpion3()
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

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                stream.GetLastSegmentNumberFrom(-1);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestSeekException1()
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

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                stream.Seek(4);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestSeekException2()
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

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                stream.Seek(1, 5);
            }
        }

        
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestConstructorException1()
        {
            int segmentSize = 4;
            int lastReserveSegment = 3;

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestConstructorException2()
        {
            int segmentSize = 8;
            int lastReserveSegment = 3;

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, 4))
            {

            }

        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void TestZeroAutoIncrease1()
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

            int segmentSize = 8;
            int lastReserveSegment = 3;

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    0, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 13);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void TestZeroAutoIncrease2()
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

            int segmentSize = 8;
            int lastReserveSegment = 3;

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    0, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                stream.AllocSegment();
            }
        }

        #endregion

        #region Function test

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic	here
            //

            TestWrite("test.db", 2048, 32, 1 * 1024 * 1024, 1, 0, 2);

            if (System.IO.File.Exists("test.db"))
            {
                System.IO.File.Delete("test.db");
            }
        }


        [TestMethod]
        public void TestInit()
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

            //Test on the boundary of reserve
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                LinkedSegmentFileStream.SegmentPosition lastSegment = stream.GetLastSegmentNumberFrom(1);

                Assert.AreEqual(lastSegment.Segment, 3);
                Assert.AreEqual(lastSegment.PositionInSegment, 4);
                Assert.AreEqual(stream.AllocSegment(), 4);
            }

            //Test over the boundary of reserve
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 13);
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                LinkedSegmentFileStream.SegmentPosition lastSegment = stream.GetLastSegmentNumberFrom(1);

                Assert.AreEqual(lastSegment.Segment, 4);
                Assert.AreEqual(lastSegment.PositionInSegment, 1);

                Assert.AreEqual(stream.AllocSegment(), 5);
            }

            //Test less the boundary of reserve
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 8);
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                LinkedSegmentFileStream.SegmentPosition lastSegment = stream.GetLastSegmentNumberFrom(1);

                Assert.AreEqual(lastSegment.Segment, 2);
                Assert.AreEqual(lastSegment.PositionInSegment, 4);

                Assert.AreEqual(stream.AllocSegment(), 4);
            }

            if (System.IO.File.Exists("test.db"))
            {
                System.IO.File.Delete("test.db");
            }

            //Test on the boundary of reserve and has normal data segment
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                int newSegment = stream.AllocSegment();
                stream.Seek(newSegment);
                stream.Write(writeBuf, 0, 8);
                Assert.AreEqual(stream.LastSegment, 5); 
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 12);
                CompareBuf(writeBuf, readBuf, 12);

                stream.Seek(4, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 8);
                CompareBuf(writeBuf, readBuf, 8);

                LinkedSegmentFileStream.SegmentPosition lastSegment = stream.GetLastSegmentNumberFrom(1);
                Assert.AreEqual(lastSegment.Segment, 3);
                Assert.AreEqual(lastSegment.PositionInSegment, 4);

                lastSegment = stream.GetLastSegmentNumberFrom(4);
                Assert.AreEqual(lastSegment.Segment, 5);
                Assert.AreEqual(lastSegment.PositionInSegment, 4);

                Assert.AreEqual(stream.LastSegment, 5);
                Assert.AreEqual(stream.AllocSegment(), 6);
                Assert.AreEqual(stream.LastSegment, 7);
            }

            //Test on the boundary of reserve and has normal data segment
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, 12);
                int newSegment = stream.AllocSegment();
                Assert.AreEqual(newSegment, 4);
                Assert.AreEqual(stream.LastSegment, 5);
            }

            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 12);
                CompareBuf(writeBuf, readBuf, 12);

                stream.Seek(4, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 0);

                LinkedSegmentFileStream.SegmentPosition lastSegment = stream.GetLastSegmentNumberFrom(1);
                Assert.AreEqual(lastSegment.Segment, 3);
                Assert.AreEqual(lastSegment.PositionInSegment, 4);

                //lastSegment = stream.GetLastSegmentNumberFrom(4);
                //Assert.AreEqual(lastSegment, 0);
                Assert.AreEqual(stream.LastSegment, 5);

                int newSegment = stream.AllocSegment();
                Assert.AreEqual(newSegment, 4);

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
            LinkedSegmentFileStream lstream = new LinkedSegmentFileStream("test.db", segmentSize,
                    segmentSize * 2, lastReserveSegment);
            lstream.Seek(1, 0);
            lstream.Write(writeBuf, 0, 4);
            lstream.Flush();
            lstream.Seek(1, 0);
            Assert.AreEqual(FillBuf(lstream, readBuf), 4);

            CompareBuf(writeBuf, readBuf, 4);

            Assert.AreEqual(lstream.CurSegment, 1);
            Assert.AreEqual(lstream.CurPositionInSegment, segmentSize - 4);

            lstream = null;
            GC.Collect();

            ClearBuf(readBuf);

            //Test end boundary in one segment
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 4);
                stream.Write(writeBuf, 0, 4);

                stream.Seek(1, 4);
                Assert.AreEqual(FillBuf(stream, readBuf),4);

                CompareBuf(writeBuf, readBuf, 4);

                Assert.AreEqual(stream.CurSegment, 2);
                Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);

                ClearBuf(readBuf);
                stream.Seek(2, 0);
                Assert.AreEqual(FillBuf(stream, readBuf),4);

                CompareBuf(writeBuf, readBuf, 4);

                Assert.AreEqual(stream.CurSegment, 2);
                Assert.AreEqual(stream.CurPositionInSegment, segmentSize - 4);
            }

            ClearBuf(readBuf);

            //Test over boundary but some segment that is not reserve segment has been used already
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                int newSegment = stream.AllocSegment();

                Assert.AreEqual(newSegment, lastReserveSegment + 1);


                stream.Seek(newSegment, 0);
                stream.Write(writeBuf, 0, 8);

                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, writeBuf.Length);

                stream.Seek(1, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), writeBuf.Length);

                CompareBuf(writeBuf, readBuf);

                stream.Seek(newSegment, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 8);

                CompareBuf(writeBuf, readBuf, 8);

                newSegment = stream.AllocSegment();
                stream.Seek(newSegment, 0);
                stream.Write(writeBuf, 0, 9);
                stream.Seek(newSegment, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), 9);

                CompareBuf(writeBuf, readBuf, 9);

            }

            ClearBuf(readBuf);

            //Test over boundary 
            using (LinkedSegmentFileStream stream = new LinkedSegmentFileStream("test.db", true, segmentSize,
                    segmentSize * 2, lastReserveSegment))
            {
                stream.Seek(1, 0);
                stream.Write(writeBuf, 0, writeBuf.Length);
                
                stream.Seek(1, 0);
                Assert.AreEqual(FillBuf(stream, readBuf), writeBuf.Length);

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

        #endregion

    }
}
