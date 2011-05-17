using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Hubble.Core.BigTable;
using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class BigTableGenerate : UserControl
    {
        FormBigTable _ParentForm;

        public string DefaultIndexFolder;

        public string DatabaseName;

        public string TableName
        {
            get
            {
                return textBoxTableName.Text.Trim();
            }

            set
            {
                textBoxTableName.Text = value;
            }
        }

        public string IndexFolder
        {
            get
            {
                return textBoxIndexFolder.Text.Trim();
            }

            set
            {
                textBoxIndexFolder.Text = value;
            }
        }

        bool _CreateTable;

        public bool CreateTable
        {
            get
            {
                return _CreateTable;
            }

            set
            {
                _CreateTable = value;
                textBoxTableName.Enabled = value;
                textBoxIndexFolder.Enabled = value;
            }
        }

        private BigTable _BigTableInfo;
        public BigTable BigTableInfo
        {
            get
            {
                if (_BigTableInfo == null)
                {
                    _BigTableInfo = new BigTable();
                }
                
                return _BigTableInfo;
            }

            set
            {
                _BigTableInfo = value;
            }
        }

        public BigTableGenerate()
        {
            InitializeComponent();
        }

        private void RefreshBalanceServerList(TabletInfo tablet)
        {
            listBoxBlanceServer.Items.Clear();
            foreach (string connectionString in tablet.BalanceServers)
            {
                listBoxBlanceServer.Items.Add(connectionString);
            }
        }


        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string tableName = null;
            string connectionString = "";

            try
            {
                if (QAMessageBox.ShowInputBox("Add tablet", "Input table name", ref tableName) == DialogResult.OK)
                {
                    TabletInfo tablet = new TabletInfo(tableName, connectionString);
                    BigTableInfo.Add(tablet);

                    listBoxTablets.Items.Add(tablet);
                    RefreshBalanceServerList(tablet);

                    //if (QAMessageBox.ShowInputBox("Add tablet", "Input connection string name", ref tableName) == DialogResult.OK)
                    //{
                    //    TabletInfo tabletInfo = new TabletInfo(tableName, connectionString);
                    //}
                }
            }
            catch (Exception ex)
            {
                QAMessageBox.ShowErrorMessage(ex.Message);
            }

            //tableCollection.T
            //_BigTableInfo.TableCollectionList.Add(new TableCollection());
        }

        private void BigTableGenerate_Load(object sender, EventArgs e)
        {
            _ParentForm = this.Parent as FormBigTable;
         

#if HubblePro
            groupBoxBalanceServers.Enabled = false;
#else

            groupBoxBalanceServers.Enabled = false;
#endif

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _ParentForm.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            IndexFolder = IndexFolder.Trim();
            TableName = TableName.Trim();
            if (IndexFolder == "")
            {
                QAMessageBox.ShowErrorMessage("Index folder can't be empty.");
            }

            if (TableName == "")
            {
                QAMessageBox.ShowErrorMessage("Table name can't be empty.");
            }

            try
            {
                string xml;
                Hubble.Framework.IO.Stream.ReadStreamToString(Hubble.Framework.Serialization.XmlSerialization<BigTable>.Serialize(BigTableInfo),
                    out xml, Encoding.UTF8);

                QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_CreateBigTable {0}, {1}, {2}",
                    TableName, IndexFolder, xml);

                _ParentForm._Result = DialogResult.OK;
                _ParentForm.Close();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }  
        }

        private void textBoxTableName_TextChanged(object sender, EventArgs e)
        {
            if (textBoxIndexFolder.Text.IndexOf(DefaultIndexFolder, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                textBoxIndexFolder.Text = Hubble.Framework.IO.Path.AppendDivision(
                    Hubble.Framework.IO.Path.AppendDivision(DefaultIndexFolder, '\\') +
                    textBoxTableName.Text, '\\');

            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            TabletInfo tablet = listBoxTablets.SelectedItem as TabletInfo;

            if (tablet != null)
            {
                listBoxTablets.Items.Remove(tablet);
                BigTableInfo.Tablets.Remove(tablet);
            }
        }
    }
}
