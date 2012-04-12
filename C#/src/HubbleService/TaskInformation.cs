using System;
using System.Collections.Generic;
using System.Text;

namespace HubbleService
{
    class TaskInformation
    {
        static internal string GetInstanceDir(string instanceName)
        {
            Microsoft.Win32.RegistryKey key;

            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Hubble.net\Instances");

            if (key == null)
            {
                throw new Exception("Hasn't instances key in registry");
            }

            string dir = (string)key.GetValue(instanceName);

            if (dir == null)
            {
                throw new Exception(string.Format("Hasn't instance name:{0} in registry", instanceName));
            }

            return Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

        }

        static internal int GetTcpPort(string instanceName)
        {
            Microsoft.Win32.RegistryKey key;

            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Hubble.net\Instances");

            if (key == null)
            {
                throw new Exception("Hasn't instances key in registry");
            }

            object obj = (object)key.GetValue(instanceName + "Port");

            if (obj == null)
            {
                return 7523;
            }

            return (int)obj;

        }

        static internal void SetTcpPort(string instanceName, int port)
        {
            Microsoft.Win32.RegistryKey key;

            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Hubble.net\Instances");

            if (key == null)
            {
                throw new Exception("Hasn't instances key in registry");
            }

            key.SetValue(instanceName + "Port", port);
        }

        static internal string GetStartTime(string instanceName)
        {
            Microsoft.Win32.RegistryKey key;

            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Hubble.net\Instances");

            if (key == null)
            {
                throw new Exception("Hasn't instances key in registry");
            }

            return (string)key.GetValue(instanceName + "StartTime");
        }    
    }
}
