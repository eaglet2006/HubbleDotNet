using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class FormRebuildTable : Form
    {
        internal DbAccess DataAccess { get; set; }
        bool _Finished = false;
        bool _Stop = false;

        bool Stop
        {
            get
            {
                lock (this)
                {
                    return _Stop;
                }
            }

            set
            {
                lock (this)
                {
                    _Stop = value;
                }
            }

        }

        public string TableName
        {
            get
            {
                return labelTableName.Text;
            }

            set
            {
                labelTableName.Text = value;
            }
        }

        public string DBAdapterName
        {
            get
            {
                return labelDbAdapterName.Text;
            }
        }

        bool _IndexOnly;
        long _LastDocId;
        string _DBTableName;

        public FormRebuildTable()
        {
            InitializeComponent();
        }

        private void FormRebuildTable_Load(object sender, EventArgs e)
        {
            try
            {
                QueryResult result = DataAccess.Excute("exec SP_GetTableAttributes {0}", TableName);

                foreach (System.Data.DataRow row in result.DataSet.Tables[0].Rows)
                {
                    if (row["Attribute"].ToString() == "IndexOnly")
                    {
                        _IndexOnly = bool.Parse(row["Value"].ToString());
                        labelIndexOnly.Text = _IndexOnly.ToString();
                    }
                    else if (row["Attribute"].ToString() == "DBTableName")
                    {
                        _DBTableName = row["Value"].ToString();
                    }
                    else if (row["Attribute"].ToString() == "DBAdapter")
                    {
                        labelDbAdapterName.Text = row["Value"].ToString();
                    }
                    else if (row["Attribute"].ToString() == "LastDocId")
                    {
                        numericUpDownDocIdFrom.Minimum = long.Parse(row["Value"].ToString());
                        numericUpDownDocIdFrom.Value = numericUpDownDocIdFrom.Minimum;
                        _LastDocId = (long)numericUpDownDocIdFrom.Value;
                    }
                }

            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void checkBoxRebuildWholeTable_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRebuildWholeTable.Checked)
            {
                numericUpDownDocIdFrom.Minimum = 0;
                numericUpDownDocIdFrom.Value = 0;
            }
            else
            {
                numericUpDownDocIdFrom.Minimum = _LastDocId;
                numericUpDownDocIdFrom.Value = _LastDocId;
            }
        }

        private string GetSelectSql(long from)
        {
            QueryResult qResult = DataAccess.Excute(string.Format("exec sp_columns '{0}'", TableName));

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Select top {0} [DocId] ", numericUpDownStep.Value);

            foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
            {
                sb.AppendFormat(", [{0}] ", row["FieldName"].ToString());
            }

            sb.AppendFormat(" from {0} where DocId >= {1} order by DocId", _DBTableName, from);

            return sb.ToString();
        }

        private string GetOneRowSql(DataSet schema, DataRow row, string tableName)
        {
            StringBuilder insertString = new StringBuilder();

            insertString.AppendFormat("Insert {0} (", tableName);

            int i = 0;

            foreach (DataColumn col in schema.Tables[0].Columns)
            {
                if (i == 0)
                {
                    insertString.AppendFormat("[{0}]", col.ColumnName);
                }
                else
                {
                    insertString.AppendFormat(", [{0}]", col.ColumnName);
                }

                i++;

            }

            insertString.Append(") Values(");

            i = 0;

            foreach (DataColumn col in schema.Tables[0].Columns)
            {
                string value;

                if (row[col.ColumnName] == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {

                    if (col.DataType == typeof(string))
                    {
                        value = "'" + row[col.ColumnName].ToString().Replace("'", "''") + "'";
                    }
                    else if (col.DataType == typeof(DateTime))
                    {
                        value = "'" + ((DateTime)(row[col.ColumnName])).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    }
                    else
                    {
                        value = row[col.ColumnName].ToString();
                    }
                }


                if (i == 0)
                {
                    insertString.AppendFormat("{0}", value);
                }
                else
                {
                    insertString.AppendFormat(", {0}", value);
                }

                i++;
            }

            insertString.Append(");");

            return insertString.ToString();
        }

        private string GetInsertSql(ref long from, ref int remain, out long count)
        {
            QueryResult qResult = DataAccess.Excute("exec SP_QuerySql {0}, {1}", TableName, GetSelectSql(from));
            StringBuilder sb = new StringBuilder();
            count = 0;

            foreach (System.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
            {
                sb.AppendLine(GetOneRowSql(qResult.DataSet, row, TableName));

                from = long.Parse(row["DocId"].ToString()) + 1;

                if (remain > 0)
                {
                    remain--;
                }

                count++;

                if (remain == 0)
                {
                    return sb.ToString();
                }
            }

            if (qResult.DataSet.Tables[0].Rows.Count < numericUpDownStep.Value)
            {
                remain = 0;
            }

            return sb.ToString();
        }

        delegate void DelegateShowCurrentCount(long count);

        private void ShowCurrentCount(long count)
        {
            if (labelCurrentCount.InvokeRequired)
            {
                labelCurrentCount.Invoke(new DelegateShowCurrentCount(ShowCurrentCount), count);

            }
            else
            {
                labelCurrentCount.Text = count.ToString();
            }
        }

        private void Rebuild()
        {
            try
            {
                DataAccess.Excute("exec SP_TableIndexOnly {0}, {1}",
                    TableName, "True");

                int remain = -1;
                long count;
                long from = (long)numericUpDownDocIdFrom.Value;

                if (checkBoxRebuildWholeTable.Checked)
                {
                    DataAccess.Excute("exec SP_TruncateTable {0}", TableName);
                }

                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                long totalCount = 0;

                int importCount = (int)numericUpDownImportCount.Value;

                if (importCount > 0)
                {
                    remain = importCount;
                }


                while (remain != 0 && !Stop)
                {
                    string insertSql = GetInsertSql(ref from, ref remain, out count);
                    DataAccess.Excute(insertSql);
                    totalCount += count;
                    ShowCurrentCount(totalCount);
                }

                sw.Stop();
                MessageBox.Show(string.Format("During {0} ms", sw.ElapsedMilliseconds), "Rebuild finished!",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    DataAccess.Excute("exec SP_TableIndexOnly {0}, {1}",
                        TableName, _IndexOnly.ToString());

                }
                catch
                {
                }

                _Finished = true;
            }
        }

        private void buttonRebuild_Click(object sender, EventArgs e)
        {
            buttonStop.Enabled = true;
            groupBoxSetting.Enabled = false;
            buttonRebuild.Enabled = false;
            System.Threading.Thread thread = new System.Threading.Thread(Rebuild);
            thread.IsBackground = true;
            thread.Start();
        }

        private void FormRebuildTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (buttonRebuild.Enabled = false && !_Finished)
            {
                e.Cancel = true;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            buttonStop.Enabled = false;
            Stop = true;
        }


    }
}
