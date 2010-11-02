using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        readonly byte[] Key = { 10, 23, 74, 02, 75, 03, 48, 46 };

        private void ShowLoginInfo(int index)
        {
            ServerInfo serverInfo = LoginInfos.Infos.ServerInfos[index];
            comboBoxServerName.Text = serverInfo.ServerName;

            comboBoxAuthentication.SelectedIndex = (int)serverInfo.AuthType;

            if (serverInfo.AuthType == AuthenticationType.Hubble)
            {
                textBoxUserName.Text = serverInfo.UserName;
                
                if (!string.IsNullOrEmpty(serverInfo.Password))
                {
                    textBoxPassword.Text = Hubble.Framework.Security.DesEncryption.Decrypt(Key, serverInfo.Password);
                }
                else
                {
                    textBoxPassword.Text = "";
                }

                checkBoxRememberPassword.Checked = textBoxUserName.Text != "";
            }
            else
            {
                textBoxUserName.Text = "";
                textBoxPassword.Text = "";
                checkBoxRememberPassword.Checked = false;
            }

        }

        private void LoginInfoLoad()
        {
            LoginInfos.Load();
            
            comboBoxServerName.Items.Clear();

            if (LoginInfos.Infos.ServerInfos.Count > 0)
            {
                foreach (ServerInfo serverInfo in LoginInfos.Infos.ServerInfos)
                {
                    comboBoxServerName.Items.Add(serverInfo.ServerName);
                }

                ShowLoginInfo(0);
            }
            else
            {
                comboBoxServerName.Text = "";
            }

        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            comboBoxAuthentication.SelectedIndex = 0;
            panelAuthentication.Enabled = false;

            LoginInfoLoad();

            Hubble.SQLClient.DataCacheMgr.MaxMemorySize = 32 * 1024 * 1024; //32M Data cache
        }

        private void SetLoginInfo(ServerInfo serverInfo)
        {
            serverInfo.ServerName = comboBoxServerName.Text.Trim();
            serverInfo.LastLoginTime = DateTime.Now;
            serverInfo.AuthType = (AuthenticationType)comboBoxAuthentication.SelectedIndex;

            if (serverInfo.AuthType == AuthenticationType.Hubble)
            {
                if (checkBoxRememberPassword.Checked)
                {
                    serverInfo.UserName = textBoxUserName.Text.Trim();
                    serverInfo.Password = Hubble.Framework.Security.DesEncryption.Encrypt(Key, textBoxPassword.Text.Trim());
                }
                else
                {
                    serverInfo.UserName = "";
                    serverInfo.Password = null;
                }
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            DbAccess dbAccess = new DbAccess();

            try
            {
                if (comboBoxAuthentication.SelectedIndex == 0)
                {
                    dbAccess.Connect(comboBoxServerName.Text.Trim());
                }
                else
                {
                    dbAccess.Connect(comboBoxServerName.Text.Trim(),
                        textBoxUserName.Text.Trim(), textBoxPassword.Text.Trim());
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool find = false;

            for (int i = 0 ; i < LoginInfos.Infos.ServerInfos.Count; i++)
            {
                ServerInfo serverInfo = LoginInfos.Infos.ServerInfos[i];

                if (serverInfo.ServerName.Equals(comboBoxServerName.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    SetLoginInfo(serverInfo);
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                ServerInfo serverInfo = new ServerInfo();

                SetLoginInfo(serverInfo);

                LoginInfos.Infos.ServerInfos.Add(serverInfo);
            }

            LoginInfos.Save();

            FormMain frmMain = new FormMain();
            this.Hide();

            GlobalSetting.DataAccess = dbAccess;

            frmMain.ShowDialog();

            Environment.Exit(0);
        }

        private void comboBoxAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                panelAuthentication.Enabled = false;
            }
            else
            {
                panelAuthentication.Enabled = true;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            bool find = false;
            ServerInfo serverInfo = null;

            for (int i = 0; i < LoginInfos.Infos.ServerInfos.Count; i++)
            {
                serverInfo = LoginInfos.Infos.ServerInfos[i];

                if (serverInfo.ServerName.Equals(comboBoxServerName.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    serverInfo.LastLoginTime = DateTime.Now;
                    find = true;
                    break;
                }
            }

            if (find)
            {
                if (MessageBox.Show(string.Format("Are you sure you want to remove server {0}", 
                    serverInfo.ServerName), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                     DialogResult.Yes)
                {
                    LoginInfos.Infos.ServerInfos.Remove(serverInfo);
                    LoginInfos.Save();
                    LoginInfoLoad();

                }
            }
        }
    }
}
