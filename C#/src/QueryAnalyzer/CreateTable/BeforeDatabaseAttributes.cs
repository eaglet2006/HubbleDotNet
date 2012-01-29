using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using Hubble.SQLClient;

namespace QueryAnalyzer.CreateTable
{
    class BeforeDatabaseAttributes: IBefore
    {
        #region IBefore Members

        public void Do(FormCreateTable frmCreateTable)
        {
            try
            {
                string databaseName = frmCreateTable.DatabaseName;

                QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_DBAdapterList");

                frmCreateTable.comboBoxDBAdapter.Items.Clear();

                foreach (Hubble.Framework.Data.DataRow row in queryResult.DataSet.Tables[0].Rows)
                {
                    frmCreateTable.comboBoxDBAdapter.Items.Add(row["Name"].ToString());
                }

                queryResult = GlobalSetting.DataAccess.Excute("exec SP_GetDatabaseAttributes {0}",
                    databaseName);

                foreach (Hubble.Framework.Data.DataRow row in queryResult.DataSet.Tables[0].Rows)
                {
                    if (row["Attribute"].ToString().Trim().Equals("DefaultPath"))
                    {
                        frmCreateTable.textBoxIndexFolder.Text = row["Value"].ToString().Trim();
                        frmCreateTable.DefaultIndexFolder = frmCreateTable.textBoxIndexFolder.Text;
                    }
                    else if (row["Attribute"].ToString().Trim().Equals("DefaultDBAdapter"))
                    {
                        frmCreateTable.comboBoxDBAdapter.Text = row["Value"].ToString().Trim();
                    }
                    else if (row["Attribute"].ToString().Trim().Equals("DefaultConnectionString"))
                    {
                        frmCreateTable.textBoxConnectionString.Text = row["Value"].ToString().Trim();
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
