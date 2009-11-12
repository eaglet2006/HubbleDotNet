using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormAddDatabase : Form
    {
        public FormAddDatabase()
        {
            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string databaseName = textBoxDatabaseName.Text.Trim();

            if (databaseName == "")
            {
                MessageBox.Show("Can't use empty database name!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                GlobalSetting.DataAccess.Excute("exec sp_adddatabase {0}", databaseName);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
    }
}
