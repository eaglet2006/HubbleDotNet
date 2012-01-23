using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Hubble.SQLClient;
using Hubble.Core.DBAdapter;

namespace QueryAnalyzer
{
    public partial class FormRebuildTable : Form
    {
        internal DbAccess DataAccess { get; set; }

        TableSynchronization _TableSync;
        string _TableName;

        System.Threading.Thread _Thread = null;

        DateTime _StartTime;

        public string TableName
        {
            get
            {
                return _TableName;
            }

            set
            {
                _TableName = value;
                labelTableName.Text = value;
            }
        }

        public FormRebuildTable()
        {
            InitializeComponent();
        }

        delegate void DelegateShowException(Exception e);
        private void ShowException(Exception e)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new DelegateShowException(ShowException), e);
            }
            else
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        delegate void DelegateShowOptimizeProgress(double progress, int insertRows);

        private void ShowOptimizeProgress(double progress, int insertRows)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new DelegateShowOptimizeProgress(ShowOptimizeProgress), progress, insertRows);
            }
            else
            {
                labelInsertRows.Text = insertRows.ToString();

                TimeSpan ts = DateTime.Now - _StartTime;

                labelElapse.Text = string.Format("{0:0.00} s", ts.TotalMilliseconds / 1000);

                if (progress >= 100 || progress < 0)
                {
                    progressBar.Value = 100;

                    buttonStart.Enabled = false;

                    labelProgress.Text = string.Format("{0}%", progressBar.Value);
                }
                else
                {
                    progressBar.Value = (int)progress;

                    labelProgress.Text = string.Format("{0}%", progressBar.Value);
                }
            }
        }

        private void ShowProgress()
        {
            double progress = 0;
            while (progress >= 0 && progress < 100)
            {
                int insertRows = 0;

                try
                {
                    progress = _TableSync.GetProgress(out insertRows);

                    ShowOptimizeProgress(progress, insertRows);

                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    ShowException(e);

                    ShowOptimizeProgress(100, insertRows);
                    
                    return;
                }
            }

        }

        private bool Start()
        {
            try
            {
                _StartTime = DateTime.Now;

                TableSynchronization.OptimizeOption option = TableSynchronization.OptimizeOption.Minimum;

                if (comboBoxOptimizeOption.Text.Equals("Middle"))
                {
                    option = TableSynchronization.OptimizeOption.Middle;
                }
                else if (comboBoxOptimizeOption.Text.Equals("None"))
                {
                    option = TableSynchronization.OptimizeOption.None;
                }

                int step = (int)numericUpDownStep.Value;

                string sql = GetSql(step, option);

                GlobalSetting.DataAccess.Excute(sql);

                _TableSync = new TableSynchronization(DataAccess.Conn, TableName, step, option, 
                    false, null);

                _Thread = new System.Threading.Thread(ShowProgress);
                _Thread.IsBackground = true;
                _Thread.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool Stop()
        {
            try
            {
                _TableSync.Stop();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "Start")
            {
                if (Start())
                {
                    buttonStart.Text = "Stop";
                }
            }
            else
            {
                if (Stop())
                {
                    buttonStart.Enabled = false;
                }
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string GetSql(int step, TableSynchronization.OptimizeOption option)
        {
            return string.Format("exec SP_Rebuild '{0}', {1}, {2}",
                _TableName.Replace("'", "''"), step, (int)option);
        }

        private void buttonViewScript_Click(object sender, EventArgs e)
        {
            TableSynchronization.OptimizeOption option = TableSynchronization.OptimizeOption.Minimum;

            if (comboBoxOptimizeOption.Text.Equals("Middle"))
            {
                option = TableSynchronization.OptimizeOption.Middle;
            }
            else if (comboBoxOptimizeOption.Text.Equals("None"))
            {
                option = TableSynchronization.OptimizeOption.None;
            }

            int step = (int)numericUpDownStep.Value;

            textBoxScript.Text = GetSql(step, option);

            this.Height = textBoxScript.Top + textBoxScript.Height + 40;
        }
    }
}
