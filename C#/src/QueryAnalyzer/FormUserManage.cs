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
    public partial class FormUserManage : Form
    {
        public FormUserManage()
        {
            InitializeComponent();
        }

        private void addUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAddUser frmAddUser = new FormAddUser();
            if (frmAddUser.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GlobalSetting.DataAccess.Excute("exec SP_CreateUser {0}, {1}, 2147483647",
                        frmAddUser.UserName, frmAddUser.Password);

                    RefreshUserList();

                    listBoxUsers.SelectedItem = frmAddUser.UserName;


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }


        }

        private void FormUserManage_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void RefreshUserList()
        {
            try
            {
                QueryResult qResult = GlobalSetting.DataAccess.Excute("exec SP_UserList");

                listBoxUsers.Items.Clear();

                foreach (System.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    listBoxUsers.Items.Add(row["UserName"]);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void FormUserManage_Load(object sender, EventArgs e)
        {
            RefreshUserList();
        }
    }
}
