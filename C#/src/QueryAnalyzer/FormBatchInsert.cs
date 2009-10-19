using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace QueryAnalyzer
{
    public partial class FormBatchInsert : Form
    {
        public int TotalRecords { get; set; }

        internal string FileName { get; set; }

        internal DbAccess DataAccess { get; set; }

        internal BatchInsert BatchInsert { get; set; }

        public FormBatchInsert()
        {
            InitializeComponent();
        }

        public void GetTotalRecordsDelegate(int current)
        {
            progressBar1.Value = (int)((decimal)(current * 100) / numericUpDownRecords.Value);
            Application.DoEvents();
        }


        private void FormBatchInsert_Load(object sender, EventArgs e)
        {
            numericUpDownRecords.Maximum = TotalRecords;
            numericUpDownRecords.Value = TotalRecords;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar1.Value = 0;

                Stopwatch sw = new Stopwatch();

                sw.Start();

                BatchInsert.BatchImport(FileName, DataAccess, (int)numericUpDownRecords.Value,
                    GetTotalRecordsDelegate);

                sw.Stop();

                MessageBox.Show(string.Format("Batch insert {0} records successful! Duration: {1} ms",
                    numericUpDownRecords.Value, sw.ElapsedMilliseconds));
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
