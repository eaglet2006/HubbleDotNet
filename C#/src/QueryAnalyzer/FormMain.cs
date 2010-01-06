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

            foreach (string tableName in GetTables(databaseNode.Text))
            {
                int index = tableName.IndexOf(databaseNode.Text, 0, StringComparison.CurrentCultureIgnoreCase);

                string tName = tableName;

                if (index >= 0)
                {
                    tName = tableName.Substring(databaseNode.Text.Length + 1);
                }

                TreeNode tableNode = new TreeNode(tName);
                tableNode.Tag = "Table";
                tableNode.ImageIndex = 1;
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

        private IEnumerable<string> GetTables(string databaseName)
        {
            QueryResult result = GlobalSetting.DataAccess.Excute(string.Format("exec sp_tablelist '{0}'",
                databaseName.Replace("'", "''")));

            foreach (DataRow row in result.DataSet.Tables[0].Rows)
            {
                yield return row["TableName"].ToString();
            }
        }


        private void ShowErrorMessage(string err)
        {
            tabControl1.SelectedTab = tabPageMessages;
            textBoxMessages.ForeColor = Color.Red;
            textBoxMessages.Text = err;
        }

        private void ShowMessages(List<string> messages)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in messages)
            {
                sb.AppendLine(str);
            }

            tabControl1.SelectedTab = tabPageMessages;
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
                        ShowMessages(queryResult.PrintMessages);
                    }
                }

                if (queryResult.DataSet.Tables.Count > 0)
                {
                    if (queryResult.DataSet.Tables.Count == 1)
                    {
                        dataGridViewResult.DataSource = table;
                    }
                    else
                    {
                        dataGridViewResult.DataSource = table ;
                    }
                }
                else
                {
                    dataGridViewResult.DataSource = null;
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
                    tableInfoToolStripMenuItem.Enabled = treeViewData.SelectedNode.Tag.ToString() == "Table";
                    rebuildTableToolStripMenuItem.Enabled = tableInfoToolStripMenuItem.Enabled;

                    refreshToolStripMenuItem.Enabled = treeViewData.SelectedNode.Tag.ToString() == "Server" ||
                        treeViewData.SelectedNode.Tag.ToString() == "Database";

                    if (treeViewData.SelectedNode.Tag.ToString() == "Table")
                    {
                        toolStripComboBoxDatabases.Text = treeViewData.SelectedNode.Parent.Text;
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
            string tableName = treeViewData.SelectedNode.Text;

            FormTableInfo frmTableInfo = new FormTableInfo();
            frmTableInfo.TableName = tableName;
            frmTableInfo.DataAccess = GlobalSetting.DataAccess;
            frmTableInfo.ShowDialog();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void openOToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void batchInsertToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void performanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormPerformance frmPerformance = new FormPerformance();
            frmPerformance.DataAccess = GlobalSetting.DataAccess;
            frmPerformance.ShowDialog();
        }

        private void treeViewData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((string)e.Node.Tag == "Database" && e.Node.Nodes.Count <= 0)
            {
                ShowTables(e.Node);
                e.Node.Expand();
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
            frmRebuildTable.ShowDialog();
        }

    }
}
