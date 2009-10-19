using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    public partial class TableField : UserControl
    {
        public TableField()
        {
            InitializeComponent();
        }


        private void comboBoxDataType_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = false;
        }


    }
}
