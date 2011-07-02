using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Hubble.SQLClient;

namespace QueryAnalyzer.BigTable
{
    public partial class FormBigTableBatchInsert : Form
    {
        private DialogResult _Result = DialogResult.Cancel;

        private Hubble.Core.BigTable.BigTable _TempBigTableInfo;

        private Hubble.Core.BigTable.BigTable _BigTableInfo;
        public Hubble.Core.BigTable.BigTable BigTableInfo
        {
            get
            {
                if (_BigTableInfo == null)
                {
                    _BigTableInfo = new Hubble.Core.BigTable.BigTable();
                }

                return _BigTableInfo;
            }

            set
            {
                _BigTableInfo = value;
                _TempBigTableInfo = _BigTableInfo.Clone();
            }
        }

        public FormBigTableBatchInsert()
        {
            InitializeComponent();
        }

        new public DialogResult ShowDialog()
        {
            comboBoxBalanceServers.Items.Clear();
            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in BigTableInfo.ServerList)
            {
                comboBoxBalanceServers.Items.Add(serverInfo);
            }

            base.ShowDialog();

            return _Result;
        }


        private bool ListTable(Hubble.Core.BigTable.ServerInfo serverInfo)
        {
            try
            {
                listBoxTable.Items.Clear();

                using (HubbleAsyncConnection conn = new HubbleAsyncConnection(serverInfo.ConnectionString))
                {
                    conn.Open();

                    
                    HubbleCommand command = new HubbleCommand("exec sp_tablelist", conn);
                    DataSet ds = command.Query();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (!bool.Parse(row["IsBigTable"].ToString()))
                        {
                            string fullName = row["TableName"].ToString();
                            int index = fullName.IndexOf(conn.Database, 0, StringComparison.CurrentCultureIgnoreCase);
                            if (index == 0)
                            {
                                index += conn.Database.Length;
                                listBoxTable.Items.Add(fullName.Substring(index + 1, fullName.Length - index - 1));
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                QAMessageBox.ShowErrorMessage(e);
                return false;
            }
        }

        private void comboBoxBalanceServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Hubble.Core.BigTable.ServerInfo serverInfo = comboBoxBalanceServers.SelectedItem as
                Hubble.Core.BigTable.ServerInfo;

            if (serverInfo != null)
            {
                if (!ListTable(serverInfo))
                {
                    comboBoxBalanceServers.SelectedIndex = -1;
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Hubble.Core.BigTable.ServerInfo serverInfo = comboBoxBalanceServers.SelectedItem as
                Hubble.Core.BigTable.ServerInfo;

                if (serverInfo != null)
                {
                    Hubble.Core.BigTable.ServerType serverType = radioButtonBalance.Checked ? Hubble.Core.BigTable.ServerType.Balance :
                        Hubble.Core.BigTable.ServerType.Failover;


                    foreach (string tableName in listBoxTable.SelectedItems)
                    {
                        _TempBigTableInfo.Add(tableName, serverType, serverInfo.ServerName);
                    }
                }

                _BigTableInfo.Tablets = _TempBigTableInfo.Tablets;
                _Result = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                QAMessageBox.ShowErrorMessage(ex);
                _TempBigTableInfo = _BigTableInfo.Clone();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
