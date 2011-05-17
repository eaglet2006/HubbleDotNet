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
    public partial class FormBigTable : Form
    {
        BigTableGenerate _BigTableGenerate;
        internal string DatabaseName;
        internal DialogResult _Result = DialogResult.Cancel;

        public FormBigTable()
        {
            InitializeComponent();
        }

        public new DialogResult ShowDialog()
        {
            base.ShowDialog();
            return _Result;
        }

        private void FormBigTable_Load(object sender, EventArgs e)
        {
            _BigTableGenerate = new BigTableGenerate();
            _BigTableGenerate.Parent = this;
            _BigTableGenerate.Visible = true;
            _BigTableGenerate.Dock = DockStyle.Fill;
            _BigTableGenerate.DatabaseName = DatabaseName;

            try
            {
                QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_GetDatabaseAttributes {0}",
                    DatabaseName);

                foreach (System.Data.DataRow row in queryResult.DataSet.Tables[0].Rows)
                {
                    if (row["Attribute"].ToString().Trim().Equals("DefaultPath"))
                    {
                        _BigTableGenerate.IndexFolder = row["Value"].ToString().Trim();
                        _BigTableGenerate.DefaultIndexFolder = _BigTableGenerate.IndexFolder;
                        break;
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }   
        }
    }
}
