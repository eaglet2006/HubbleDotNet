using System;
using System.Text;
using System.Collections.Generic;
using Hubble.Framework.Threading;

namespace TestFramework.Cases
{
    /// <summary>
    /// Summary description for TestMessageQueue
    /// </summary>
    class TestMessageQueue : TestCaseBase
    {
        public TestMessageQueue()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public override void Test()
        {
            TestMethodMessageQueue();
        }

        private object OnMessage(int evt, MessageQueue.MessageFlag flag, object data)
        {
            Console.Write(string.Format("Event:{0} ",evt));

            switch (evt)
            {
                case 1: //Normal async message
                    Console.WriteLine(data);
                    break;

                case 2: //Normal urgent async message
                    Console.WriteLine(data);
                    break;

                case 3: //Normal urgent sync message
                    Console.WriteLine(data);
                    return string.Format("Hi {0} {1}", evt, data);

                case 4: //Normal urgent sync message
                    Console.WriteLine(data);
                    return string.Format("Hi {0} {1}", evt, data);

                case 5: //Normal urgent sync message
                    Console.WriteLine(data);
                    throw new Exception(string.Format("Process data {0} raise exception", data));

                case 6: //sync message time out
                    Console.WriteLine(data);
                    System.Threading.Thread.Sleep(2000);
                    break;
                case 7: //urgent sync message time out
                    Console.WriteLine(data);
                    System.Threading.Thread.Sleep(2000);
                    break;

            }

            return null;
        }

        private void OnError(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }

        public void TestMethodMessageQueue()
        {
            const int MessageCount = 10;

            MessageQueue msgQueue = new MessageQueue(OnMessage);
            msgQueue.OnErrorEvent = OnError;
            msgQueue.Start();

            System.Threading.Thread t1 = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    delegate()
                    {
                        for (int i = 0; i < MessageCount; i++)
                        {
                            try
                            {
                                msgQueue.ASendMessage(1, i);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                    ));
            t1.Start();

            System.Threading.Thread t2 = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    delegate()
                    {
                        for (int i = 0; i < MessageCount; i++)
                        {
                            try
                            {
                                msgQueue.ASendUrgentMessage(2, i);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                    ));
            t2.Start();

            System.Threading.Thread t3 = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    delegate()
                    {
                        for (int i = 0; i < MessageCount; i++)
                        {
                            try
                            {
                                Console.WriteLine(string.Format("Receive {0}", msgQueue.SSendMessage(3, i, 100000)));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                    ));
            t3.Start();

            System.Threading.Thread t4 = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    delegate()
                    {
                        for (int i = 0; i < MessageCount; i++)
                        {
                            try
                            {
                                Console.WriteLine(string.Format("Receive {0}", msgQueue.SSendUrgentMessage(4, i, 1000)));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                    ));
            t4.Start();

            msgQueue.ASendMessage(5, 1000);

            //try
            //{
            //    msgQueue.SSendMessage(5, 1001, 1000);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(string.Format("SSend 5, 1001, \r\nexception msg:\r\n{0} \r\nstack\r\n {1} \r\ninnerStack\r\n {2}",
            //        e.Message, e.StackTrace, e.InnerException.StackTrace));
            //}

            //System.Threading.Thread.Sleep(5000);

            //try
            //{
            //    msgQueue.SSendMessage(6, 1002, 1000);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(string.Format("SSend 6, 1002, \r\nexception msg:\r\n{0} \r\nstack\r\n {1}",
            //        e.Message, e.StackTrace));
            //}

            //GC.Collect();

            //try
            //{
            //    msgQueue.SSendUrgentMessage(7, 1003, 1000);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(string.Format("SSend 7, 1003, \r\nexception msg:\r\n{0} \r\nstack\r\n {1}",
            //        e.Message, e.StackTrace));
            //}


            //try
            //{
            //    msgQueue.SSendUrgentMessage(7, 1003, 1000);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(string.Format("SSend 7, 1003, \r\nexception msg:\r\n{0} \r\nstack\r\n {1}",
            //        e.Message, e.StackTrace));
            //}

            //System.Threading.Thread.Sleep(5000);
            if (msgQueue.Close(1000))
            {
                Console.WriteLine("Close successfully");
            }
            else
            {
                Console.WriteLine("Close fail");
            }

            GC.Collect();
        }
    }
}
