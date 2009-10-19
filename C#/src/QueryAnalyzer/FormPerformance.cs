using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Hubble.Core.SFQL.Parse;

namespace QueryAnalyzer
{
    public partial class FormPerformance : Form
    {
        internal DbAccess DataAccess { get; set; }

        public FormPerformance()
        {
            InitializeComponent();
        }


        private void ShowErrorMessage(string err)
        {
            MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void buttonTest_Click(object sender, EventArgs e)
        {
            try
            {
                QueryPerfCounter qp = new QueryPerfCounter();
                qp.Start();

                SFQLParse sfqlParse = new SFQLParse();
                string sql = textBoxSql.Text;

                if (!string.IsNullOrEmpty(textBoxSql.SelectedText))
                {
                    sql = textBoxSql.SelectedText;
                }

                for (int i = 0; i < numericUpDownIteration.Value; i++)
                {
                    DataAccess.Excute(sql);
                }

                qp.Stop();
                double ns = qp.Duration(1);

                StringBuilder report = new StringBuilder();

                report.AppendFormat("{0} ", (ns / (1000 * 1000 * (int)numericUpDownIteration.Value)).ToString("0.00") + " ms");

                labelDuration.Text = report.ToString();

            }
            catch (Hubble.Core.SFQL.LexicalAnalysis.LexicalException lexicalEx)
            {
                ShowErrorMessage(lexicalEx.ToString());
            }
            catch (Hubble.Core.SFQL.SyntaxAnalysis.SyntaxException syntaxEx)
            {
                ShowErrorMessage(syntaxEx.ToString());
            }
            catch (Exception e1)
            {
                ShowErrorMessage(e1.Message + "\r\n" + e1.StackTrace);
            }
            finally
            {
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
