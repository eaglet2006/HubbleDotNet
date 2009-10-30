using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

                tableField.labelFieldName.Text = row["FieldName"].ToString();
                tableField.comboBoxDataType.Text = row["DataType"].ToString();
                tableField.comboBoxIndexType.Text = row["IndexType"].ToString();
                tableField.numericUpDownDataLength.Value = int.Parse(row["DataLength"].ToString());
                tableField.comboBoxAnalyzer.Text = row["Analyzer"].ToString();
                tableField.checkBoxNull.Checked = bool.Parse(row["IsNull"].ToString());
                tableField.checkBoxPK.Checked = bool.Parse(row["IsPrimaryKey"].ToString());
                tableField.textBoxDefault.Text = row["Default"].ToString();
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
