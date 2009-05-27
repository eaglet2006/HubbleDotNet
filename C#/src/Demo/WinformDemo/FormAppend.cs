using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using Hubble.Core.Data;

namespace WinformDemo
{
    public partial class FormAppend : Form
    {
        XmlNodeList _DocumentList;

        decimal _From;
        decimal _To;

        public FormAppend()
        {
            InitializeComponent();
        }

        private void buttonBrowserNewsXml_Click(object sender, EventArgs e)
        {
            if (openFileDialogNewsXml.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    textBoxNewsXml.Text = openFileDialogNewsXml.FileName;

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(textBoxNewsXml.Text);
                    XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");

                    numericUpDownTo.Value = nodes.Count;
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(textBoxNewsXml.Text))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(textBoxNewsXml.Text);
                    _DocumentList = xmlDoc.SelectNodes(@"News/Item");

                    buttonAppend.Enabled = true;
                }
                else
                {
                    MessageBox.Show(string.Format("File {0} does not exits",
                        textBoxNewsXml.Text), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                Stopwatch watch = new Stopwatch();
                long docId = 0;
                int totalChars = 0;
                int contentCount = 0;

                List<Document> docs = new List<Document>();
                int i = 0;

                foreach (XmlNode node in _DocumentList)
                {
                    if (i++ < numericUpDownFrom.Value)
                    {
                        continue;
                    }

                    String title = node.Attributes["Title"].Value;
                    DateTime time = DateTime.Parse(node.Attributes["Time"].Value);
                    String Url = node.Attributes["Url"].Value;
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;


                    Document doc = new Document();
                    doc.FieldValues.Add(new FieldValue("title", title, DataType.String));
                    doc.FieldValues.Add(new FieldValue("content", content, DataType.String));
                    doc.FieldValues.Add(new FieldValue("Time", time.ToString("yyyy-MM-dd HH:mm:ss"), DataType.String));
                    doc.FieldValues.Add(new FieldValue("Url", Url, DataType.String));
                    docs.Add(doc);

                    contentCount++;

                    if (contentCount % 100 == 0)
                    {
                        watch.Start();
                        dbAccess.Insert("News", docs);
                        watch.Stop();

                        docs.Clear();
                        ShowProgress((int)(contentCount * 100 / (_To - _From)));
                    }

                    if (contentCount == (int)(_To - _From))
                    {
                        break;
                    }
                }

                watch.Start();
                dbAccess.Close();
                watch.Stop();

                MessageBox.Show(String.Format("Insert {0} rows , total chars: {1} , duration: {2} seconds",
                    docId, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000
                    ));
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message + "\r\n" + e1.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void buttonAppend_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            _From = numericUpDownFrom.Value;
            _To = numericUpDownTo.Value;

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(RunAppend));

            buttonAppend.Enabled = false;
        }
    }
}
