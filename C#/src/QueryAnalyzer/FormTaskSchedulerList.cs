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
    public partial class FormTaskSchedulerList : Form
    {
        class SchemaInfo
        {
            internal int SchemaId;
            internal string Name;
            internal string Description;
            internal string State;

            public SchemaInfo(int schemaId, string name, string description, string state)
            {
                SchemaId = schemaId;
                Name = name;
                Description = description;
                State = state;
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public FormTaskSchedulerList()
        {
            InitializeComponent();
        }

        private void listBoxTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTasks.SelectedIndex >= 0)
            {
                buttonUpdate.Enabled = true;
                buttonDelete.Enabled = true;
                SchemaInfo schemaInfo = listBoxTasks.SelectedItem as SchemaInfo;
                textBoxDescription.Text = schemaInfo.Description;
                labelState.Text = schemaInfo.State;
            }
            else
            {
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                textBoxDescription.Text = "";
                labelState.Text = "Disable";
            }
        }

        private void FormTaskSchedulerList_Load(object sender, EventArgs e)
        {
            buttonUpdate.Enabled = false;
            buttonDelete.Enabled = false;

            List();
        }

        private void List()
        {
            try
            {
                QueryResult qResult = GlobalSetting.DataAccess.Excute("exec SP_SchemaList");

                listBoxTasks.Items.Clear();

                foreach (Hubble.Framework.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
                {
                    listBoxTasks.Items.Add(new SchemaInfo(int.Parse(row["SchemaId"].ToString()),
                        row["Name"].ToString(), row["Description"].ToString(), row["State"].ToString()));
                }

                if (listBoxTasks.Items.Count > 0)
                {
                    listBoxTasks.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            TaskManage.FrmCreateSchema frmCreateSchema = new TaskManage.FrmCreateSchema();
            frmCreateSchema.Text = "New task scheduler";
                 
            if (frmCreateSchema.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string xmlStr = frmCreateSchema.Schema.ConvertToString();
                    GlobalSetting.DataAccess.Excute("exec SP_AddSchema {0}", xmlStr);
                    List();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBoxTasks.SelectedItem == null)
            {
                return;
            }

            SchemaInfo schemaInfo = listBoxTasks.SelectedItem as SchemaInfo;

            if (MessageBox.Show(string.Format("Are you show you want to remove task:{0}",
                schemaInfo.Name), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    GlobalSetting.DataAccess.Excute("exec SP_RemoveSchema {0}", new object[] { schemaInfo.SchemaId });
                    List();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (listBoxTasks.SelectedItem == null)
            {
                return;
            }

            SchemaInfo schemaInfo = listBoxTasks.SelectedItem as SchemaInfo;
            TaskManage.Schema schema = null;

            try
            {
                QueryResult qResult = GlobalSetting.DataAccess.Excute("exec SP_GetSchema {0}", new object[]{schemaInfo.SchemaId});

                if (qResult.DataSet.Tables[0].Rows.Count > 0)
                {
                    string xmlStr = qResult.DataSet.Tables[0].Rows[0]["Schema"].ToString();
                    schema = TaskManage.Schema.ConvertFromString(xmlStr);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TaskManage.FrmCreateSchema frmCreateSchema = new TaskManage.FrmCreateSchema();
            frmCreateSchema.Text = "Update task scheduler";
            frmCreateSchema.SchemaId = schemaInfo.SchemaId;
            frmCreateSchema.Schema = schema;

            if (frmCreateSchema.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GlobalSetting.DataAccess.Excute("exec SP_RemoveSchema {0}", new object[] { schemaInfo.SchemaId });

                    string xmlStr = frmCreateSchema.Schema.ConvertToString();

                    GlobalSetting.DataAccess.Excute("exec SP_AddSchema {0}", xmlStr);
                    
                    List();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
