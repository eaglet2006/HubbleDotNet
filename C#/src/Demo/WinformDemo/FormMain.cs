using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hubble.Core.SFQL.Parse;

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
            try
            {
                _CurDBProvider = Hubble.Core.Data.DBProvider.GetDBProvider("News");
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message + "\r\n" + e1.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                
                int count;

                _CurDBProvider.Select(textBoxSearch.Text, 0, 100, out count);

                qp.Stop();
                double ns = qp.Duration(1);

                labelDuration.Text = (ns / (1000 * 1000)).ToString() + " ms";
                labelCount.Text = count.ToString();
            }
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            FormCreate frmCreate = new FormCreate();
            frmCreate.ShowDialog();
            GC.Collect();
        }

        private List<byte[]> GetArray()
        {
            List<byte[]> Test = new List<byte[]>();

            for (int i = 0; i < 300000; i++)
            {
                Test.Add(new byte[10]);
            }

            return Test;
        }

        private void buttonDrop_Click(object sender, EventArgs e)
        {
            Hubble.Core.Data.DBProvider.Drop("News");
            GC.Collect();
            _CurDBProvider = null;
        }

        private void buttonAppend_Click(object sender, EventArgs e)
        {
            FormAppend frmAppend = new FormAppend();
            frmAppend.Show();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            FormDelete frmDelete = new FormDelete();
            frmDelete.Show();
        }

        private void buttonOptimize_Click(object sender, EventArgs e)
        {
            if (_CurDBProvider != null)
            {
                _CurDBProvider.Optimize();
            }
        }

        private void buttonOptimizeMin_Click(object sender, EventArgs e)
        {
            if (_CurDBProvider != null)
            {
                _CurDBProvider.Optimize(Hubble.Core.Data.OptimizationOption.Minimum);
            }

        }

        private void ShowErrorMessage(string err)
        {
            tabControl1.SelectedTab = tabPageMessages;
            textBoxMessages.ForeColor = Color.Red;
            textBoxMessages.Text = err;
        }

        private void ShowMessages(List<string> messages)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in messages)
            {
                sb.AppendLine(str);
            }

            tabControl1.SelectedTab = tabPageMessages;
            textBoxMessages.ForeColor = Color.Black;
            textBoxMessages.Text = sb.ToString();

        }

        private void buttonExcute_Click(object sender, EventArgs e)
        {
            try
            {
                tabControl1.SelectedTab = tabPageResults;
                textBoxMessages.Text = "";

                QueryPerfCounter qp = new QueryPerfCounter();
                qp.Start();

                int count = 0;

                SFQLParse sfqlParse = new SFQLParse();
                QueryResult queryResult = sfqlParse.Query(textBoxSql.Text);

                System.Data.DataTable table = null;

                if (queryResult.DataSet.Tables.Count > 0)
                {
                    table = queryResult.DataSet.Tables[0];
                    count = table.MinimumCapacity;
                }

                qp.Stop();
                double ns = qp.Duration(1);

                labelDuration.Text = (ns / (1000 * 1000)).ToString("0.00") + " ms";
                labelCount.Text = count.ToString();

                if (queryResult.PrintMessages != null)
                {
                    if (queryResult.PrintMessages.Count > 0)
                    {
                        ShowMessages(queryResult.PrintMessages);
                    }
                }

                if (queryResult.DataSet.Tables.Count > 0)
                {
                    dataGridViewResult.DataSource = table;
                }
                else
                {
                    dataGridViewResult.DataSource = null;
                }

                
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
        }


    }
}
