using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;

using System.Windows.Forms;
using Hubble.Core.BigTable;
using Hubble.SQLClient;

using ServerInfomation = Hubble.Core.BigTable.ServerInfo;

namespace QueryAnalyzer.BigTable
{
    public partial class BigTableGenerate : UserControl
    {
        bool _Init = true;
        FormBigTable _ParentForm;

        public string DefaultIndexFolder;

        public string DatabaseName;

        string _TableName;

        public string TableName
        {
            get
            {
                return textBoxTableName.Text;
            }

            set
            {
                _TableName = value;
            }
        }

        public string IndexFolder
        {
            get
            {
                return textBoxIndexFolder.Text;
            }

            set
            {
                textBoxIndexFolder.Text = value;
            }
        }

        bool _CreateTable;

        public bool CreateTable
        {
            get
            {
                return _CreateTable;
            }

            set
            {
                _CreateTable = value;
                textBoxTableName.Enabled = value;
                textBoxIndexFolder.Enabled = value;
                labelLastUpdateTime.Visible = !value;
            }
        }

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
            }
        }

        public BigTableGenerate()
        {
            InitializeComponent();
        }

        private void RefreshServerGUI()
        {
            listViewServers.Items.Clear();

            foreach (ServerInfomation serverInfo in BigTableInfo.ServerList)
            {
                ListViewItem item = new ListViewItem(new string[] {serverInfo.Enabled.ToString(), serverInfo.ServerName, serverInfo.ConnectionString });
                item.Name = "Enabled";
                item.SubItems[0].Name = "Enabled";
                item.SubItems[1].Name = "ServerName";
                item.SubItems[2].Name = "ConnectionString";

                listViewServers.Items.Add(item);
            }

            RefreshServersComboBox();
        }

        private void RefreshTabletGUI()
        {
            listBoxTablets.Items.Clear();

            foreach (TabletInfo tableinfo in BigTableInfo.Tablets)
            {
                listBoxTablets.Items.Add(tableinfo);
            }

            if (listBoxTablets.Items.Count > 0)
            {
                listBoxTablets.SelectedIndex = 0;
            }
        }

        private void RefreshGUI()
        {
            if (BigTableInfo != null)
            {
                labelLastUpdateTime.Text = string.Format("Last Update Time:{0}", 
                    BigTableInfo.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

                checkBoxKeepDataIntegrity.Checked = BigTableInfo.KeepDataIntegrity;
                numericUpDownExecuteTimeout.Value = BigTableInfo.ExecuteTimeout;
            }
            RefreshServerGUI();
            RefreshTabletGUI();
        }

        private void RefreshBalanceServerList(TabletInfo tablet)
        {
            listBoxBalanceServer.Items.Clear();
            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tablet.BalanceServers)
            {
                listBoxBalanceServer.Items.Add(serverInfo.ServerName);
            }
        }

        private void RefreshFailoverServerList(TabletInfo tablet)
        {
            listBoxFailoverServers.Items.Clear();
            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tablet.FailoverServers)
            {
                listBoxFailoverServers.Items.Add(serverInfo.ServerName);
            }
        }


        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
#if HubblePro
                FormBigTableBatchInsert frmBigtableBatchInsert = new FormBigTableBatchInsert();
                frmBigtableBatchInsert.BigTableInfo = this.BigTableInfo;
                if (frmBigtableBatchInsert.ShowDialog() == DialogResult.OK)
                {
                    RefreshTabletGUI();
                }
#else
                string tableName = null;
                string connectionString = "";

                if (QAMessageBox.ShowInputBox("Add tablet", "Input table name", ref tableName) == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(tableName))
                    {
                        QAMessageBox.ShowErrorMessage("table name can't be empty!");
                        return;
                    }

                    TabletInfo tablet = new TabletInfo(tableName, new Hubble.Core.BigTable.ServerInfo("", connectionString));
                    BigTableInfo.Add(tablet);
                    listBoxTablets.Items.Add(tablet);
                    listBoxTablets.SelectedItem = tablet;
                }

