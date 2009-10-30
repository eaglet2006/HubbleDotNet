using System;
using System.Collections.Generic;
using System.Text;

namespace HubbleTask
{
    class Program
    {
        static string _LogFilePath;

        static private void WriteErrorMessage(string msg)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Error message at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", null));
                sb.AppendLine(msg);

                Console.WriteLine(sb.ToString());
                Hubble.Framework.IO.File.WriteLine(_LogFilePath, sb.ToString());
            }
            catch
            {
            }
        }

        static void Main(string[] args)
        {
            _LogFilePath = Hubble.Framework.IO.Path.AppendDivision(
                Hubble.Framework.IO.Path.ProcessDirectory, '\\') + "hubblesvr.log";
            
            string taskPath ;

            if (args.Length < 1)
            {
                WriteErrorMessage("args.Length == 0 and set path to current dir");

                taskPath = Hubble.Framework.IO.Path.ProcessDirectory;
            }
            else
            {
                taskPath = args[0];
            }

            try
            {
                Hubble.Core.Service.HubbleTask hubbleTask = new Hubble.Core.Service.HubbleTask(taskPath);

                Console.ReadKey();
            }
            catch (Exception e)
            {
                WriteErrorMessage(e.Message + e.StackTrace);
            }
        }
    }
}
