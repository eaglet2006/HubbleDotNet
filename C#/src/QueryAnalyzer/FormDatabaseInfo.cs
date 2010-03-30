using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormDatabaseInfo : Form
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

        public string DatabaseName
        {
            get
            {
                return textBoxDatabaseName.Text;
            }

            set
            {
                textBoxDatabaseName.Text = value;
            }
        }

        string _OriginalDefaultIndexFolder;

        public string DefaultIndexFolder
        {
            get
            {
                return textBoxDefIndexFolder.Text;
            }

            set
            {
                _OriginalDefaultIndexFolder = value;
                textBoxDefIndexFolder.Text = value;
            }
        }

        string _OriginalDefaultDBAdapter;

        public string DefaultDBAdapter
        {
            get
            {
                return comboBoxDBAdapter.Text;
            }

            set
            {
                _OriginalDefaultDBAdapter = value;
                comboBoxDBAdapter.Text = value;
            }
        }

        string _OriginalDefaultConnectionString;
        public string DefaultConnectionString
        {
            get
            {
                return textBoxDefConnectionString.Text;
            }

            set
            {
                _OriginalDefaultConnectionString = value;
                textBoxDefConnectionString.Text = value;
            }
        }

        public FormDatabaseInfo()
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

        private void buttonChange_Click(object sender, EventArgs e)
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
                if (_OriginalDefaultIndexFolder != textBoxDefIndexFolder.Text)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_SetDatabaseAttribute {0}, {1}, {2}",
                        databaseName, "DefaultPath", textBoxDefIndexFolder.Text.Trim());

                    _Result = DialogResult.OK;
                }

                if (_OriginalDefaultDBAdapter != comboBoxDBAdapter.Text)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_SetDatabaseAttribute {0}, {1}, {2}",
                        databaseName, "DefaultDBAdapter", comboBoxDBAdapter.Text.Trim());

                    _Result = DialogResult.OK;
                }


                if (_OriginalDefaultConnectionString != textBoxDefConnectionString.Text)
                {
                    GlobalSetting.DataAccess.Excute("exec SP_SetDatabaseAttribute {0}, {1}, {2}",
                        databaseName, "DefaultConnectionString", textBoxDefConnectionString.Text.Trim());
                    _Result = DialogResult.OK;

                }

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
