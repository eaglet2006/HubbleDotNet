using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinformDemo
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            //Hubble.Analyzer.PanGuAnalyzer panguAnalyzer = new Hubble.Analyzer.PanGuAnalyzer();
            //panguAnalyzer.Init();

            FormMain frmMain = new FormMain();

            frmMain.Show();

            this.Hide();
        }
    }
}
