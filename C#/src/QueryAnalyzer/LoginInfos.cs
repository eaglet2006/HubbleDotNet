using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace QueryAnalyzer
{
    public class ServerInfo : IComparable<ServerInfo>
    {
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public byte[] Password { get; set; }
        public DateTime LastLoginTime { get; set; }

        public ServerInfo()
        {
            ServerName = "";
            UserName = "";
            Password = null;
        }

        #region IComparable<ServerInfo> Members

        public int CompareTo(ServerInfo other)
        {
            return other.LastLoginTime.CompareTo(this.LastLoginTime);
        }

        #endregion
    }

    public class LoginInfos
    {
        [System.Xml.Serialization.XmlIgnore]
        static public LoginInfos Infos { get; set; }

        private const string FileName = "LoginInfos.xml";

        public List<ServerInfo> ServerInfos { get; set; }

        public LoginInfos()
        {
            ServerInfos = new List<ServerInfo>();
        }

        static public void Load(string path)
        {
            string fileName = Path.AppendDivision(path, '\\') + FileName;

            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read))
                    {
                        Infos = XmlSerialization<LoginInfos>.Deserialize(fs);
                    }
                }
                catch
                {
                    Infos = new LoginInfos();
                }
            }
            else
            {
                Infos = new LoginInfos();
            }
        }


        static public void Load()
        {
            Load(Path.ProcessDirectory);
        }

        static public void Save()
        {
            Infos.ServerInfos.Sort();

            string fileName = Path.AppendDivision(Path.ProcessDirectory, '\\') + FileName;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                 System.IO.FileAccess.ReadWrite))
            {
                XmlSerialization<LoginInfos>.Serialize(Infos, Encoding.UTF8, fs);
            }
        }

    }
}
