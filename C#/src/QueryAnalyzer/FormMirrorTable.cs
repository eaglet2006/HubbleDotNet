using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormMirrorTable : Form
    {
        DialogResult _Result = DialogResult.Cancel;

        public FormMirrorTable()
        {
            InitializeComponent();
        }

        public string TableName
        {
            get
            {
                return textBoxTableName.Text.Trim();
            }
        }

        public string ConnectionString
        {
            get
            {
                return textBoxConnectionString.Text.Trim();
            }
        }

        public string DBAdapter
        {
            get
            {
                return comboBoxDBAdapter.Text.Trim();
            }
        }


        public string SqlForCreate
        {
            get
            {
                return textBoxSQLForCreate.Text.Trim();
            }
        }

        new public DialogResult ShowDialog()
        {
            base.ShowDialog();
            return _Result;
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            _Result = DialogResult.OK;
            Close();
        }

        private void FormMirrorTable_Load(object sender, EventArgs e)
        {
        }

        private void comboBoxDBAdapter_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonTestConnectionString_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxDBAdapter.Text.Trim() == "")
                {
                    MessageBox.Show("Can't use empty DBAdapter!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (textBoxConnectionString.Text.Trim() == "")
                {
                    MessageBox.Show("Can't use empty connection string!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


    }
}