#endif
            }
            catch (Exception ex)
            {
                QAMessageBox.ShowErrorMessage(ex.Message);
            }
        }

        private void BigTableGenerate_Load(object sender, EventArgs e)
        {
            _ParentForm = this.Parent as FormBigTable;

            textBoxTableName.Text = _TableName;
            

#if HubblePro
            groupBoxBalanceServers.Enabled = true;
            panelServers.Enabled = true;
#else
            groupBoxBalanceServers.Enabled = false;
            panelServers.Enabled = false;
#endif
            listViewServers.GridLines = true;
            listViewServers.View = View.Details;
            listViewServers.LabelEdit = false;
            listViewServers.HeaderStyle = ColumnHeaderStyle.Clickable;
            listViewServers.FullRowSelect = true;

            ColumnHeader colHeader = new ColumnHeader();
            colHeader.Name = "Enabled";
            colHeader.Text = "Enabled";
            colHeader.Width = 100;
            listViewServers.Columns.Add(colHeader);

            colHeader = new ColumnHeader();
            colHeader.Name = "ServerName";
            colHeader.Text = "Server Name";
            colHeader.Width = 100;
            listViewServers.Columns.Add("ServerName", "Server Name", 100);

            colHeader = new ColumnHeader();
            colHeader.Name = "ConnectionString";
            colHeader.Text = "Connection String";
            colHeader.Width = 500;
            listViewServers.Columns.Add("ConnectionString", "Connection String", 500);

            if (!CreateTable)
            {
                RefreshGUI();
            }

            _Init = false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _ParentForm.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            IndexFolder = IndexFolder.Trim();
            TableName = TableName.Trim();
            if (IndexFolder == "")
            {
                QAMessageBox.ShowErrorMessage("Index folder can't be empty.");
                return;
            }

            if (TableName == "")
            {
                QAMessageBox.ShowErrorMessage("Table name can't be empty.");
                return;
            }

            if (BigTableInfo.Tablets.Count <= 0)
            {
                QAMessageBox.ShowErrorMessage("BigTable must have at least one tablet!");
                return;
            }

            foreach (TabletInfo tableInfo in BigTableInfo.Tablets)
            {
                if (tableInfo.BalanceServers.Count <= 0)
                {
                    QAMessageBox.ShowErrorMessage(string.Format("Table name:{0} must include at least one BalanceServer!",
                        tableInfo.TableName));
                    return;
                }
            }

            try
            {
                _ParentForm.TableName = this.TableName;
                _ParentForm.IndexFolder = this.IndexFolder;
                _ParentForm.BigTable = BigTableInfo;
                _ParentForm.BigTable.KeepDataIntegrity = checkBoxKeepDataIntegrity.Checked;
                _ParentForm.BigTable.ExecuteTimeout = (int)numericUpDownExecuteTimeout.Value;
                _ParentForm._Result = DialogResult.OK;
                _ParentForm.Close();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }  
        }

        private void textBoxTableName_TextChanged(object sender, EventArgs e)
        {
            if (_Init)
            {
                return;
            }

            if (textBoxIndexFolder.Text.IndexOf(DefaultIndexFolder, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                textBoxIndexFolder.Text = Hubble.Framework.IO.Path.AppendDivision(
                    Hubble.Framework.IO.Path.AppendDivision(DefaultIndexFolder, '\\') +
                    textBoxTableName.Text, '\\');

            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet != null)
            {
                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove tablet: {0} ?",
                   tablet.TableName)) == DialogResult.Yes)
                {
                    listBoxTablets.Items.Remove(tablet);
                    BigTableInfo.Tablets.Remove(tablet);

                    if (listBoxTablets.Items.Count > 0)
                    {
                        listBoxTablets.SelectedIndex = 0;
                    }
                }
            }
        }

        private void CheckServerList()
        {
            List<string> delServerInfos = new List<string>();

            foreach (string serverName in listBoxBalanceServer.Items)
            {
                if (!BigTableInfo.ServerList.Contains(new Hubble.Core.BigTable.ServerInfo(serverName, "")))
                {
                    delServerInfos.Add(serverName);
                }
            }

            foreach (string serverName in delServerInfos)
            {
                listBoxBalanceServer.Items.Remove(serverName);
            }

            delServerInfos.Clear();

            foreach (string serverName in listBoxFailoverServers.Items)
            {
                if (!BigTableInfo.ServerList.Contains(new Hubble.Core.BigTable.ServerInfo(serverName, "")))
                {
                    delServerInfos.Add(serverName);
                }
            }

            foreach (string serverName in delServerInfos)
            {
                listBoxFailoverServers.Items.Remove(serverName);
            }
        }

        private void RefreshServersComboBox()
        {
            comboBoxBalanceServers.Items.Clear();

            foreach (ServerInfomation serverInfo in BigTableInfo.ServerList)
            {
                comboBoxBalanceServers.Items.Add(serverInfo);
            }

            comboBoxFailoverServers.Items.Clear();

            foreach (ServerInfomation serverInfo in BigTableInfo.ServerList)
            {
                comboBoxFailoverServers.Items.Add(serverInfo);
            }

            if (BigTableInfo.ServerList.Count > 0)
            {
                comboBoxBalanceServers.Text = BigTableInfo.ServerList[0].ServerName;
                comboBoxFailoverServers.Text = BigTableInfo.ServerList[0].ServerName;
            }

        }

        private void buttonAddServer_Click(object sender, EventArgs e)
        {
            string serverName = "";
            string connectionString = "";
            if (QAMessageBox.ShowInputBox("Server name", "Please input server name", ref serverName) == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(serverName))
                {
                    QAMessageBox.ShowErrorMessage("server name can't be empty!");
                    return;
                }

                if (QAMessageBox.ShowInputBox("Server name", "Please input connection string", ref connectionString) == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        QAMessageBox.ShowErrorMessage("connection string can't be empty!");
                        return;
                    }

                    Hubble.Core.BigTable.ServerInfo serverInfo = 
                        new Hubble.Core.BigTable.ServerInfo(serverName,
                        connectionString);

                    if (BigTableInfo.ServerList.Contains(serverInfo))
                    {
                        QAMessageBox.ShowErrorMessage("Can't input reduplicate server name!");
                        return;
                    }

                    BigTableInfo.ServerList.Add(new Hubble.Core.BigTable.ServerInfo(serverName,
                        connectionString));

                    ListViewItem item = new ListViewItem(new string[] {serverName, connectionString});

                    listViewServers.Items.Add(item);

                    RefreshServersComboBox();
                }
            }
        }

        private void buttonDeleteServer_Click(object sender, EventArgs e)
        {
            if (listViewServers.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewServers.SelectedItems[0];

                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove server: {0} ?",
                    item.SubItems["ServerName"].Text)) == DialogResult.Yes)
                {
                    listViewServers.Items.Remove(item);

                    Hubble.Core.BigTable.ServerInfo serverInfo =
                                           new Hubble.Core.BigTable.ServerInfo(item.SubItems["ServerName"].Text,
                                           item.SubItems["ConnectionString"].Text);

                    BigTableInfo.RemoveServerInfo(serverInfo);

                    RefreshServersComboBox();
                    CheckServerList();
                }

            }
        }

        private void comboBoxServers_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void buttonAddBS_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            ServerInfomation serverInfo = comboBoxBalanceServers.SelectedItem as ServerInfomation;
            if (serverInfo == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a server name");
                return;
            }

            if (listBoxBalanceServer.Items.Contains(serverInfo))
            {
                QAMessageBox.ShowErrorMessage("Can't input reduplicate server name!");
                return;
            }

            tablet.BalanceServers.Add(serverInfo);
            listBoxBalanceServer.Items.Add(serverInfo);
        }

        private void buttonDeleteBS_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            string serverName = listBoxBalanceServer.SelectedItem as string;
            if (serverName != null)
            {
                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove server: {0} ?",
                   serverName)) == DialogResult.Yes)
                {
                    listBoxBalanceServer.Items.Remove(serverName);
                    tablet.BalanceServers.Remove(new Hubble.Core.BigTable.ServerInfo(serverName, ""));
                }
            }
        }

        private void buttonAddFailoverServers_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            ServerInfomation serverInfo = comboBoxFailoverServers.SelectedItem as ServerInfomation;
            if (serverInfo == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a server name");
                return;
            }

            if (listBoxFailoverServers.Items.Contains(serverInfo))
            {
                QAMessageBox.ShowErrorMessage("Can't input reduplicate server name!");
                return;
            }

            listBoxFailoverServers.Items.Add(serverInfo.ServerName);
            tablet.FailoverServers.Add(serverInfo);
        }

        private void buttonDelFailoverServers_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            string serverName = listBoxFailoverServers.SelectedItem as string;
            if (serverName != null)
            {
                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove server: {0} ?",
                   serverName)) == DialogResult.Yes)
                {
                    listBoxFailoverServers.Items.Remove(serverName);
                    tablet.FailoverServers.Remove(new Hubble.Core.BigTable.ServerInfo(serverName, ""));
                }
            }
        }

        private void listBoxTablets_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet != null)
            {
                RefreshBalanceServerList(tablet);
                RefreshFailoverServerList(tablet);
                //CheckServerList();
            }
        }

        private void buttonUpdateServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewServers.SelectedItems.Count > 0)
                {
                    string serverName = listViewServers.SelectedItems[0].SubItems["ServerName"].Text;
                    string connectionString = listViewServers.SelectedItems[0].SubItems["ConnectionString"].Text;

                    Hubble.Core.BigTable.ServerInfo serverInfo =
                        new Hubble.Core.BigTable.ServerInfo(serverName,
                        connectionString);

                    serverInfo = BigTableInfo.ServerList.SingleOrDefault(s => s.Equals(serverInfo));

                    if (serverInfo == null)
                    {
                        return;
                    }

                    FormServerInfo frmServerInfo = new FormServerInfo();

                    if (frmServerInfo.ShowDialog(serverInfo) == DialogResult.OK)
                    {
                        serverInfo = frmServerInfo.ServerInfo;

                        for (int i = 0; i < BigTableInfo.ServerList.Count; i++)
                        {
                            if (BigTableInfo.ServerList[i].Equals(serverInfo))
                            {
                                BigTableInfo.ServerList[i] = serverInfo;
                                break;
                            }
                        }

                        //Find the tablets and change the specified server info 
                        for (int i = 0; i < BigTableInfo.Tablets.Count; i++)
                        {
                            for (int j = 0; j < BigTableInfo.Tablets[i].BalanceServers.Count; j++)
                            {
                                if (BigTableInfo.Tablets[i].BalanceServers[j].Equals(serverInfo))
                                {
                                    BigTableInfo.Tablets[i].BalanceServers[j] = serverInfo.Clone();
                                    break;
                                }
                            }

                            for (int j = 0; j < BigTableInfo.Tablets[i].FailoverServers.Count; j++)
                            {
                                if (BigTableInfo.Tablets[i].FailoverServers[j].Equals(serverInfo))
                                {
                                    BigTableInfo.Tablets[i].FailoverServers[j] = serverInfo.Clone();
                                    break;
                                }
                            }
                        }

                        listViewServers.SelectedItems[0].SubItems["ConnectionString"].Text = connectionString;

                        RefreshServerGUI();
                        RefreshServersComboBox();
                        RefreshTabletGUI();
                    }
                }
            }
            catch (Exception ex)
            {
                QAMessageBox.ShowErrorMessage(ex);
            }
        }

        private void listViewServers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonUpdateServer_Click(sender, e);
        }
    }
}
