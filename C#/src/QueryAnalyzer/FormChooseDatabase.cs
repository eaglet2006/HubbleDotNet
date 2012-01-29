using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Hubble.Framework.Data;
using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class FormChooseDatabase : Form
    {
        DialogResult _Result = DialogResult.Cancel;

        public string DatabaseName
        {
            get
            {
                return comboBoxDatabaseName.Text.Trim();
            }
        }

        public FormChooseDatabase()
        {
            InitializeComponent();
        }

        private IEnumerable<string> GetDatabases()
        {
            QueryResult result = GlobalSetting.DataAccess.Excute("exec sp_databaselist");

            foreach (DataRow row in result.DataSet.Tables[0].Rows)
            {
                yield return row["DatabaseName"].ToString();
            }
        }

        public DialogResult ShowDialog(ListBox listBoxDatabase)
        {
            comboBoxDatabaseName.Items.Clear();

            foreach (string database in GetDatabases())
            {
                bool exist = false;

                foreach (DatabaseRight dbRight in listBoxDatabase.Items)
                {
                    if (database.Equals(dbRight.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    comboBoxDatabaseName.Items.Add(database);
                }
            }

            base.ShowDialog();

            return _Result;
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            if (comboBoxDatabaseName.Text.Trim() != "")
            {
                _Result = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
