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
        class DatabaseTableEnumerate
        {
            /// <summary>
            /// Database Name specified in bigtable
            /// </summary>
            internal string DBName { get; private set; }

            /// <summary>
            /// list all of the tables in this database
            /// </summary>
            internal List<string> Tables { get; private set; }

            private void ListTable(Hubble.Core.BigTable.ServerInfo serverInfo)
            {
                Tables = new List<string>();

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
                                Tables.Add(fullName.Substring(index + 1, fullName.Length - index - 1));
                            }
                        }
                    }
                }
            }

            internal DatabaseTableEnumerate(Hubble.Core.BigTable.ServerInfo serverInfo)
            {
                ListTable(serverInfo);
                this.DBName = serverInfo.ServerName;
            }
        }


        bool _Init = true;
        bool _SettingChanged = false;

        List<DatabaseTableEnumerate> _DBTableEnumerate = null;

        public bool SettingChanged
        {
            get
            {
                return _SettingChanged;
            }
        }

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

        private bool TableInDatabase(string tableName, string dbName)
        {
            var dbTableEnumerate = _DBTableEnumerate.SingleOrDefault(s => s.DBName.Equals(dbName, StringComparison.CurrentCultureIgnoreCase));

            if (dbTableEnumerate == null)
            {
                return false;
            }

            return dbTableEnumerate.Tables.SingleOrDefault(s => s.Equals(tableName, StringComparison.CurrentCultureIgnoreCase)) != null;
        }

        private void RefreshDatabaseTableEnumerate()
        {
            _DBTableEnumerate = new List<DatabaseTableEnumerate>();

            foreach (var serverInfo in _BigTableInfo.ServerList)
            {
                try
                {
                    _DBTableEnumerate.Add(new DatabaseTableEnumerate(serverInfo));
                }
                catch (Exception e)
                {
                    QAMessageBox.ShowInformationMessage(string.Format("Try to connect server:{0} fail. Err:{1}",
                        serverInfo.ServerName, e.Message));
                }
            }
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
            listBoxBalanceServers.Items.Clear();
            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tablet.BalanceServers)
            {
                listBoxBalanceServers.Items.Add(serverInfo);
            }
        }

        private void RefreshFailoverServerList(TabletInfo tablet)
        {
            listBoxFailoverServers.Items.Clear();
            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tablet.FailoverServers)
            {
                listBoxFailoverServers.Items.Add(serverInfo);
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

        private void buttonSettingChanged_Click(object sender, EventArgs e)
        {
            _SettingChanged = true;
        }

        private void SetControlsSettingChanged_Click(Control control)
        {
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.Controls.Count > 0)
                {
                    SetControlsSettingChanged_Click(ctrl);
                }
                else
                {
                    if (ctrl is Button)
                    {
                        (ctrl as Button).Click += new EventHandler(buttonSettingChanged_Click);
                    }
                }
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
                RefreshDatabaseTableEnumerate();
                RefreshGUI();
            }

            _Init = false;

            SetControlsSettingChanged_Click(tabPageServers);
            SetControlsSettingChanged_Click(tabPageTables);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _ParentForm.Close();
        }

        internal bool Save()
        {
            IndexFolder = IndexFolder.Trim();
            TableName = TableName.Trim();
            if (IndexFolder == "")
            {
                QAMessageBox.ShowErrorMessage("Index folder can't be empty.");
                return false;
            }

            if (TableName == "")
            {
                QAMessageBox.ShowErrorMessage("Table name can't be empty.");
                return false;
            }

            if (BigTableInfo.Tablets.Count <= 0)
            {
                QAMessageBox.ShowErrorMessage("BigTable must have at least one tablet!");
                return false;
            }

            foreach (TabletInfo tableInfo in BigTableInfo.Tablets)
            {
                if (tableInfo.BalanceServers.Count <= 0)
                {
                    QAMessageBox.ShowErrorMessage(string.Format("Table name:{0} must include at least one BalanceServer!",
                        tableInfo.TableName));
                    return false;
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

                return true;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }  
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                _SettingChanged = false;
                _ParentForm.Close();
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
            List<Hubble.Core.BigTable.ServerInfo> delServerInfos = new List<Hubble.Core.BigTable.ServerInfo>();

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in listBoxBalanceServers.Items)
            {
                if (!BigTableInfo.ServerList.Contains(serverInfo))
                {
                    delServerInfos.Add(serverInfo);
                }
            }

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in delServerInfos)
            {
                listBoxBalanceServers.Items.Remove(serverInfo);
            }

            delServerInfos.Clear();

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in listBoxFailoverServers.Items)
            {
                if (!BigTableInfo.ServerList.Contains(serverInfo))
                {
                    delServerInfos.Add(serverInfo);
                }
            }

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in delServerInfos)
            {
                listBoxFailoverServers.Items.Remove(serverInfo);
            }
        }

        private void RefreshServersComboBox(string tableName)
        {
            comboBoxBalanceServers.Items.Clear();

            foreach (ServerInfomation serverInfo in BigTableInfo.ServerList)
            {
                if (TableInDatabase(tableName, serverInfo.ServerName))
                {
                    comboBoxBalanceServers.Items.Add(serverInfo);
                }
            }

            comboBoxFailoverServers.Items.Clear();

            foreach (ServerInfomation serverInfo in BigTableInfo.ServerList)
            {
                if (TableInDatabase(tableName, serverInfo.ServerName))
                {
                    comboBoxFailoverServers.Items.Add(serverInfo);
                }
            }

            if (comboBoxBalanceServers.Items.Count > 0)
            {
                comboBoxBalanceServers.SelectedIndex = 0;
            }

            if (comboBoxFailoverServers.Items.Count > 0)
            {
                comboBoxFailoverServers.SelectedIndex = 0;
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

                    RefreshDatabaseTableEnumerate();
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

                    RefreshDatabaseTableEnumerate();

                    Hubble.Core.BigTable.ServerInfo serverInfo =
                                           new Hubble.Core.BigTable.ServerInfo(item.SubItems["ServerName"].Text,
                                           item.SubItems["ConnectionString"].Text);

                    BigTableInfo.RemoveServerInfo(serverInfo);

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

            if (listBoxBalanceServers.Items.Contains(serverInfo))
            {
                QAMessageBox.ShowErrorMessage("Can't input reduplicate server name!");
                return;
            }

            tablet.BalanceServers.Add(serverInfo);
            listBoxBalanceServers.Items.Add(serverInfo);
            buttonEnableBS.Enabled = listBoxBalanceServers.Items.Count > 0;

        }

        private void buttonDeleteBS_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            Hubble.Core.BigTable.ServerInfo serverInfo = listBoxBalanceServers.SelectedItem as Hubble.Core.BigTable.ServerInfo;
            if (serverInfo != null)
            {
                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove server: {0} ?",
                   serverInfo.ServerName)) == DialogResult.Yes)
                {
                    listBoxBalanceServers.Items.Remove(serverInfo);
                    tablet.BalanceServers.Remove(serverInfo);
                }
            }

            buttonEnableBS.Enabled = listBoxBalanceServers.Items.Count > 0;
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

            listBoxFailoverServers.Items.Add(serverInfo);
            tablet.FailoverServers.Add(serverInfo);

            buttonEnableFS.Enabled = listBoxFailoverServers.Items.Count > 0;
        }

        private void buttonDelFailoverServers_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet == null)
            {
                QAMessageBox.ShowErrorMessage("Please choose a tablet");
                return;
            }

            Hubble.Core.BigTable.ServerInfo serverInfo = listBoxFailoverServers.SelectedItem as Hubble.Core.BigTable.ServerInfo;
            if (serverInfo != null)
            {
                if (QAMessageBox.ShowQuestionMessage(string.Format("Are you sure you want to remove server: {0} ?",
                   serverInfo.ServerName)) == DialogResult.Yes)
                {
                    listBoxFailoverServers.Items.Remove(serverInfo);
                    tablet.FailoverServers.Remove(serverInfo);
                }
            }

            buttonEnableFS.Enabled = listBoxFailoverServers.Items.Count > 0;
        }

        private void listBoxTablets_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet != null)
            {
                RefreshServersComboBox(tablet.TableName);
                RefreshBalanceServerList(tablet);
                RefreshFailoverServerList(tablet);
                if (listBoxBalanceServers.Items.Count > 0)
                {
                    listBoxBalanceServers.SelectedIndex = 0;
                    buttonEnableBS.Enabled = true;
                }
                else
                {
                    buttonEnableBS.Enabled = false;
                }

                if (listBoxFailoverServers.Items.Count > 0)
                {
                    listBoxFailoverServers.SelectedIndex = 0;
                    buttonEnableFS.Enabled = true;
                }
                else
                {
                    buttonEnableFS.Enabled = false;
                }
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

                        RefreshDatabaseTableEnumerate();

                        RefreshServerGUI();
                        RefreshTabletGUI();
                        _SettingChanged = true;
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

        private void listBoxTablets_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;

            int index = e.Index;

            if (index >= 0)
            {
                bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

                TabletInfo tableinfo = listBoxTablets.Items[index] as TabletInfo;

                if (selected)
                {
                    g.FillRectangle(new SolidBrush(Color.FromKnownColor(KnownColor.Highlight)), e.Bounds);
                }
                else
                {
                    if (tableinfo.AllDisabled())
                    {
                        g.FillRectangle(new SolidBrush(Color.Gray), e.Bounds);
                    }
                    else if (tableinfo.PartDisabled())
                    {
                        g.FillRectangle(new SolidBrush(Color.Silver), e.Bounds);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
                    }
                }

                g.DrawString(tableinfo.TableName, listBoxTablets.Font, new SolidBrush(Color.Black), e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void listBoxBalanceServer_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            e.DrawBackground();
            Graphics g = e.Graphics;

            int index = e.Index;

            if (index >= 0)
            {
                bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

                Hubble.Core.BigTable.ServerInfo serverinfo = listBox.Items[index] as Hubble.Core.BigTable.ServerInfo;

                if (selected)
                {
                    g.FillRectangle(new SolidBrush(Color.FromKnownColor(KnownColor.Highlight)), e.Bounds);
                }
                else
                {
                    if (!serverinfo.Enabled)
                    {
                        g.FillRectangle(new SolidBrush(Color.Gray), e.Bounds);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
                    }
                }

                g.DrawString(serverinfo.ServerName, listBox.Font, new SolidBrush(Color.Black), e.Bounds);
                e.DrawFocusRectangle();
            }

        }

        private void listBoxBalanceServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxBalanceServers.SelectedIndex;

            if (index >= 0)
            {
                Hubble.Core.BigTable.ServerInfo serverInfo = listBoxBalanceServers.Items[index] as
                    Hubble.Core.BigTable.ServerInfo;

                labelBalanceEnabled.Text = serverInfo.ServerName + " ";
                labelBalanceEnabled.Text += serverInfo.Enabled ? "Enabled" : "Disabled";
                buttonEnableBS.Text = !serverInfo.Enabled ? "Enable" : "Disable";
            }
        }

        private void listBoxFailoverServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxFailoverServers.SelectedIndex;

            if (index >= 0)
            {
                Hubble.Core.BigTable.ServerInfo serverInfo = listBoxFailoverServers.Items[index] as
                    Hubble.Core.BigTable.ServerInfo;

                labelFailoverEnabled.Text = serverInfo.ServerName + " ";
                labelFailoverEnabled.Text += serverInfo.Enabled ? "Enabled" : "Disabled";
                buttonEnableFS.Text = !serverInfo.Enabled ? "Enable" : "Disable";
            }
        }

        private void buttonEnableBS_Click(object sender, EventArgs e)
        {
            int index = listBoxBalanceServers.SelectedIndex;

            if (index >= 0)
            {
                Hubble.Core.BigTable.ServerInfo serverInfo = listBoxBalanceServers.Items[index] as
                    Hubble.Core.BigTable.ServerInfo;
                serverInfo.Enabled = buttonEnableBS.Text == "Enable";

                int tabletIndex = listBoxTablets.SelectedIndex;

                RefreshTabletGUI();
                
                listBoxTablets.SelectedIndex = tabletIndex;

                listBoxBalanceServers.SelectedIndex = index;
            }
        }

        private void buttonEnableFS_Click(object sender, EventArgs e)
        {
            int index = listBoxFailoverServers.SelectedIndex;

            if (index >= 0)
            {
                Hubble.Core.BigTable.ServerInfo serverInfo = listBoxFailoverServers.Items[index] as
                    Hubble.Core.BigTable.ServerInfo;
                serverInfo.Enabled = buttonEnableFS.Text == "Enable";

                int tabletIndex = listBoxTablets.SelectedIndex;

                RefreshTabletGUI();

                listBoxTablets.SelectedIndex = tabletIndex;

                listBoxFailoverServers.SelectedIndex = index;
            }
        }

        private void checkBoxKeepDataIntegrity_CheckedChanged(object sender, EventArgs e)
        {
            _SettingChanged = true;
        }

        private void numericUpDownExecuteTimeout_ValueChanged(object sender, EventArgs e)
        {
            _SettingChanged = true;
        }
    }
}
