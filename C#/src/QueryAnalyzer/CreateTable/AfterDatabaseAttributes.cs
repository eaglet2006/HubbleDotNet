using System;
using System.Collections.Generic;
using System.Text;

namespace QueryAnalyzer.CreateTable
{
    class AfterDatabaseAttributes: IAfter
    {
        #region IAfter Members

        public void Do(FormCreateTable frmCreateTable)
        {
            if (frmCreateTable.textBoxTableName.Text.Trim() == "")
            {
                throw new Exception("Table Name can't be empty!");
            }

            if (frmCreateTable.textBoxIndexFolder.Text.Trim() == "")
            {
                throw new Exception("Index folder can't be empty!");
            }

            if (frmCreateTable.comboBoxDBAdapter.Text.Trim() == "")
            {
                throw new Exception("DB Adapter can't be empty!");
            }

            if (frmCreateTable.textBoxConnectionString.Text.Trim() == "")
            {
                throw new Exception("Connection String can't be empty!");
            }

            frmCreateTable.textBoxDBTableName.Text = frmCreateTable.textBoxTableName.Text;
        }

        #endregion
    }
}
