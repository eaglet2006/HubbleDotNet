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
    public partial class FormCreateTable : Form
    {
        DialogResult _Result = DialogResult.Cancel;

        TabPage[] _Pages;
        int _CurPage = 0;

        List<TableField> _TableFields = new List<TableField>();

        List<string> _AnalyzerList = new List<string>();

        private string _DefaultIndexFolder;

        internal string DefaultIndexFolder
        {
            get
            {
                return _DefaultIndexFolder;
            }

            set
            {
                _DefaultIndexFolder = value;
            }
        }

        private string _DatabaseName;

        internal string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
        }

        internal string[] AnalyzerList
        {
            get
            {
                return _AnalyzerList.ToArray();
            }
        }

        CreateTable.IBefore[] BeforeEvents = 
        {
            new CreateTable.BeforeDatabaseAttributes(),
            null,
            null,
        
        };

        CreateTable.IAfter[] AfterEvents = 
        {
            new CreateTable.AfterDatabaseAttributes(),
            new CreateTable.AfterIndexMode() ,
            new CreateTable.AfterFields(),

        };

        private void ClearTableFields()
        {
            List<TableField> tableFields = new List<TableField>();

            foreach(Control control in panelFields.Controls)
            {
                TableField tableField = control as TableField;

                if (tableField != null)
                {
                    tableFields.Add(tableField);
                }
            }

            foreach (TableField tableField in tableFields)
            {
                panelFields.Controls.Remove(tableField);
            }
        }

        internal void ClearAllTableFields()
        {
            ClearTableFields();
            _TableFields.Clear();
        }

        internal void AddTableField(System.Data.DataColumn col)
        {
            TableField tableField = new TableField(col, AnalyzerList);
            tableField.Enabled = true;
            _TableFields.Add(tableField);
        }

        internal void ShowTableField()
        {
            ClearTableFields();

            int currentTop = panelHead.Top + panelHead.Height;

            foreach (TableField tableField in _TableFields)
            {
                tableField.Top = currentTop;
                tableField.Left = panelHead.Left;
                tableField.Visible = true;
                currentTop += tableField.Height;
                panelFields.Controls.Add(tableField);
            }
        }

        internal void CheckDocIdReplaceField()
        {
            if (radioButtonAll.Checked)
            {
                string docIdReplaceFieldName = textBoxDocIdReplaceField.Text.Trim().Replace("'", "''");
                if (docIdReplaceFieldName == "")
                {
                    throw new Exception("ID Field can't be empty!");
                }

                bool IDFieldExists = false;

                foreach (TableField tableField in _TableFields)
                {
                    if (tableField.FieldName.Equals(docIdReplaceFieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        switch (tableField.DataType)
                        {
                            case "TinyInt":
                            case "SmallInt":
                            case "Int":
                            case "BigInt":
                                if (tableField.IndexType == "Untokenized")
                                {
                                    IDFieldExists = true;
                                }
                                break;
                        }
                    }
                }

                if (!IDFieldExists)
                {
                    throw new Exception(string.Format("ID Field: {0} does not exists in the table or is not UnTokenized index or is not int data type!",
                        docIdReplaceFieldName));
                }
            }
        }

        internal string GetCreateTableSql()
        {
            StringBuilder sb = new StringBuilder();

            if (radioButtonCreateTableFromExistTable.Checked)
            {
                sb.Append("[IndexOnly]\r\n");

                if (radioButtonAll.Checked)
                {
                    string docIdReplaceFieldName = textBoxDocIdReplaceField.Text.Trim().Replace("'", "''");

                    sb.AppendFormat("[DocId('{0}')]\r\n", docIdReplaceFieldName);
                }
            }

            sb.AppendFormat("[Directory ('{0}')]\r\n", textBoxIndexFolder.Text.Replace("'", "''"));
            sb.AppendFormat("[DBTableName ('{0}')]\r\n", textBoxDBTableName.Text.Replace("'", "''"));
            sb.AppendFormat("[DBAdapter ('{0}')]\r\n", comboBoxDBAdapter.Text.Replace("'", "''"));
            sb.AppendFormat("[DBConnect ('{0}')]\r\n", textBoxConnectionString.Text.Replace("'", "''"));



            sb.AppendFormat("Create table {0}\r\n", textBoxTableName.Text.Replace("'", "''"));
            sb.AppendLine("(");

            int i = 0;
            foreach (TableField tableField in _TableFields)
            {
                if (i == _TableFields.Count - 1)
                {
                    sb.AppendLine(tableField.GetSql());
                }
                else
                {
                    sb.AppendLine(tableField.GetSql() + ",");
                }

                i++;
            }

            sb.AppendLine(");");

            return sb.ToString();
        }

        public FormCreateTable(string databaseName)
        {
            _DatabaseName = databaseName;
            InitializeComponent();
        }

        new public DialogResult ShowDialog()
        {
            base.ShowDialog();
            return _Result;
        }

        private void FormCreateTable_Load(object sender, EventArgs e)
        {
            _Pages = new TabPage[tabControl.TabCount];

            for (int i = 0; i < tabControl.TabCount; i++)
            {
                _Pages[i] = tabControl.TabPages[i];
            }

            tabControl.TabPages.Clear();

            tabControl.TabPages.Add(_Pages[0]);

            buttonBack.Enabled = false;

            try
            {
                QueryResult qResult = GlobalSetting.DataAccess.Excute("exec SP_AnalyzerList");
                            
                foreach(System.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    _AnalyzerList.Add(row["Name"].ToString().Trim());
                }


                if (0 < BeforeEvents.Length)
                {
                    if (BeforeEvents[0] != null)
                    {
                        BeforeEvents[0].Do(this);
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            buttonFinish.Enabled = false;

            if (_CurPage > 0)
            {
                _CurPage--;
            }
            else
            {
                return;
            }

            tabControl.TabPages.Clear();
            tabControl.TabPages.Add(_Pages[_CurPage]);

            if (_CurPage == 0)
            {
                buttonBack.Enabled = false;
            }

            if (_CurPage < _Pages.Length - 1)
            {
                buttonNext.Enabled = true;
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (_CurPage < AfterEvents.Length)
                {
                    if (AfterEvents[_CurPage] != null)
                    {
                        AfterEvents[_CurPage].Do(this);
                    }
                }

                if (_CurPage < _Pages.Length - 1)
                {
                    _CurPage++;
                }
                else
                {
                    return;
                }

                buttonFinish.Enabled = _CurPage == _Pages.Length - 1;

                tabControl.TabPages.Clear();
                tabControl.TabPages.Add(_Pages[_CurPage]);

                if (_CurPage > 0)
                {
                    buttonBack.Enabled = true;
                }

                if (_CurPage >= _Pages.Length - 1)
                {
                    buttonNext.Enabled = false;
                }

                if (_CurPage < BeforeEvents.Length)
                {
                    if (BeforeEvents[_CurPage] != null)
                    {
                        BeforeEvents[_CurPage].Do(this);
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void radioButtonIndexMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCreateNewTable.Checked)
            {
                labelDBTableName.Text = "TableName in database";
                groupBoxIncrementalMode.Visible = false;
            }
            else
            {
                labelDBTableName.Text = "Exist Table Name or View Name in database";
                groupBoxIncrementalMode.Visible = true;
            }
        }

        private void buttonAddField_Click(object sender, EventArgs e)
        {
            TableField tableField = new TableField(radioButtonCreateTableFromExistTable.Checked, AnalyzerList);
            tableField.Enabled = true;

            _TableFields.Add(tableField);

            ShowTableField();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            List<TableField> tableFields = new List<TableField>();

            foreach (TableField tableField in _TableFields)
            {
                if (tableField.Selected)
                {
                    tableFields.Add(tableField);
                }
            }

            foreach (TableField tableField in tableFields)
            {
                _TableFields.Remove(tableField);
            }

            ShowTableField();
        }

        private void buttonTestConnectionString_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxDBAdapter.Text.Trim() == "")
                {
                    MessageBox.Show("Can't use empty Default DBAdapter!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (textBoxConnectionString.Text.Trim() == "")
                {
                    MessageBox.Show("Can't use empty Default connection string!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                GlobalSetting.DataAccess.Excute("exec sp_TestConnectionString {0}, {1}",
                        comboBoxDBAdapter.Text.Trim(),
                        textBoxConnectionString.Text.Trim());
                MessageBox.Show("Connect successful!", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void radioButtonAppendOnly_CheckedChanged(object sender, EventArgs e)
        {
            panelDocIdReplaceField.Visible = !radioButtonAppendOnly.Checked;
        }

        private void buttonFinish_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalSetting.DataAccess.Excute(textBoxScript.Text);
                _Result = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxTableName_TextChanged(object sender, EventArgs e)
        {
            if (textBoxIndexFolder.Text.IndexOf(DefaultIndexFolder, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                textBoxIndexFolder.Text = Hubble.Framework.IO.Path.AppendDivision(
                    Hubble.Framework.IO.Path.AppendDivision(DefaultIndexFolder, '\\') +
                    textBoxTableName.Text, '\\');

            }
        }
    }
}
