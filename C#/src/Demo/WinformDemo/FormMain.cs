using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinformDemo
{
    public partial class FormMain : Form
    {
        Hubble.Core.Data.DBProvider _CurDBProvider;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            _CurDBProvider = Hubble.Core.Data.DBProvider.GetDBProvider("News");
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Hubble.Core.Data.DBProvider.Init();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (_CurDBProvider != null)
            {
                QueryPerfCounter qp = new QueryPerfCounter();
                qp.Start();

                _CurDBProvider.Select(textBoxSearch.Text, 0, 100);

                qp.Stop();
                double ns = qp.Duration(1);

                labelDuration.Text = (ns / (1000 * 1000)).ToString() + " ms"; 
            }
        }
    }
}
