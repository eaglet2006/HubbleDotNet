using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace HubbleService
{
    public partial class HubbleSvr : ServiceBase
    {
        public HubbleSvr()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Process hubbleTask = new Process();

            string defaultDir = TaskInformation.GetInstanceDir("Default");

            hubbleTask.StartInfo.FileName = defaultDir + "HubbleTask.exe";
            hubbleTask.StartInfo.Arguments = "\"" + defaultDir + "\"";
            hubbleTask.StartInfo.UseShellExecute = false;
            hubbleTask.StartInfo.RedirectStandardInput = false;
            hubbleTask.StartInfo.RedirectStandardOutput = false;
            hubbleTask.StartInfo.RedirectStandardError = false;
            hubbleTask.StartInfo.CreateNoWindow = true;
            hubbleTask.StartInfo.WorkingDirectory = defaultDir;

            hubbleTask.Start();
        }

        protected override void OnStop()
        {
            //Process defaultProcess = null;

            string defaultDir = TaskInformation.GetInstanceDir("Default");


            {
                Process[] processes = Process.GetProcessesByName("HubbleTask");

                string dir = defaultDir;

                if (processes.Length <= 0)
                {
                    return;
                }

                //foreach (Process p in processes)
                //{
                //    string targetDir = System.IO.Path.GetDirectoryName(p.MainModule.FileName);

                //    targetDir = Hubble.Framework.IO.Path.AppendDivision(targetDir, '\\');

                //    if (targetDir.Equals(dir, StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        defaultProcess = p;
                //        break;
                //    }
                //}

                //if (defaultProcess == null)
                //{
                //    return;
                //}
            }

            try
            {

                Hubble.Framework.Net.TcpClient tcpClient = new Hubble.Framework.Net.TcpClient();
                tcpClient.Port = TaskInformation.GetTcpPort("Default");
                tcpClient.SendSyncMessage(1001, TaskInformation.GetStartTime("Default"));
                tcpClient.Close();
            }
            catch(Exception e)
            {
                System.Threading.Thread.Sleep(5000);

                try
                {
                    Hubble.Framework.IO.File.WriteLine(defaultDir + "hubblesvr.log",
                        string.Format("Close hubble.net fail! err:{0}", e.Message));
                }
                catch
                {
                }

                Process[] processes = Process.GetProcessesByName("HubbleTask");

                foreach (Process p in processes)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch(Exception e1)
                    {
                        Hubble.Framework.IO.File.WriteLine(defaultDir + "hubblesvr.log",
                            string.Format("Kill fail hubble.net fail! err:{0}", e1.Message));
                    }
                }
            }

            int timeout = 600;
            while (timeout-- > 0)
            {
                System.Threading.Thread.Sleep(1000);

                Process[] processes = Process.GetProcessesByName("HubbleTask");

                string dir = defaultDir;
                bool stillRunning = false;

                if (processes.Length <= 0)
                {
                    return;
                }

                //foreach (Process p in processes)
                //{
                //    string targetDir = System.IO.Path.GetDirectoryName(p.MainModule.FileName);

                //    targetDir = Hubble.Framework.IO.Path.AppendDivision(targetDir, '\\');

                //    if (targetDir.Equals(dir, StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        stillRunning = true;
                //        break;
                //    }
                //}

                //if (!stillRunning)
                //{
                //    return;
                //}

                try
                {
                    if ((timeout % 10) == 0)
                    {
                        Hubble.Framework.IO.File.WriteLine(defaultDir + "hubblesvr.log",
                            string.Format("Watting hubble task exit. Process count:{0}", processes.Length));
                    }
                }
                catch
                {
                }

            }

            throw new Exception("Stop hubble.net service timeout!");
        }
    }
}
