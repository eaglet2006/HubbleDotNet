using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormAddUser : Form
    {
        DialogResult result = DialogResult.Cancel;

        public FormAddUser()
        {
            InitializeComponent();
        }

        public string UserName
        {
            get
            {
                return textBoxUserName.Text.Trim();
            }
        }

        public string Password
        {
            get
            {
                return textBoxPassword.Text.Trim();
            }
        }

        public new DialogResult ShowDialog()
        {
            base.ShowDialog();
            return result;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxUserName.Text.Trim() == "")
            {
                MessageBox.Show("User name can't be empty!",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (textBoxPassword.Text.Trim() != textBoxConfirm.Text.Trim())
            {
                MessageBox.Show("Please check your password; the confirmation entry does not match.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            result = DialogResult.OK;

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
