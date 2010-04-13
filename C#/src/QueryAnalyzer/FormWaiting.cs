using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class FormWaiting : Form
    {
        static private bool _Showing;

        public static bool Showing
        {
            get
            {
                return _Showing;
            }
        }

        public FormWaiting()
        {
            InitializeComponent();
        }

        new public void Show()
        {
            if (Showing)
            {
                return;
            }

            base.Show();

            Application.DoEvents();
        }

        private void FormWatting_Load(object sender, EventArgs e)
        {
            _Showing = true;
        }

        private void FormWaiting_FormClosed(object sender, FormClosedEventArgs e)
        {
            _Showing = false;
        }

    }
}