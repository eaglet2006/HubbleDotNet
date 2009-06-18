using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hubble.Core.Data;

namespace WinformDemo
{
    public partial class FormDelete : Form
    {
        long _From;
        long _To;

        public FormDelete()
        {
            InitializeComponent();
        }

        delegate void ShowProgressDelegate(int newPos);

        private void ShowProgress(int newPos)
        {
            if (!progressBar1.InvokeRequired)
            {
                progressBar1.Value = newPos;
            }
            else
            {
                ShowProgressDelegate showProgress = new ShowProgressDelegate(ShowProgress);
                this.BeginInvoke(showProgress, new object[] { newPos });
            }
        }

        private void RunAppend(Object stateInfo)
        {
            try
            {
                DBAccess dbAccess = new DBAccess();

                long steps = (_To - _From) / 10 + 1;

                for (int i = 0; i < steps; i++)
                {
                    List<long> docs = new List<long>();

                    for (int j = i * 10; j < (i + 1) * 10 && j < _To; j++)
                    {
                        docs.Add(j);
                    }

                    dbAccess.Delete("News", docs);

                    ShowProgress((int)(i * 100 / steps));

                    System.Threading.Thread.Sleep(100);
                }

                MessageBox.Show("Delete successfull!");
            }
            catch
            {
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            _From = (long)numericUpDownFrom.Value;
            _To = (long)numericUpDownTo.Value;

            if (_To < _From)
            {
                MessageBox.Show("To can't less then from!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(RunAppend));

        }
    }
}
