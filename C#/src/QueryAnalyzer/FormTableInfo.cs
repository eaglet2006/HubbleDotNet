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
        class AttributeCtrl
        {
            public string OriginalValue;
            public Control Ctrl;

            public AttributeCtrl(Control ctrl)
            {
                OriginalValue = ctrl.Text;
                Ctrl = ctrl;
            }

        }

        public FormTableInfo()
        {
            InitializeComponent();

            InitTableInfoControlDict();

        }

        internal string TableName { get; set; }
        internal DbAccess DataAccess { get; set; }



        private Dictionary<string, AttributeCtrl> _TableInfoControlDict;
        
        private void InitTableInfoControlDict()
        {
            _TableInfoControlDict = new Dictionary<string, AttributeCtrl>();

            foreach (Control ctrl in tabPageAttributes.Controls)
            {
                if (ctrl.Tag != null)
                {
                    _TableInfoControlDict.Add(ctrl.Tag.ToString(), new AttributeCtrl(ctrl));
                }
            }
        }

        private void FormTableInfo_Load(object sender, EventArgs e)
        {
            try
            {
                QueryResult qResult = DataAccess.Excute("exec sp_columns {0}", TableName);
                ShowFields(qResult.DataSet.Tables[0]);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void ShowTableAttributes(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                AttributeCtrl ctrl;
                if (_TableInfoControlDict.TryGetValue(row["Attribute"].ToString(), out ctrl))
                {
                    if (ctrl.Ctrl is NumericUpDown)
                    {
                        (ctrl.Ctrl as NumericUpDown).Value = long.Parse(row["Value"].ToString());

                        ctrl.OriginalValue = (ctrl.Ctrl as NumericUpDown).Value.ToString();
                    }
                    else
                    {
                        ctrl.Ctrl.Text = row["Value"].ToString();
                        ctrl.OriginalValue = ctrl.Ctrl.Text;
                    }

                    
                }
            }
        }

        private void SetTableAttributes()
        {
            foreach (AttributeCtrl ctrl in _TableInfoControlDict.Values)
            {
                string value;

                if (ctrl.Ctrl is NumericUpDown)
                {
                    value = (ctrl.Ctrl as NumericUpDown).Value.ToString();
                }
                else
                {
                    value = ctrl.Ctrl.Text;
                }

                if (ctrl.OriginalValue != value)
                {
                    DataAccess.Excute("exec SP_SetTableAttribute {0}, {1}, {2}",
                        TableName, ctrl.Ctrl.Tag.ToString(), ctrl.Ctrl.Text);
                }
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

        private void buttonInfoCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPageAttributes)
            {
                FormWaiting frmWatting = new FormWaiting();
                frmWatting.Show();

                try
                {
                    QueryResult qResult = DataAccess.Excute("exec SP_GetTableAttributes {0}", TableName);
                    ShowTableAttributes(qResult.DataSet.Tables[0]);
                    frmWatting.Close();
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    frmWatting.Close();

                    this.Close();
                }
            }
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            SetTableAttributes();
            Close();
        }


    }
}
