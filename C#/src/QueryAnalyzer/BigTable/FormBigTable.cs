using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hubble.SQLClient;
using Hubble.Core.BigTable;

namespace QueryAnalyzer.BigTable
{
    public partial class FormBigTable : Form
    {
        BigTableGenerate _BigTableGenerate;
        internal string DatabaseName;
        internal DialogResult _Result = DialogResult.Cancel;

        Hubble.Core.BigTable.BigTable _BigTable;

        internal Hubble.Core.BigTable.BigTable BigTable
        {
            get
            {
                return _BigTable;
            }

            set
            {
                _BigTable = value;
            }
        }

        string _TableName;
        internal string TableName
        {
            get
            {
                return _TableName;
            }

            set
            {
                _TableName = value;
            }
        }

        private string _IndexFolder;
        internal string IndexFolder
        {
            get
            {
                return _IndexFolder;
            }

            set
            {
                _IndexFolder = value;
            }
        }

        bool _CreateTable;

        internal bool CreateTable
        {
            get
            {
                return _CreateTable;
            }

            set
            {
                _CreateTable = value;
            }
        }

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
            _BigTableGenerate.BigTableInfo = this.BigTable;
            _BigTableGenerate.CreateTable = this.CreateTable;
            _BigTableGenerate.TableName = this.TableName;
            _BigTableGenerate.IndexFolder = this.IndexFolder;
            _BigTableGenerate.DatabaseName = DatabaseName;
            _BigTableGenerate.Parent = this;
            _BigTableGenerate.Visible = true;
            _BigTableGenerate.Dock = DockStyle.Fill;


            if (this.CreateTable)
            {
                try
                {
                    QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_GetDatabaseAttributes {0}",
                        DatabaseName);

                    foreach (Hubble.Framework.Data.DataRow row in queryResult.DataSet.Tables[0].Rows)
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

        private void FormBigTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_BigTableGenerate.SettingChanged)
            {
                if (QAMessageBox.ShowQuestionMessage("Setting changed, do you want to save the setting?") == DialogResult.Yes)
                {
                    if (!_BigTableGenerate.Save())
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}
