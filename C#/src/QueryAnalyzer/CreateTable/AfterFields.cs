using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer.CreateTable
{
    class AfterFields : IAfter
    {
        #region IAfter Members

        public void Do(FormCreateTable frmCreateTable)
        {
            if (frmCreateTable.radioButtonCreateTableFromExistTable.Checked)
            {
                frmCreateTable.CheckDocIdReplaceField();

                string prompt;
                if (frmCreateTable.radioButtonAppendOnly.Checked)
                {
                    prompt = "You should make sure that docid field has unique index and can be increased automaticly.";
                }
                else
                {
                    prompt = "You should make sure that ID field has unique index.";
                }

                MessageBox.Show(prompt, "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }

            frmCreateTable.textBoxScript.Text = frmCreateTable.GetCreateTableSql();
        }

        #endregion
    }
}
