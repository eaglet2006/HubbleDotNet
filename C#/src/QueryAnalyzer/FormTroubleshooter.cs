using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormTroubleshooter : Form
    {
        public FormTroubleshooter()
        {
            InitializeComponent();
        }

        internal void ShowDialog(string text)
        {
            textBoxError.Text = text;
            ShowDialog();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
