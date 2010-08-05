using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormAttachTable : Form
    {
        DialogResult _Result = DialogResult.Cancel;

        public FormAttachTable()
        {
            InitializeComponent();
        }

        public new DialogResult ShowDialog()
        {
            base.ShowDialog();

            return _Result;
        }

        private void checkBoxTableName_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTableName.Enabled = checkBoxTableName.Checked; 
        }

        private void checkBoxConnectString_CheckedChanged(object sender, EventArgs e)
        {
            textBoxConnectString.Enabled = checkBoxConnectString.Checked;
            checkBoxTableName.Checked = checkBoxConnectString.Checked;
        }

        private void checkBoxDBTableName_CheckedChanged(object sender, EventArgs e)
        {
            textBoxDBTableName.Enabled = checkBoxDBTableName.Checked;
            checkBoxConnectString.Checked = checkBoxDBTableName.Checked;
        }

        private void buttonAttach_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sql = new StringBuilder();

                if (checkBoxDBTableName.Checked)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_AttachTable {0}, {1}, {2}, {3}",
                        textBoxDirectory.Text.Trim(), textBoxTableName.Text.Trim(),
                        textBoxConnectString.Text.Trim(), textBoxDBTableName.Text.Trim());
                }
                else if (checkBoxConnectString.Checked)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_AttachTable {0}, {1}, {2}",
                        textBoxDirectory.Text.Trim(), textBoxTableName.Text.Trim(),
                        textBoxConnectString.Text.Trim());
                }
                else if (checkBoxTableName.Checked)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_AttachTable {0}, {1}",
                        textBoxDirectory.Text.Trim(), textBoxTableName.Text.Trim());
                }
                else
                {
                    GlobalSetting.DataAccess.Excute("exec SP_AttachTable {0}",
                        textBoxDirectory.Text.Trim());
                }

                _Result = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
