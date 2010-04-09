using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class TableField : UserControl
    {
        bool _IndexOnly;

        #region Properties

        public bool Selected
        {
            get
            {
                return checkBoxSelect.Checked;
            }
        }

        public string FieldName
        {
            get
            {
                return textBoxFieldName.Text.Trim();
            }

            set
            {
                textBoxFieldName.Text = value;
            }
        }

        public string DataType
        {
            get
            {
                return comboBoxDataType.Text.Trim();
            }

            set
            {
                comboBoxDataType.Text = value;
            }
        }

        public int DataLength
        {
            get
            {
                return (int)numericUpDownDataLength.Value;
            }

            set
            {
                numericUpDownDataLength.Value = value;
            }
        }

        public string IndexType
        {
            get
            {
                return comboBoxIndexType.Text.Trim();
            }

            set
            {
                comboBoxIndexType.Text = value;
            }
        }

        public string AnalyzerName
        {
            get
            {
                return comboBoxAnalyzer.Text.Trim();
            }

            set
            {
                comboBoxAnalyzer.Text = value;
            }
        }

        public bool IsNull
        {
            get
            {
                return checkBoxNull.Checked;
            }

            set
            {
                checkBoxNull.Checked = value;
            }
        }

        public bool IsPK
        {
            get
            {
                return checkBoxPK.Checked;
            }

            set
            {
                if (_IndexOnly)
                {
                    checkBoxPK.Checked = false;
                }
                else
                {
                    checkBoxPK.Checked = value;
                }
            }
        }

        public string DefaultValue
        {
            get
            {
                return textBoxDefault.Text.Trim();
            }

            set
            {
                textBoxDefault.Text = value;
            }
        }

        #endregion

        private void SetDataType(System.Data.DataColumn col)
        {
            if (col.DataType == typeof(byte))
            {
                DataType = "TinyInt";
            }
            else if (col.DataType == typeof(Int16))
            {
                DataType = "SmallInt";
            }
            else if (col.DataType == typeof(int))
            {
                DataType = "Int";
            }
            else if (col.DataType == typeof(long))
            {
                DataType = "BigInt";
            }
            else if (col.DataType == typeof(double))
            {
                DataType = "Float";
            }
            else if (col.DataType == typeof(float))
            {
                DataType = "Float";
            }
            else if (col.DataType == typeof(decimal))
            {
                DataType = "Float";
            }
            else if (col.DataType == typeof(DateTime))
            {
                DataType = "DateTime";
            }
            else if (col.DataType == typeof(string))
            {
                DataType = "NVarchar";

                if (col.MaxLength < 0)
                {
                    DataLength = -1;
                }
                else
                {
                    if (col.MaxLength < 4000)
                    {
                        DataLength = col.MaxLength;
                    }
                    else
                    {
                        DataLength = 4000;
                    }
                }
            }
        }


        public TableField(System.Data.DataColumn col, string[] analyzers)
        {
            InitializeComponent();

            _IndexOnly = true;

            comboBoxDataType.Items.Clear();

            FieldName = col.ColumnName;

            foreach (string dataType in DataTypes)
            {
                comboBoxDataType.Items.Add(dataType);
            }

            SetDataType(col);

            foreach (string indexType in IndexTypes)
            {
                comboBoxIndexType.Items.Add(indexType);
            }

            comboBoxIndexType.Text = "None";

            checkBoxPK.Enabled = false;

            foreach (string analyzer in analyzers)
            {
                comboBoxAnalyzer.Items.Add(analyzer);
            }

            comboBoxAnalyzer.Text = "SimpleAnalyzer";
        }

        public TableField(bool indexOnly, string[] analyzers)
        {
            InitializeComponent();

            _IndexOnly = indexOnly;

            comboBoxDataType.Items.Clear();

            foreach (string dataType in DataTypes)
            {
                comboBoxDataType.Items.Add(dataType);
            }

            comboBoxDataType.Text = "Int";

            foreach (string indexType in IndexTypes)
            {
                comboBoxIndexType.Items.Add(indexType);
            }

            comboBoxIndexType.Text = "None";

            checkBoxPK.Enabled = !indexOnly;

            foreach (string analyzer in analyzers)
            {
                comboBoxAnalyzer.Items.Add(analyzer);
            }

            comboBoxAnalyzer.Text = "SimpleAnalyzer";
        }

        public TableField() 
            : this(false, new string[]{})
        {
        }

        public string GetSql()
        {
            StringBuilder sb = new StringBuilder();

            if (FieldName == "")
            {
                throw new Exception("FieldName can't be empty!");
            }

            if (checkBoxNull.Checked && DefaultValue == "" && IndexType != "None")
            {
                switch (comboBoxDataType.Text)
                {
                    case "TinyInt":
                    case "SmallInt":
                    case "Int":
                    case "BigInt":
                    case "Float":
                    case "DateTime":
                    case "SmallDateTime":
                    case "Date":
                        throw new Exception(string.Format("Nullabled Field:{0} must have a default value!", FieldName));
                }
            }

            sb.AppendFormat("{0} {1}", FieldName, DataType);

            switch (DataType)
            {
                case "NVarchar":
                case "Varchar":
                case "NChar":
                case "Char":
                    if (DataLength < 0)
                    {
                        sb.Append("(max) ");
                    }
                    else
                    {
                        sb.AppendFormat("({0}) ", DataLength);
                    }
                    break;
                default:
                    sb.Append(" ");
                    break;
            }

            switch (IndexType)
            {
                case "Tokenized":
                    sb.AppendFormat("Tokenized Analyzer '{0}' ", AnalyzerName.Replace("'", "''"));
                    break;
                case "Untokenized":
                    sb.Append("Untokenized ");
                    break;
                default:
                    break;
            }

            if (IsNull)
            {
                sb.Append("NULL ");
            }
            else
            {
                sb.Append("NOT NULL ");
            }

            if (DefaultValue != "" || (IsNull && IndexType != "None"))
            {
                switch (DataType)
                {
                    case "TinyInt":
                        byte.Parse(DefaultValue);
                        sb.AppendFormat("default {0} ", DefaultValue);
                        break;
                    case "SmallInt":
                        Int16.Parse(DefaultValue);
                        sb.AppendFormat("default {0} ", DefaultValue);
                        break;
                    case "Int":
                        int.Parse(DefaultValue);
                        sb.AppendFormat("default {0} ", DefaultValue);
                        break;

                    case "BigInt":
                        long.Parse(DefaultValue);
                        sb.AppendFormat("default {0} ", DefaultValue);
                        break;

                    case "DateTime":
                    case "SmallDateTime":
                    case "Date":
                        DateTime.Parse(DefaultValue);
                        sb.AppendFormat("default '{0}' ", DefaultValue.Replace("'", "''"));
                        break;

                    case "Float":
                        float.Parse(DefaultValue);
                        sb.AppendFormat("default {0} ", DefaultValue);
                        break;
                    case "NVarchar":
                    case "Varchar":
                    case "NChar":
                    case "Char":
                        sb.AppendFormat("default '{0}' ", DefaultValue.Replace("'", "''"));
                        break;
                    default:
                        break;
                }

                if (IsPK)
                {
                    sb.Append("Primary Key");
                }
            }

            return sb.ToString();

        }



        static string[] DataTypes = 
        {
            "Int", "TinyInt", "SmallInt", "BigInt", "Float", "DateTime", 
            "SmallDateTime", "Date", "NVarchar", "Varchar", "NChar", "Char"
        };

        static string[] IndexTypes = 
        {
            "Tokenized", "Untokenized", "None"
        };

        private void comboBoxDataType_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TableField_Load(object sender, EventArgs e)
        {

        }

        private void comboBoxIndexType_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (checkBoxNull.Checked && DefaultValue == "" && IndexType != "None")
            {
                switch (comboBoxDataType.Text)
                {
                    case "TinyInt":
                    case "SmallInt":
                    case "Int":
                    case "BigInt":
                    case "Float":
                        DefaultValue = "0";
                        break;
                    case "DateTime":
                    case "SmallDateTime":
                    case "Date":
                        DefaultValue = "1900-1-1";
                        break;

                }
            }

            switch (comboBoxDataType.Text)
            {
                case "TinyInt":
                case "SmallInt":
                case "Int":
                case "BigInt":
                case "DateTime":
                case "SmallDateTime":
                case "Date":
                case "Float":
                    numericUpDownDataLength.Minimum = 0;
                    numericUpDownDataLength.Maximum = 0;
                    numericUpDownDataLength.Value = 0;
                    if (IndexType.Equals("Tokenized", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MessageBox.Show("Tokenized field only can be set by string data types", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        comboBoxIndexType.Text = "None";
                        return;
                    }

                    break;
                case "NVarchar":
                case "Varchar":
                case "NChar":
                case "Char":

                    if (IndexType.Equals("Untokenized", StringComparison.CurrentCultureIgnoreCase))
                    {
                        numericUpDownDataLength.Minimum = 1;
                        numericUpDownDataLength.Maximum = 32;
                        numericUpDownDataLength.Value = 1;
                    }
                    else
                    {
                        numericUpDownDataLength.Minimum = -1;
                        numericUpDownDataLength.Maximum = 4000;
                        numericUpDownDataLength.Value = -1;
                    }
                    break;
            }

            if (IndexType.Equals("Tokenized", StringComparison.CurrentCultureIgnoreCase))
            {
                comboBoxAnalyzer.Enabled = true;
            }
            else
            {
                comboBoxAnalyzer.Enabled = false;
            }
        }

        private void checkBoxNull_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNull.Checked && DefaultValue == "" && IndexType != "None")
            {
                switch (comboBoxDataType.Text)
                {
                    case "TinyInt":
                    case "SmallInt":
                    case "Int":
                    case "BigInt":
                    case "Float":
                        DefaultValue = "0";
                        break;
                    case "DateTime":
                    case "SmallDateTime":
                    case "Date":
                        DefaultValue = "1900-1-1";
                        break;

                }
            }
        }


    }
}
