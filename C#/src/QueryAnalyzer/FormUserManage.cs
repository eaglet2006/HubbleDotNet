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
        List<DatabaseRight> _CurDatabaseRightList = null;

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

                    if (listBoxUsers.Items.Count == 0)
                    {
                        MessageBox.Show("User account initiated, you should restart QueryAnalyzer", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }

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

        private void deleteUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem == null)
            {
                return;
            }

            if (MessageBox.Show(string.Format("Are you show you want to remove user:{0}",
                listBoxUsers.SelectedItem), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string userName = (string)listBoxUsers.SelectedItem;
                    GlobalSetting.DataAccess.Excute("exec SP_RemoveUser {0}",
                        userName);

                    RefreshUserList();

                    if (listBoxUsers.Items.Count > 0)
                    {
                        listBoxUsers.SelectedIndex = 0;
                    }

                    if (GlobalSetting.DataAccess.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        MessageBox.Show("Delete current user itself, you should restart QueryAnalyzer", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
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

        private List<DatabaseRight> GetDatabaseRightFromGUI()
        {
            List<DatabaseRight> result = new List<DatabaseRight>();

            result.Add(new DatabaseRight("", GetRight(groupBoxSystemRight)));

            foreach (DatabaseRight dbRight in listBoxDatabase.Items)
            {
                result.Add(dbRight);
            }

            return result;
        }

        int GetRight(GroupBox groupBox)
        {
            int result = 0;

            foreach (Control control in groupBox.Controls)
            {
                if (control is CheckBox)
                {
                    string hex = control.Tag.ToString();
                    int index = hex.IndexOf("0x", 0, StringComparison.CurrentCultureIgnoreCase);

                    if (index >= 0)
                    {
                        hex = hex.Substring(index + 2, hex.Length - (index + 2));
                    }

                    int rightItem = int.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);

                    CheckBox checkBox = (control as CheckBox);
                    if (checkBox.Checked)
                    {
                        result |= rightItem;
                    }
                }
            }

            return result;
        }

        void SetRight(GroupBox groupBox, int right)
        {
            SetRight(groupBox, right, false);
        }

        void SetRight(GroupBox groupBox, int right, bool databaseRight)
        {
            foreach (Control control in groupBox.Controls)
            {
                if (control is CheckBox)
                {
                    string hex = control.Tag.ToString();
                    int index = hex.IndexOf("0x", 0, StringComparison.CurrentCultureIgnoreCase);

                    if (index >= 0)
                    {
                        hex = hex.Substring(index + 2, hex.Length - (index + 2));
                    }

                    int rightItem = int.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);

                    CheckBox checkBox = (control as CheckBox);
                    checkBox.Checked = (right & rightItem) != 0;

                    if (databaseRight)
                    {
                        bool sysCtrlChecked = false;

                        foreach (Control sysCtrl in groupBoxSystemRight.Controls)
                        {
                            CheckBox sysCheckBox = sysCtrl as CheckBox;

                            if (sysCheckBox != null)
                            {
                                string sysHex = sysCtrl.Tag.ToString();
                                int sysIndex = sysHex.IndexOf("0x", 0, StringComparison.CurrentCultureIgnoreCase);
                                if (sysIndex >= 0)
                                {
                                    sysHex = sysHex.Substring(sysIndex + 2, sysHex.Length - (sysIndex + 2));
                                }

                                int sysRightItem = int.Parse(sysHex, System.Globalization.NumberStyles.AllowHexSpecifier);

                                if (sysRightItem == rightItem && sysCheckBox.Checked)
                                {
                                    checkBox.Checked = true;
                                    checkBox.Enabled = false;
                                    sysCtrlChecked = true;
                                }
                            }
                        }

                        if (!sysCtrlChecked)
                        {
                            checkBox.Enabled = true;
                        }
                    }

                }
            }



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

        private List<DatabaseRight> GetDatabaseRightList(string userName)
        {
            try
            {
                List<DatabaseRight> result = new List<DatabaseRight>();

                QueryResult qResult = GlobalSetting.DataAccess.Excute("exec SP_GetUserRights {0}", 
                    userName);

                foreach (System.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    result.Add(new DatabaseRight(row["Database"].ToString(), int.Parse(row["Right"].ToString())));
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private int GetCurSystemRight()
        {
            if (_CurDatabaseRightList == null)
            {
                return 0;
            }

            foreach (DatabaseRight dbRight in _CurDatabaseRightList)
            {
                if (string.IsNullOrEmpty(dbRight.DatabaseName))
                {
                    return dbRight.Right;
                }
            }

            return 0;
        }

        private void listBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem == null)
            {
                return;
            }

            string userName = listBoxUsers.SelectedItem.ToString();

            _CurDatabaseRightList = GetDatabaseRightList(userName);

            if (_CurDatabaseRightList == null)
            {
                Close();
                return;
            }

            SetRight(groupBoxSystemRight, GetCurSystemRight());

            listBoxDatabase.Items.Clear();

            if (_CurDatabaseRightList == null)
            {
                return;
            }

            foreach (DatabaseRight dbRight in _CurDatabaseRightList)
            {
                if (string.IsNullOrEmpty(dbRight.DatabaseName))
                {
                    continue;
                }

                listBoxDatabase.Items.Add(dbRight);
            }

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                string userName = listBoxUsers.SelectedItem.ToString();

                List<DatabaseRight> dbRightList = GetDatabaseRightFromGUI();

                List<DatabaseRight> deleteRightList = new List<DatabaseRight>();


                foreach (DatabaseRight dbRight in _CurDatabaseRightList)
                {
                    if (!dbRightList.Contains(dbRight))
                    {
                        deleteRightList.Add(dbRight);
                    }
                }

                foreach (DatabaseRight dbRight in deleteRightList)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_RemoveUserRight {0},{1}",
                        userName, dbRight.DatabaseName);
                }

                foreach (DatabaseRight dbRight in dbRightList)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_SetUserRight {0},{1},{2}",
                        userName, dbRight.DatabaseName, dbRight.Right);
                }

                _CurDatabaseRightList = dbRightList;

                MessageBox.Show("Save successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void checkBoxDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (listBoxDatabase.SelectedItem != null)
            {
                ((DatabaseRight)listBoxDatabase.SelectedItem).Right =
                    GetRight(groupBoxDatabaseRight);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormChooseDatabase frmChooseDatabase = new FormChooseDatabase();
            if (frmChooseDatabase.ShowDialog(listBoxDatabase) == DialogResult.OK)
            {
                listBoxDatabase.Items.Add(new DatabaseRight(frmChooseDatabase.DatabaseName, 0));
                listBoxDatabase.SelectedIndex = listBoxDatabase.Items.Count - 1;
            }
        }

        private void listBoxDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDatabase.SelectedItem != null)
            {
                SetRight(groupBoxDatabaseRight, ((DatabaseRight)listBoxDatabase.SelectedItem).Right, true);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBoxDatabase.SelectedItem != null)
            {
                string databaseName = ((DatabaseRight)listBoxDatabase.SelectedItem).DatabaseName;

                if (MessageBox.Show(string.Format("Are you sure to delete the right of database: {0}",
                    databaseName), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.Yes)
                {
                    listBoxDatabase.Items.Remove(listBoxDatabase.SelectedItem);

                    if (listBoxDatabase.Items.Count > 0)
                    {
                        listBoxDatabase.SelectedIndex = 0;
                    }
                }
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tabControlRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlRight.SelectedTab == tabPageDatabase)
            {
                if (listBoxDatabase.Items.Count > 0)
                {
                    if (listBoxDatabase.SelectedIndex < 0)
                    {
                        listBoxDatabase.SelectedIndex = 0;
                    }
                    else
                    {
                        listBoxDatabase_SelectedIndexChanged(sender, e);
                    }
                }
            }
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem == null)
            {
                return;
            }

            string userName = listBoxUsers.SelectedItem.ToString();
            FormAddUser frmAddUser = new FormAddUser();
            if (frmAddUser.ShowDialog(userName) == DialogResult.OK)
            {
                try
                {
                    GlobalSetting.DataAccess.Excute("exec SP_ChangePassword {0}, {1}",
                        userName, frmAddUser.Password);
                    if (GlobalSetting.DataAccess.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        GlobalSetting.DataAccess.Password = frmAddUser.Password;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
        }

    }
}
