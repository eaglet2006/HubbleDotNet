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
    public partial class FormCreateDatabase : Form
    {
        DialogResult _Result = DialogResult.Cancel;

        List<string> _DBAdapterList = new List<string>();

        public List<string> DBAdapterList
        {
            get
            {
                return _DBAdapterList;
            }
        }

        public FormCreateDatabase()
        {
            InitializeComponent();
        }

        new public DialogResult ShowDialog()
        {
            base.ShowDialog();

            return _Result;
        }

        private void FormCreateDatabase_Load(object sender, EventArgs e)
        {
            comboBoxDBAdapter.Items.Clear();

            foreach (string dbAdapter in DBAdapterList)
            {
                comboBoxDBAdapter.Items.Add(dbAdapter);
            }
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            string databaseName = textBoxDatabaseName.Text.Trim();

            if (databaseName == "")
            {
                MessageBox.Show("Can't use empty database name!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (textBoxDefIndexFolder.Text.Trim() == "")
            {
                MessageBox.Show("Can't use empty Default index folder!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBoxDBAdapter.Text.Trim() == "")
            {
                MessageBox.Show("Can't use empty Default DBAdapter!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (textBoxDefConnectionString.Text.Trim() == "")
            {
                MessageBox.Show("Can't use empty Default connection string!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                GlobalSetting.DataAccess.Excute("exec sp_adddatabase {0}, {1}, {2}, {3}",
                    databaseName, textBoxDefIndexFolder.Text.Trim(), 
                    comboBoxDBAdapter.Text.Trim(),
                    textBoxDefConnectionString.Text.Trim());

                _Result = DialogResult.OK;

                Close();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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

                if (textBoxDefConnectionString.Text.Trim() == "")
                {
                    MessageBox.Show("Can't use empty Default connection string!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                GlobalSetting.DataAccess.Excute("exec sp_TestConnectionString {0}, {1}",
                        comboBoxDBAdapter.Text.Trim(),
                        textBoxDefConnectionString.Text.Trim());
                MessageBox.Show("Connect successful!", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
