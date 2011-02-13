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
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Hubble.SQLClient;
using Hubble.Core.DBAdapter;

namespace QueryAnalyzer
{
    public partial class FormRebuildTable : Form
    {
        internal DbAccess DataAccess { get; set; }
        int _SleepInterval = 120;
        int _SleepRows = 200000;

        private bool _InitError = false;

        bool _Finished = false;
        bool _Stop = false;
        bool _NeedDisableSynchronizeFirst = false;

        string _TableName;
        string _DBAdapterName;

        internal bool InitError
        {
            get
            {
                return _InitError;
            }

            set
            {
                _InitError = value;
            }
        }

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
                return _TableName;
            }

            set
            {
                _TableName = value;
                labelTableName.Text = value;
            }
        }

        public string DBAdapterName
        {
            get
            {
                return _DBAdapterName;
            }
        }

        bool _IndexOnly;
        bool _TableSynchronization;
        long _LastDocId;
        string _DBTableName;
        string _DocIdReplaceField = null;

        Stopwatch _IndexDuration = new Stopwatch();
        Stopwatch _ReadDuration = new Stopwatch();
        Stopwatch _TotalDuration = new Stopwatch();
        double _TotalSqlLength = 0;

        public FormRebuildTable()
        {
            InitializeComponent();
        }

        private void FormRebuildTable_Load(object sender, EventArgs e)
        {
            FormWaiting frmWatting = new FormWaiting();
            frmWatting.Show();

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
                        _DBAdapterName = row["Value"].ToString();
                        labelDbAdapterName.Text = _DBAdapterName;
                    }
                    else if (row["Attribute"].ToString() == "LastDocId")
                    {
                        numericUpDownDocIdFrom.Minimum = long.Parse(row["Value"].ToString());
                        numericUpDownDocIdFrom.Value = numericUpDownDocIdFrom.Minimum;
                        _LastDocId = (long)numericUpDownDocIdFrom.Value;
                    }
                    else if (row["Attribute"].ToString() == "DocId")
                    {
                        if (row["Value"] != DBNull.Value)
                        {
                            _DocIdReplaceField = row["Value"].ToString();
                        }
                    }
                    else if (row["Attribute"].ToString() == "TableSynchronization")
                    {
                        _TableSynchronization = bool.Parse(row["Value"].ToString());
                    }
                }

                if (InitError)
                {
                    checkBoxRebuildWholeTable.Checked = true;
                    checkBoxRebuildWholeTable.Enabled = false;
                }

                labelOptimizeProgress.Visible = false;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                frmWatting.Close();
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

        private string GetSqlServerSelectSql(long from)
        {
            QueryResult qResult = DataAccess.Excute(string.Format("exec sp_columns '{0}'", TableName));

            StringBuilder sb = new StringBuilder();

            if (_DocIdReplaceField == null)
            {
                sb.AppendFormat("Select top {0} [DocId] ", numericUpDownStep.Value);

                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    sb.AppendFormat(", [{0}] ", row["FieldName"].ToString());
                }

                sb.AppendFormat(" from {0} where DocId >= {1} order by DocId", _DBTableName, from);
            }
            else
            {
                sb.AppendFormat("Select top {0} ", numericUpDownStep.Value);

                int i = 0;
                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    if (i == 0)
                    {
                        sb.AppendFormat("[{0}] ", row["FieldName"].ToString());
                    }
                    else
                    {
                        sb.AppendFormat(", [{0}] ", row["FieldName"].ToString());
                    }

                    i++;
                }

                sb.AppendFormat(" from {0} where {1} >= {2} order by {1}", _DBTableName, _DocIdReplaceField, from);
            }

            return sb.ToString();
        }

        private string GetOracleFieldNameList(long from)
        {
            QueryResult qResult = DataAccess.Excute(string.Format("exec sp_columns '{0}'", TableName));

            StringBuilder sb = new StringBuilder();

            if (_DocIdReplaceField == null)
            {
                sb.AppendFormat(" DocId ", numericUpDownStep.Value);

                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    sb.AppendFormat(", {0} ", Oracle8iAdapter.GetFieldName(row["FieldName"].ToString()));
                }
            }
            else
            {
                int i = 0;
                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    if (i == 0)
                    {
                        sb.AppendFormat("{0} ", Oracle8iAdapter.GetFieldName(row["FieldName"].ToString()));
                    }
                    else
                    {
                        sb.AppendFormat(", {0} ", Oracle8iAdapter.GetFieldName(row["FieldName"].ToString()));
                    }

                    i++;
                }
            }

            return sb.ToString();
        }

        private string GetOracleSelectSql(long from)
        {
            string fields = GetOracleFieldNameList(from);

            StringBuilder sb = new StringBuilder();

            if (_DocIdReplaceField == null)
            {
                sb.AppendFormat("select {0} from {1} where DocId >= {2} and rownum <= {3} order by DocId ",
                    fields, _DBTableName, from, numericUpDownStep.Value);
            }
            else
            {
                sb.AppendFormat("select {0} from {1} where {2} >= {3} and rownum <= {4} order by {2} ",
                    fields, _DBTableName, _DocIdReplaceField, from, numericUpDownStep.Value);
            }

            return sb.ToString();
        }

        private string GetMySqlSelectSql(long from)
        {
            QueryResult qResult = DataAccess.Excute(string.Format("exec sp_columns '{0}'", TableName));

            StringBuilder sb = new StringBuilder();

            if (_DocIdReplaceField == null)
            {
                sb.Append("Select `DocId` ");

                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    sb.AppendFormat(", `{0}` ", row["FieldName"].ToString());
                }

                sb.AppendFormat(" from {0} where DocId >= {1} order by DocId  limit {2}", _DBTableName, from, numericUpDownStep.Value);
            }
            else
            {
                sb.Append("Select ");

                int i = 0;
                foreach (DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    if (i == 0)
                    {
                        sb.AppendFormat("`{0}` ", row["FieldName"].ToString());
                    }
                    else
                    {
                        sb.AppendFormat(", `{0}` ", row["FieldName"].ToString());
                    }

                    i++;
                }

                sb.AppendFormat(" from {0} where {1} >= {2} order by {1} limit {3}", _DBTableName, _DocIdReplaceField, from, numericUpDownStep.Value);
            }

            return sb.ToString();

        }


        private string GetSelectSql(long from)
        {
            if (_DBAdapterName.IndexOf("sqlserver", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqlServerSelectSql(from);
            }
            else if (_DBAdapterName.IndexOf("oracle", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetOracleSelectSql(from);
            }
            else if (_DBAdapterName.IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetMySqlSelectSql(from);
            }
            else
            {
                return GetSqlServerSelectSql(from);
            }

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
                    insertString.AppendFormat("[{0}]", TableField.GetFieldName(col.ColumnName));
                }
                else
                {
                    insertString.AppendFormat(", [{0}]", TableField.GetFieldName(col.ColumnName));
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
                    else if (col.DataType == typeof(double) || col.DataType == typeof(Single) || col.DataType == typeof(decimal))
                    {
                        value = string.Format("{0:0.################}", row[col.ColumnName]);
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

                if (_DocIdReplaceField == null)
                {
                    from = long.Parse(row["DocId"].ToString()) + 1;
                }
                else
                {
                    from = long.Parse(row[_DocIdReplaceField].ToString()) + 1;
                }

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

        delegate void DelegateShowOptimizeProgress(double progress);

        private void ShowOptimizeProgress(double progress)
        {

            if (labelOptimizeProgress.InvokeRequired)
            {
                labelOptimizeProgress.Invoke(new DelegateShowOptimizeProgress(ShowOptimizeProgress), progress);
            }
            else
            {
                labelOptimizeProgress.Text = string.Format("Optimize progress: {0:f2}%", progress);

                if (progress >= 100)
                {
                    labelOptimizeProgress.Visible = false;
                }
                else
                {
                    labelOptimizeProgress.Visible = true;
                }
            }
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

        delegate void DelegateFinishRebuild(Stopwatch sw);

        private void FinishRebuild(Stopwatch sw)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateFinishRebuild(FinishRebuild), sw);

            }
            else
            {
                _IndexDuration.Stop();
                _ReadDuration.Stop();
                _TotalDuration.Stop();

                MessageBox.Show(string.Format("During {0} ms", sw.ElapsedMilliseconds), "Rebuild finished!",
    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Close();
            }
        }

        delegate void DelegateShowDuration();

        private void ShowDuration()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateShowDuration(ShowDuration));

            }
            else
            {
                labelIndexDuration.Text = string.Format("Index duration:{0:F1}s", _IndexDuration.Elapsed.TotalSeconds);
                labelReadDuration.Text = string.Format("Read duration:{0:F1}s", _ReadDuration.Elapsed.TotalSeconds);
                labelTotalDuration.Text = string.Format("Total duration:{0:F1}s" , _TotalDuration.Elapsed.TotalSeconds);
                labelTotalSqlLength.Text = string.Format("Total data length:{0:F3}MB", _TotalSqlLength / (1024 * 1024));
                labelIndexSpeed.Text = string.Format("Index speed:{0:F1}MB/h", _TotalSqlLength * 3600 / (1024 * 1024 * _IndexDuration.Elapsed.TotalSeconds));
            }
        }


        private void DoFastOptimize()
        {
            try
            {
                DataAccess.Excute("exec SP_OptimizeTable {0}, 3", TableName);

                int times = 0;

                while (times++ < 1800) //Time out is 3600 s
                {
                    System.Threading.Thread.Sleep(2000);

                    try
                    {
                        QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_GetTableMergeRate {0}", TableName);

                        double totalRate = 0;

                        foreach (DataRow row in queryResult.DataSet.Tables[0].Rows)
                        {
                            double rate = double.Parse(row["Rate"].ToString());

                            if (rate < 0)
                            {
                                rate = 1;
                            }

                            totalRate += rate * 100;
                        }

                        double progress = totalRate / queryResult.DataSet.Tables[0].Rows.Count;

                        if (progress >= 100)
                        {
                            ShowOptimizeProgress(100);
                            break;
                        }
                        else
                        {
                            ShowOptimizeProgress(progress);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                }


                
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TooManyIndexFiles(QueryResult qResult)
        {
            foreach (string message in qResult.PrintMessages)
            {
                if (message != null)
                {
                    if (message.Equals("TooManyIndexFiles"))
                    {
                        return true;
                    }
                }
            }

            return false;
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

                if (_NeedDisableSynchronizeFirst)
                {
                    DataAccess.Excute("exec SP_SetTableAttribute {0}, 'tablesynchronization', 'false'", TableName);
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
                    //DataAccess.ReConnect();

                    _ReadDuration.Start();
                    string insertSql = GetInsertSql(ref from, ref remain, out count);
                    _ReadDuration.Stop();

                    if (!string.IsNullOrEmpty(insertSql))
                    {
                        _IndexDuration.Start();
                        QueryResult qResult = DataAccess.Excute(insertSql);
                        _IndexDuration.Stop();

                        _TotalSqlLength += insertSql.Length * 2; //1 char = 2 bytes

                        if (TooManyIndexFiles(qResult))
                        {
                            DoFastOptimize();//first optimize to optimize index or watting for exist optimizing.
                            DoFastOptimize();
                        }
                    }

                    totalCount += count;
                    ShowCurrentCount(totalCount);
                    ShowDuration();

                    if (_SleepRows > 0 && _SleepInterval > 0 && remain > 0)
                    {
                        if (totalCount % _SleepRows == 0)
                        {
                            System.Threading.Thread.Sleep(_SleepInterval * 1000);
                        }
                    }
                }

                sw.Stop();

                FinishRebuild(sw);
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

                    if (_NeedDisableSynchronizeFirst)
                    {
                        DataAccess.Excute("exec SP_SetTableAttribute {0}, 'tablesynchronization', 'true'", TableName);
                    }
                }
                catch
                {
                }

                _Finished = true;
            }
        }

        private void buttonRebuild_Click(object sender, EventArgs e)
        {
            _NeedDisableSynchronizeFirst = false;

            if (_DocIdReplaceField != null)
            {
                if (_LastDocId > 0)
                {
                    if (!checkBoxRebuildWholeTable.Checked)
                    {
                        MessageBox.Show("You should use synchronize table function to synchronize with database or check RebuildWholeTable to rebuild all records.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        if (_TableSynchronization)
                        {
                            _NeedDisableSynchronizeFirst = true;
                        }
                    }
                }
                else
                {
                    if (_TableSynchronization)
                    {
                        _NeedDisableSynchronizeFirst = true;
                    }
                }
            }

            labelOptimizeProgress.Visible = false;

            _SleepInterval = (int)numericUpDownSleepSeconds.Value;
            _SleepRows = (int)numericUpDownSleepRows.Value;

            buttonStop.Enabled = true;
            groupBoxSetting.Enabled = false;
            buttonRebuild.Enabled = false;
            System.Threading.Thread thread = new System.Threading.Thread(Rebuild);
            thread.IsBackground = true;
            _TotalSqlLength = 0;
            _IndexDuration.Reset();
            _ReadDuration.Reset();
            _TotalDuration.Reset();
            _TotalDuration.Start();

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
