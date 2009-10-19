using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Hubble.Core.SFQL.Parse;

namespace QueryAnalyzer
{
    public partial class FormMain : Form
    {
        internal DbAccess DataAccess { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            ShowTree();
        }

        private void ShowTree()
        {
            treeViewData.Nodes.Clear();

            TreeNode serverNode = new TreeNode(DataAccess.ServerName);
            serverNode.Tag = "Server";
            serverNode.ImageIndex = 0;
            treeViewData.Nodes.Add(serverNode);

            foreach (string tableName in GetTables())
            {
                TreeNode tableNode = new TreeNode(tableName);
                tableNode.Tag = "Table";
                tableNode.ImageIndex = 1;
                tableNode.SelectedImageIndex = tableNode.ImageIndex;
                serverNode.Nodes.Add(tableNode);
            }
        }

        private IEnumerable<string> GetTables()
        {
            QueryResult result = DataAccess.Excute("exec sp_tablelist");

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

                QueryResult queryResult = DataAccess.Excute(sql);

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
            catch (Exception e1)
            {
                ShowErrorMessage(e1.Message + "\r\n" + e1.StackTrace);
            }
            finally
            {
            }

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
                tableInfoToolStripMenuItem.Enabled = treeViewData.SelectedNode.Tag.ToString() == "Table";
                refreshToolStripMenuItem.Enabled = treeViewData.SelectedNode.Tag.ToString() == "Server";
            }
        }

        private void tableInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tableName = treeViewData.SelectedNode.Text;

            FormTableInfo frmTableInfo = new FormTableInfo();
            frmTableInfo.TableName = tableName;
            frmTableInfo.DataAccess = DataAccess;
            frmTableInfo.ShowDialog();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTree();
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
                frmBatchInsert.DataAccess = DataAccess;
                frmBatchInsert.FileName = openFileDialogSql.FileName;
                frmBatchInsert.BatchInsert = batchInsert;
                frmBatchInsert.ShowDialog();
            }
        }

        private void performanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormPerformance frmPerformance = new FormPerformance();
            frmPerformance.DataAccess = this.DataAccess;
            frmPerformance.ShowDialog();
        }
    }
}
