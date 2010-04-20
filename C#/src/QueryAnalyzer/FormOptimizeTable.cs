using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Hubble.SQLClient;

namespace QueryAnalyzer
{
    public partial class FormOptimizeTable : Form
    {
        Thread _Thread;
        string _TableName;
        bool _CanClosed = false;

        public FormOptimizeTable()
        {
            InitializeComponent();
        }

        private void FormOptimizeTable_Load(object sender, EventArgs e)
        {

        }

        public void ShowDialog(string tableName)
        {
            _TableName = tableName;
            _Thread = new Thread(ShowMergeRate);
            _Thread.IsBackground = true;
            _Thread.Start();

            base.ShowDialog();
        }

        delegate void DelegateShowProgress(double rate);

        private void ShowProgress(double rate)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateShowProgress(ShowProgress), rate);
            }
            else
            {
                this.progressBar.Value = (int)rate;

                if (rate >= 100)
                {
                    this.buttonFinished.Enabled = true;
                }
            }
        }



        private void ShowMergeRate()
        {
            try
            {
                GlobalSetting.DataAccess.Excute("exec SP_OptimizeTable {0}", _TableName);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowProgress(100);
                _CanClosed = true;
                return;
            }

            int rate100Times = 0;

            while (true)
            {
                Thread.Sleep(2000);

                try
                {
                    QueryResult queryResult = GlobalSetting.DataAccess.Excute("exec SP_GetTableMergeRate {0}", _TableName);

                    double totalRate = 0;

                    foreach (DataRow row in queryResult.DataSet.Tables[0].Rows)
                    {
                        double rate = double.Parse(row["Rate"].ToString());

                        if (rate < 0)
                        {
                            rate = 1;
                        }

                        totalRate += rate * 100;
                    }

                    double progress = totalRate / queryResult.DataSet.Tables[0].Rows.Count;

                    if (progress >= 100)
                    {
                        progress = 90;
                        rate100Times++;
                    }

                    if (rate100Times > 1)
                    {
                        ShowProgress(100);
                        break;
                    }
                    else
                    {
                        ShowProgress(progress);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowProgress(100);
                    break;
                }
            }

            _CanClosed = true;
        }

        private void FormOptimizeTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !_CanClosed;
        }

        private void buttonFinished_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
