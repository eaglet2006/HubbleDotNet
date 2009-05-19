using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Core.Global
{
    /// <summary>
    /// Global setting
    /// </summary>
    [Serializable, System.Xml.Serialization.XmlRoot(Namespace = "http://www.hubble.net")] 
    public class Setting
    {
        static private object _LockObj = new object();

        static private Setting _Config = null;

        static public Setting Config
        {
            get
            {
                lock (_LockObj)
                {
                    if (_Config == null)
                    {
                        Load();
                    }
                }

                return _Config;
            }

            set
            {
                _Config = value;
            }
        }

        const string FileName = "setting.xml";

        static public void Save()
        {
            string fileName = Path.AppendDivision(Path.ProcessDirectory, '\\') + FileName;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                 System.IO.FileAccess.ReadWrite))
            {
                XmlSerialization<Setting>.Serialize(Config, Encoding.UTF8, fs);
            }
        }

        static public void RemoveTableConfig(string tableDir)
        {
            Setting cfg = Config;
            lock (_LockObj)
            {
                cfg.Tables.Remove(new TableConfig(tableDir));
            }
        }


        static public void Load()
        {
            string fileName = Path.AppendDivision(Path.ProcessDirectory, '\\') + FileName;

            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read))
                    {
                        Config = XmlSerialization<Setting>.Deserialize(fs);
                    }
                }
                catch
                {
                    Config = new Setting();
                }
            }
            else
            {
                Config = new Setting();
            }
        }

        public Setting()
        {
            Directories = new Directories();
        }

        #region Public properties

        Directories _Directories;

        public Directories Directories
        {
            get
            {
                return _Directories;
            }

            set
            {
                _Directories = value;
            }
        }

        List<TableConfig> _Tables = new List<TableConfig>();

        public List<TableConfig> Tables
        {
            get
            {
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }


        #endregion
    }

    [Serializable]
    public class Directories
    {
        private string _LogDirectory;

        public string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_LogDirectory))
                {
                    return Path.AppendDivision(Path.ProcessDirectory, '\\') + @"Log\";
                }

                return Path.AppendDivision(System.IO.Path.GetFullPath(_LogDirectory), '\\');
            }

            set
            {
                _LogDirectory = value;
            }
        }

        public Directories()
        {
            
        }
    }

    [Serializable]
    public class TableConfig
    {
        private string _Directory;

        public string Directory
        {
            get
            {
                return Path.AppendDivision(System.IO.Path.GetFullPath(_Directory), '\\');
            }

            set
            {
                _Directory = value;
            }
        }

        public TableConfig()
        {
        }

        public TableConfig(string dir)
        {
            Directory = dir;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return this.Directory.Trim().ToLower() == ((TableConfig)obj).Directory.Trim().ToLower();
        }
    }
}
