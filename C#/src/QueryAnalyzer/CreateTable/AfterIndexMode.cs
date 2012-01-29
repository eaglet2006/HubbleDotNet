using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Framework.Data;

namespace QueryAnalyzer.CreateTable
{
    class AfterIndexMode : IAfter
    {
        #region IAfter Members

        public void Do(FormCreateTable frmCreateTable)
        {
            if (frmCreateTable.radioButtonCreateTableFromExistTable.Checked)
            {
                Hubble.SQLClient.QueryResult qResult = GlobalSetting.DataAccess.Excute(
                    "exec SP_GetTableSchema {0}, {1}, {2}", frmCreateTable.comboBoxDBAdapter.Text,
                    frmCreateTable.textBoxConnectionString.Text, frmCreateTable.textBoxDBTableName.Text);

                frmCreateTable.ClearAllTableFields();

                bool hasDocIdField = false;

                foreach (DataColumn col in qResult.DataSet.Tables[0].Columns)
                {
                    if (col.ColumnName.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        hasDocIdField = true;
                        if (!frmCreateTable.radioButtonAll.Checked)
                        {
                            continue;
                        }
                    }

                    frmCreateTable.AddTableField(col);
                }

                if (frmCreateTable.radioButtonAppendOnly.Checked)
                {
                    if (!hasDocIdField)
                    {
                        frmCreateTable.ClearAllTableFields();
                        throw new Exception("Append only mode must have a int data type field named docid!");
                    }
                }
                else
                {
                    if (hasDocIdField)
                    {
                        frmCreateTable.ClearAllTableFields();
                        throw new Exception("This mode can't have field named docid!");
                    }
                }

                frmCreateTable.ShowTableField();
            }
        }

        #endregion
    }
}
