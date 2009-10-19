using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormWaittingGetTotalRecords : Form
    {
        public void GetTotalRecordsDelegate(int current)
        {
            labelCurrent.Text = current.ToString();
            Application.DoEvents();
        }

        public FormWaittingGetTotalRecords()
        {
            InitializeComponent();
        }

        private void FormWaittingGetTotalRecords_Load(object sender, EventArgs e)
        {

        }
    }
}
