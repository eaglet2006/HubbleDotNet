/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Hubble.Core.SFQL.Parse;
using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class FormMain : Form
    {
        struct TableInfo
        {
            public string TableName;
            public string InitError;

            public TableInfo(string tableName, string initError)
            {
                TableName = tableName;
                InitError = initError;
            }

            public override string ToString()
            {
                return "Table";
            }
        }
        

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            ShowTree();
        }

        private void ShowTables(TreeNode databaseNode)
        {
            databaseNode.Nodes.Clear();

            foreach (TableInfo tableInfo in GetTables(databaseNode.Text))
            {
                int index = tableInfo.TableName.IndexOf(databaseNode.Text, 0, StringComparison.CurrentCultureIgnoreCase);

                string tName = tableInfo.TableName;

                if (index >= 0)
                {
                    tName = tableInfo.TableName.Substring(databaseNode.Text.Length + 1);
                }

                TreeNode tableNode = new TreeNode(tName);
                tableNode.Tag = tableInfo;

                if (string.IsNullOrEmpty(tableInfo.InitError))
                {
                    tableNode.ImageIndex = 1;
                }
                else
                {
                    tableNode.ImageIndex = 5;
                }
                
                tableNode.SelectedImageIndex = tableNode.ImageIndex;
                databaseNode.Nodes.Add(tableNode);
            }
        }

        private void ShowTree()
        {
            try
            {
                treeViewData.Nodes.Clear();
                toolStripComboBoxDatabases.Items.Clear();

                TreeNode serverNode = new TreeNode(GlobalSetting.DataAccess.ServerName);
                serverNode.Tag = "Server";
                serverNode.ImageIndex = 0;
                treeViewData.Nodes.Add(serverNode);

                foreach (string databaseName in GetDatabases())
                {
                    TreeNode databaseNode = new TreeNode(databaseName);
                    databaseNode.Tag = "Database";
                    databaseNode.ImageIndex = 3;
                    databaseNode.SelectedImageIndex = databaseNode.ImageIndex;
                    serverNode.Nodes.Add(databaseNode);
                    toolStripComboBoxDatabases.Items.Add(databaseName);
                }

                toolStripComboBoxDatabases.Text = GlobalSetting.DataAccess.DatabaseName;

            }
            catch (Exception e)
            {
                ShowErrorMessage(e.Message);
            }

        }

        private IEnumerable<string> GetDatabases()
        {
            QueryResult result = GlobalSetting.DataAccess.Excute("exec sp_databaselist");

            foreach (DataRow row in result.DataSet.Tables[0].Rows)
            {
                yield return row["DatabaseName"].ToString();
            }
        }

        private IEnumerable<TableInfo> GetTables(string databaseName)
        {
            QueryResult result = GlobalSetting.DataAccess.Excute(string.Format("exec sp_tablelist '{0}'",
                databaseName.Replace("'", "''")));

            foreach (DataRow row in result.DataSet.Tables[0].Rows)
            {
                yield return new TableInfo(row["TableName"].ToString(), row["InitError"].ToString());
            }
        }


        private void ShowErrorMessage(string err)
        {
            tabControl1.SelectedTab = tabPageMessages;
            textBoxMessages.ForeColor = Color.Red;
            textBoxMessages.Text = err;
        }

        private void ShowMessages(List<string> messages, bool showMessage)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in messages)
            {
                sb.AppendLine(str);
            }

            if (showMessage)
            {
                tabControl1.SelectedTab = tabPageMessages;
            }

            textBoxMessages.ForeColor = Color.Black;
            textBoxMessages.Text = sb.ToString();

        }

        private void toolStripButtonExcute_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripStatusLabelReport.Text = "";

                tabControl1.SelectedTab = tabPageResults;
                textBoxMessages.Text = "";

                QueryPerfCounter qp = new QueryPerfCounter();
                qp.Start();

                int count = 0;

                SFQLParse sfqlParse = new SFQLParse();
                string sql = textBoxSql.Text;

                if (!string.IsNullOrEmpty(textBoxSql.SelectedText))
                {
                    sql = textBoxSql.SelectedText;
                }

                QueryResult queryResult = GlobalSetting.DataAccess.Excute(sql, 0);

                System.Data.DataTable table = null;

                if (queryResult.DataSet.Tables.Count > 0)
                {
                    table = queryResult.DataSet.Tables[0];
                    count = table.MinimumCapacity;
                }

                qp.Stop();
                double ns = qp.Duration(1);

                StringBuilder report = new StringBuilder();

                report.AppendFormat("Duration:{0} ", (ns / (1000 * 1000)).ToString("0.00") + " ms");
                report.AppendFormat("Count={0}", count);

                if (queryResult.PrintMessages != null)
                {
                    if (queryResult.PrintMessages.Count > 0)
                    {
                        ShowMessages(queryResult.PrintMessages, table == null);
                    }
                }

                MultiGridView mulitGridView = new MultiGridView(panelResult, queryResult.DataSet.Tables.Count);

                for (int i = 0; i < queryResult.DataSet.Tables.Count; i++)
                {
                    mulitGridView.GridViewList[i].DataSource = queryResult.DataSet.Tables[i];
                }

                toolStripStatusLabelReport.Text = report.ToString();

            }
            catch (Hubble.Core.SFQL.LexicalAnalysis.LexicalException lexicalEx)
            {
                ShowErrorMessage(lexicalEx.ToString());
            }
            catch (Hubble.Core.SFQL.SyntaxAnalysis.SyntaxException syntaxEx)
            {
                ShowErrorMessage(syntaxEx.ToString());
            }
            catch (Hubble.Framework.Net.ServerException e1)
            {
                ShowErrorMessage(e1.Message + "\r\n" + e1.InnerStackTrace);
            }
            catch (Exception e1)
            {
                ShowErrorMessage(e1.Message + "\r\n" + e1.StackTrace);
            }
            finally
            {
            }

            textBoxSql.Focus();
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyValue == 'e' || e.KeyValue == 'E')
                {
                    toolStripButtonExcute_Click(sender, e);
                }
            }
        }

        private void treeViewData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    contextMenuStripServer.Enabled = false;
                    contextMenuStripDatabase.Enabled = false;
                    contextMenuStripTable.Enabled = false;

                    troubleshooterToolStripMenuItem.Enabled = false;
                    troubleshooterToolStripMenuItem1.Enabled = false;
                    troubleshooterToolStripMenuItem2.Enabled = false;


                    if (treeViewData.SelectedNode.Tag.ToString() == "Table")
                    {
                        TableInfo tableInfo = (TableInfo)treeViewData.SelectedNode.Tag;
                        treeViewData.ContextMenuStrip = contextMenuStripTable;
                        contextMenuStripTable.Enabled = true;

                        toolStripComboBoxDatabases.Text = treeViewData.SelectedNode.Parent.Text;

                        if (!string.IsNullOrEmpty(tableInfo.InitError))
                        {
                            troubleshooterToolStripMenuItem.Enabled = true;
                        }
                    }
                    else if (treeViewData.SelectedNode.Tag.ToString() == "Server")
                    {
                        treeViewData.ContextMenuStrip = contextMenuStripServer;
                        contextMenuStripServer.Enabled = true;
                    }
                    else if (treeViewData.SelectedNode.Tag.ToString() == "Database")
                    {
                        treeViewData.ContextMenuStrip = contextMenuStripDatabase;
                        contextMenuStripDatabase.Enabled = true;
                    }
                }
                catch(Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tableInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string tableName = treeViewData.SelectedNode.Text;

                FormTableInfo frmTableInfo = new FormTableInfo();
                frmTableInfo.TableName = tableName;
                frmTableInfo.DataAccess = GlobalSetting.DataAccess;
                frmTableInfo.ShowDialog();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeViewData.SelectedNode.Tag.ToString() == "Server")
                {
                    ShowTree();
                }
                else if (treeViewData.SelectedNode.Tag.ToString() == "Database")
                {
                    ShowTables(treeViewData.SelectedNode);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialogSql.ShowDialog() == DialogResult.OK)
                {
                    string sqlText = Hubble.Framework.IO.File.ReadFileToString(
                        openFileDialogSql.FileName, Encoding.UTF8);

                    if (sqlText.Length > textBoxSql.MaxLength)
                    {
                        MessageBox.Show(string.Format("SQL file length large then {0}. It will be truncated", textBoxSql.MaxLength),
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        sqlText = sqlText.Substring(0, textBoxSql.MaxLength);
                    }

                    textBoxSql.Text = sqlText;
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void batchInsertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialogSql.ShowDialog() == DialogResult.OK)
                {
                    BatchInsert batchInsert = new BatchInsert();

                    FormWaittingGetTotalRecords frmWatting = new FormWaittingGetTotalRecords();

                    frmWatting.Show();

                    int totalRecords = batchInsert.GetTotalRecords(openFileDialogSql.FileName,
                        frmWatting.GetTotalRecordsDelegate);

                    frmWatting.Close();

                    FormBatchInsert frmBatchInsert = new FormBatchInsert();

                    frmBatchInsert.TotalRecords = totalRecords;
                    frmBatchInsert.DataAccess = GlobalSetting.DataAccess;
                    frmBatchInsert.FileName = openFileDialogSql.FileName;
                    frmBatchInsert.BatchInsert = batchInsert;
                    frmBatchInsert.ShowDialog();
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void performanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormPerformance frmPerformance = new FormPerformance();
                frmPerformance.DataAccess = GlobalSetting.DataAccess;
                frmPerformance.ShowDialog();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeViewData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Node.Tag.ToString() == "Database" && e.Node.Nodes.Count <= 0)
                {
                    ShowTables(e.Node);
                    e.Node.Expand();
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripComboBoxDatabases_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void toolStripComboBoxDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripComboBoxDatabases.Text))
            {
                return;
            }

            try
            {
                GlobalSetting.DataAccess.ChangeDatabase(toolStripComboBoxDatabases.Text);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rebuildTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormRebuildTable frmRebuildTable = new FormRebuildTable();
            frmRebuildTable.TableName = treeViewData.SelectedNode.Text;
            frmRebuildTable.DataAccess = GlobalSetting.DataAccess;
            TableInfo tableInfo = (TableInfo)treeViewData.SelectedNode.Tag;
            if (!string.IsNullOrEmpty(tableInfo.InitError))
            {
                frmRebuildTable.InitError = true;
            }

            frmRebuildTable.ShowDialog();
        }

        private void troubleshooterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewData.SelectedNode.Tag.ToString() == "Table")
            {
                TableInfo tableInfo = (TableInfo)treeViewData.SelectedNode.Tag;
                if (!string.IsNullOrEmpty(tableInfo.InitError))
                {
                    FormTroubleshooter frmTroubleshooter = new FormTroubleshooter();
                    frmTroubleshooter.ShowDialog(tableInfo.InitError);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

    }
}
