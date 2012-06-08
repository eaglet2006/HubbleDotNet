using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer.BigTable
{
    public partial class FormServerInfo : Form
    {
        private DialogResult _Result = DialogResult.Cancel;

        private Hubble.Core.BigTable.ServerInfo _ServerInfo;

        internal Hubble.Core.BigTable.ServerInfo ServerInfo
        {
            get
            {
                return _ServerInfo;
            }
        }

        public FormServerInfo()
        {
            InitializeComponent();
        }

        internal DialogResult ShowDialog(Hubble.Core.BigTable.ServerInfo serverInfo)
        {
            _ServerInfo = serverInfo.Clone();
            labelServerName.Text = _ServerInfo.ServerName;
            textBoxConnectionString.Text = _ServerInfo.ConnectionString;
            checkBoxEnabled.Checked = _ServerInfo.Enabled;
            textBoxConnectionString.Select();

            base.ShowDialog();

            return _Result;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _ServerInfo.ConnectionString = textBoxConnectionString.Text;
            _ServerInfo.Enabled = checkBoxEnabled.Checked;
            _Result = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
