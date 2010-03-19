using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_Configure : StoredProcedure, IStoredProc
    {
        private void ShowValues()
        {
            AddColumn("Name");
            AddColumn("Value");
            AddColumn("Note");

            OutputValue("Name", "MemoryLimited");
            OutputValue("Value", (Global.Setting.Config.MemoryLimited / (1024*1024)).ToString());
            OutputValue("Note", "MB, Minimun is 1 MB");

            NewRow();
            OutputValue("Name", "QueryCacheMemoryLimited");
            OutputValue("Value", (Global.Setting.Config.QueryCacheMemoryLimited / (1024 * 1024)).ToString());
            OutputValue("Note", "MB, Minimun is 1 MB");

            NewRow();
            OutputValue("Name", "InitTablesStartup");
            OutputValue("Value", Global.Setting.Config.InitTablesStartup.ToString());
            OutputValue("Note", "Initialize all the tables after startup");

            NewRow();
            OutputValue("Name", "LogDirectory");
            OutputValue("Value", Global.Setting.Config.Directories.LogDirectory);
            OutputValue("Note", "Dicrectory of log file in server");


        }

        private void ShowValues(string name)
        {
            AddColumn("Name");
            AddColumn("Value");
            AddColumn("Note");

            switch (name.ToLower())
            {
                case "memorylimited":
                    OutputValue("Name", "MemoryLimited");
                    OutputValue("Value", (Global.Setting.Config.MemoryLimited / (1024 * 1024)).ToString());
                    OutputValue("Note", "MB, Minimun is 1 MB");
                    break;

                case "querycachememorylimited":
                    OutputValue("Name", "QueryCacheMemoryLimited");
                    OutputValue("Value", (Global.Setting.Config.QueryCacheMemoryLimited / (1024 * 1024)).ToString());
                    OutputValue("Note", "MB, Minimun is 1 MB");
                    break;

                case "inittablesstartup":
                    OutputValue("Name", "InitTablesStartup");
                    OutputValue("Value", Global.Setting.Config.InitTablesStartup);
                    OutputValue("Note", "Initialize all the tables after startup");
                    break;


                case "logdirectory":
                    OutputValue("Name", "LogDirectory");
                    OutputValue("Value", Global.Setting.Config.Directories.LogDirectory);
                    OutputValue("Note", "Dicrectory of log file in server");
                    break;

                default:
                    RemoveTable();
                    throw new StoredProcException(string.Format("The configuration option '{0}' does not exist, or it may be an advanced option.",
                        name));
            }
        }

        private void SetValue(string name, string value)
        {
            string oldValue;

            switch (name.ToLower())
            {
                case "memorylimited":
                    {
                        oldValue = (Global.Setting.Config.MemoryLimited / (1024 * 1024)).ToString();
                        long memoryLimited;

                        if (!long.TryParse(value, out memoryLimited))
                        {
                            throw new StoredProcException("Error converting data type varchar to int.");
                        }

                        Global.Setting.Config.MemoryLimited = memoryLimited * 1024 * 1024;

                        Global.Setting.Save();

                        OutputMessage(string.Format("Configuration option '{0}' changed from {1} to {2}.",
                            name, oldValue, (Global.Setting.Config.MemoryLimited / (1024 * 1024)).ToString()));
                    }
                    break;
                case "querycachememorylimited":
                    {
                        oldValue = (Global.Setting.Config.QueryCacheMemoryLimited / (1024 * 1024)).ToString();
                        long memoryLimited;

                        if (!long.TryParse(value, out memoryLimited))
                        {
                            throw new StoredProcException("Error converting data type varchar to int.");
                        }

                        Global.Setting.Config.QueryCacheMemoryLimited = memoryLimited * 1024 * 1024;
                        Cache.QueryCacheManager.Manager.MaxMemorySize = Global.Setting.Config.QueryCacheMemoryLimited;

                        Global.Setting.Save();

                        OutputMessage(string.Format("Configuration option '{0}' changed from {1} to {2}.",
                            name, oldValue, (Global.Setting.Config.QueryCacheMemoryLimited / (1024 * 1024)).ToString()));
                    }

                    break;
                case "inittablesstartup":

                    oldValue = Global.Setting.Config.InitTablesStartup.ToString();

                    Global.Setting.Config.InitTablesStartup = bool.Parse(value);

                    Global.Setting.Save();

                    OutputMessage(string.Format("Configuration option '{0}' changed from {1} to {2}.",
                        name, oldValue, value));
                    break;

                case "logdirectory":
                    oldValue = Global.Setting.Config.Directories.LogDirectory;

                    if (!System.IO.Directory.Exists(value))
                    {
                        throw new StoredProcException("Directory does not exist.");
                    }

                    Global.Setting.Config.Directories.LogDirectory = value;

                    Global.Setting.Save();

                    OutputMessage(string.Format("Configuration option '{0}' changed from {1} to {2}.",
                        name, oldValue, Global.Setting.Config.Directories.LogDirectory));

                    break;

                default:
                    throw new StoredProcException(string.Format("The configuration option '{0}' does not exist, or it may be an advanced option.",
                        name));
            }
        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_Configure";
            }
        }

        public void Run()
        {
            if (Parameters.Count == 0)
            {
                ShowValues();
            }
            else if (Parameters.Count == 1)
            {
                ShowValues(Parameters[0]);
            }
            else if (Parameters.Count == 2)
            {
                SetValue(Parameters[0], Parameters[1]);
            }
            else
            {
                throw new StoredProcException("The number of parameters is more then 2.");
            }

        }

        #endregion
    }
}
