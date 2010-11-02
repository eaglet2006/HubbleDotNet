using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace QueryAnalyzer
{
    public enum AuthenticationType
    {
        None = 0,
        Hubble = 1,
    }

    public class ServerInfo : IComparable<ServerInfo>
    {
        public string ServerName { get; set; }
        public AuthenticationType AuthType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime LastLoginTime { get; set; }

        public ServerInfo()
        {
            AuthType = AuthenticationType.None;
            ServerName = "";
            UserName = "";
            Password = "";
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

        public List<ServerInfo> ServerInfos { get; set; }

        public LoginInfos()
        {
            ServerInfos = new List<ServerInfo>();
        }

        static public void Load()
        {
            Microsoft.Win32.RegistryKey key;

            using (key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Hubble.net\QueryAnalyzer"))
            {
                if (key == null)
                {
                    Infos = new LoginInfos();
                    return;
                }

                object loginInfosObj;

                loginInfosObj = key.GetValue("LoginInfos", null);

                if (loginInfosObj == null)
                {
                    Infos = new LoginInfos();
                    return;
                }

                System.IO.MemoryStream m = new System.IO.MemoryStream((byte[])loginInfosObj);

                m.Position = 0;

                Infos = XmlSerialization<LoginInfos>.Deserialize(m);
            }
        }

        static public void Save()
        {
            Infos.ServerInfos.Sort();

            Microsoft.Win32.RegistryKey key;

            using (key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Hubble.net\QueryAnalyzer"))
            {

                if (key == null)
                {
                    throw new Exception("Create queryanzlyer registry fail!");
                }

                System.IO.MemoryStream m = new System.IO.MemoryStream();

                XmlSerialization<LoginInfos>.Serialize(Infos, Encoding.UTF8, m);

                m.Position = 0;

                key.SetValue("LoginInfos", m.ToArray());
            }
        }

    }
}
