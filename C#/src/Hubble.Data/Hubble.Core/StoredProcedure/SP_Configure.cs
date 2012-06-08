using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_Configure : StoredProcedure, IStoredProc, IHelper
    {
        private void ShowValues()
        {
            AddColumn("Name");
            AddColumn("Value");
            AddColumn("Note");

            OutputValue("Name", "RamIndexMemoryLimited");
            OutputValue("Value", Global.Setting.Config.RamIndexMemoryLimited.ToString());
            OutputValue("Note", "MB, Minimun is 64 MB");
 
            NewRow();
            OutputValue("Name", "RamIndexAllocedMemory");
            OutputValue("Value", Hubble.Framework.IO.CachedFileBufferManager.GetAllocedMemorySize().ToString());
            OutputValue("Note", "MB");

            NewRow();
            OutputValue("Name", "LastSystemAvaliableMemory");
            OutputValue("Value", Hubble.Framework.IO.CachedFileBufferManager.GetAvaliableRam().ToString());
            OutputValue("Note", "MB");

            NewRow();
            OutputValue("Name", "QueryCacheMemoryLimited");
            OutputValue("Value", (Global.Setting.Config.QueryCacheMemoryLimited / (1024 * 1024)).ToString());
            OutputValue("Note", "MB, Minimun is 1 MB");

            NewRow();
            OutputValue("Name", "InitTablesStartup");
            OutputValue("Value", Global.Setting.Config.InitTablesStartup.ToString());
            OutputValue("Note", "Initialize all the tables after startup");

            NewRow();
            OutputValue("Name", "SqlTrace");
            OutputValue("Value", Global.Setting.Config.SqlTrace.ToString());
            OutputValue("Note", "Enable SqlTrace");

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
                case "ramindexmemorylimited":
                    OutputValue("Name", "RamIndexMemoryLimited");
                    OutputValue("Value", Global.Setting.Config.RamIndexMemoryLimited.ToString());
                    OutputValue("Note", "MB, Minimun is 64 MB");
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

                case "sqltrace":
                    OutputValue("Name", "SqlTrace");
                    OutputValue("Value", Global.Setting.Config.SqlTrace);
                    OutputValue("Note", "Enable SqlTrace");
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
                case "ramindexmemorylimited":
                    {
                        oldValue = Global.Setting.Config.RamIndexMemoryLimited.ToString();
                        int memoryLimited;

                        if (!int.TryParse(value, out memoryLimited))
                        {
                            throw new StoredProcException("Error converting data type varchar to int.");
                        }

                        Global.Setting.Config.RamIndexMemoryLimited = memoryLimited;

                        Hubble.Framework.IO.CachedFileBufferManager.SetMaxMemorySize(
                            Global.Setting.Config.RamIndexMemoryLimited);

                        Global.Setting.Save();

                        OutputMessage(string.Format("Configuration option '{0}' changed from {1} to {2}.",
                            name, oldValue, Global.Setting.Config.RamIndexMemoryLimited.ToString()));
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

                case "sqltrace":

                    oldValue = Global.Setting.Config.SqlTrace.ToString();

                    Global.Setting.Config.SqlTrace = bool.Parse(value);

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

        override public string Name
        {
            get
            {
                return "SP_Configure";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo("", Right.RightItem.ManageSystem);

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
                throw new StoredProcException("The number of parameters is more than 2.");
            }

        }

        #endregion

        #region IHelper Members

        string IHelper.Help
        {
            get 
            {
                return "Configure system.";
            }
        }

        #endregion
    }
}
