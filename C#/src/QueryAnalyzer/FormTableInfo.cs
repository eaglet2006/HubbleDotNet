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

using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class FormTableInfo : Form
    {
        public FormTableInfo()
        {
            InitializeComponent();
        }

        internal string TableName { get; set; }
        internal DbAccess DataAccess { get; set; }

        private void FormTableInfo_Load(object sender, EventArgs e)
        {
            try
            {
                QueryResult qResult = DataAccess.Excute(string.Format("exec sp_columns '{0}'", TableName));
                ShowFields(qResult.DataSet.Tables[0]);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

        }

        private void ShowFields(DataTable table)
        {
            int currentTop = panelHead.Top + panelHead.Height;

            foreach (DataRow row in table.Rows)
            {
                TableField tableField = new TableField();

                tableField.FieldName = row["FieldName"].ToString();
                tableField.DataType = row["DataType"].ToString();
                tableField.IndexType = row["IndexType"].ToString();
                tableField.DataLength = int.Parse(row["DataLength"].ToString());
                tableField.AnalyzerName = row["Analyzer"].ToString();
                tableField.IsNull = bool.Parse(row["IsNull"].ToString());
                tableField.IsPK = bool.Parse(row["IsPrimaryKey"].ToString());
                tableField.DefaultValue = row["Default"].ToString();
                tableField.Top = currentTop;
                tableField.Left = panelHead.Left;
                tableField.Visible = true;
                tableField.Enabled = false;
                currentTop += tableField.Height;
                tabPageFields.Controls.Add(tableField);

            }
        }
    }
}
