using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private void FormLogin_Load(object sender, EventArgs e)
        {
            LoginInfos.Load();

            if (LoginInfos.Infos.ServerInfos.Count > 0)
            {
                comboBoxServerName.Text = LoginInfos.Infos.ServerInfos[0].ServerName;

                foreach (ServerInfo serverInfo in LoginInfos.Infos.ServerInfos)
                {
                    comboBoxServerName.Items.Add(serverInfo.ServerName);
                }
            }

            Hubble.SQLClient.DataCacheMgr.MaxMemorySize = 32 * 1024 * 1024; //32M Data cache
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            DbAccess dbAccess = new DbAccess();

            try
            {
                dbAccess.Connect(comboBoxServerName.Text.Trim());
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
                    serverInfo.LastLoginTime = DateTime.Now;
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                ServerInfo serverInfo = new ServerInfo();
                serverInfo.ServerName = comboBoxServerName.Text.Trim();
                serverInfo.LastLoginTime = DateTime.Now;

                LoginInfos.Infos.ServerInfos.Add(serverInfo);
            }

            LoginInfos.Save();

            FormMain frmMain = new FormMain();
            this.Hide();

            GlobalSetting.DataAccess = dbAccess;

            frmMain.ShowDialog();

            Environment.Exit(0);
        }
    }
}
