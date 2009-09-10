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
    public partial class FormCreate : Form
    {
        XmlNodeList _DocumentList;


        public FormCreate()
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

                    numericUpDownCount.Value = nodes.Count;
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FormCreate_Load(object sender, EventArgs e)
        {
            buttonCreate.Enabled = false;
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

                    buttonCreate.Enabled = true;
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

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            try
            {
                Hubble.Core.Data.DBProvider.Drop("News");

                string analyseName = "PanGuSegment";

                DBAccess dbAccess = new DBAccess();
                Table table = new Table();
                table.ConnectionString = textBoxConnectionString.Text;
                table.DBAdapterTypeName = "SQLSERVER2005";
                table.DBTableName = textBoxTableName.Text;
                table.Fields.Add(new Field("title", DataType.String, 256, true, Field.Index.Tokenized, analyseName));

                if (checkBoxComplexIndex.Checked)
                {
                    table.Fields.Add(new Field("content", DataType.String, true, Field.Index.Tokenized, Field.IndexMode.Complex, analyseName));
                }
                else
                {
                    table.Fields.Add(new Field("content", DataType.String, true, Field.Index.Tokenized, Field.IndexMode.Simple, analyseName));
                }

                table.Fields.Add(new Field("Url", DataType.String, true, Field.Index.None));
                table.Fields.Add(new Field("Time", DataType.DateTime, true, Field.Index.Untokenized));
                table.Name = textBoxTableName.Text;
                //table.SQLForCreate = "create index I_News_Title on News(title);";

                dbAccess.CreateTable(table, @"D:\Test\News");

                Stopwatch watch = new Stopwatch();
                long docId = 0;
                int totalChars = 0;
                int contentCount = 0;

                List<Document> docs = new List<Document>();

                foreach (XmlNode node in _DocumentList)
                {
                    String title = node.Attributes["Title"].Value;
                    DateTime time = DateTime.Parse(node.Attributes["Time"].Value);
                    String Url = node.Attributes["Url"].Value;
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;


                    Document doc = new Document();
                    doc.FieldValues.Add(new FieldValue("title", title));
                    doc.FieldValues.Add(new FieldValue("content", content));
                    doc.FieldValues.Add(new FieldValue("Time", time.ToString("yyyy-MM-dd HH:mm:ss")));
                    doc.FieldValues.Add(new FieldValue("Url", Url));
                    docs.Add(doc);

                    contentCount++;

                    if (contentCount % 100 == 0)
                    {
                        watch.Start();
                        dbAccess.Insert(table.Name, docs);
                        watch.Stop();

                        docs.Clear();
                        progressBar1.Value =(int)(contentCount * 100 / numericUpDownCount.Value);
                        Application.DoEvents();
                    }

                    if (contentCount == (int)numericUpDownCount.Value)
                    {
                        break;
                    }
                }

                if (docs.Count > 0)
                {
                    watch.Start();
                    dbAccess.Insert(table.Name, docs);
                    watch.Stop();

                    docs.Clear();
                    progressBar1.Value = (int)(contentCount * 100 / numericUpDownCount.Value);
                    Application.DoEvents();
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
    }
}
