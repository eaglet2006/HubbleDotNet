namespace WinformDemo
{
    partial class FormCreate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDirectory = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxNewsXml = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.openFileDialogNewsXml = new System.Windows.Forms.OpenFileDialog();
            this.buttonBrowserNewsXml = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownCount = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxComplexIndex = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Table Name";
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Location = new System.Drawing.Point(110, 13);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(342, 20);
            this.textBoxTableName.TabIndex = 1;
            this.textBoxTableName.Text = "News";
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Location = new System.Drawing.Point(110, 39);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(342, 20);
            this.textBoxConnectionString.TabIndex = 3;
            this.textBoxConnectionString.Text = "Data Source=(local);Initial Catalog=Test;Integrated Security=True";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "ConnectionString";
            // 
            // textBoxDirectory
            // 
            this.textBoxDirectory.Location = new System.Drawing.Point(110, 65);
            this.textBoxDirectory.Name = "textBoxDirectory";
            this.textBoxDirectory.Size = new System.Drawing.Size(342, 20);
            this.textBoxDirectory.TabIndex = 5;
            this.textBoxDirectory.Text = "D:\\Test\\News";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Table Directory";
            // 
            // textBoxNewsXml
            // 
            this.textBoxNewsXml.Location = new System.Drawing.Point(110, 91);
            this.textBoxNewsXml.Name = "textBoxNewsXml";
            this.textBoxNewsXml.Size = new System.Drawing.Size(342, 20);
            this.textBoxNewsXml.TabIndex = 7;
            this.textBoxNewsXml.Text = "News.xml";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "News.xml";
            // 
            // buttonCreate
            // 
            this.buttonCreate.Enabled = false;
            this.buttonCreate.Location = new System.Drawing.Point(110, 235);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(75, 23);
            this.buttonCreate.TabIndex = 8;
            this.buttonCreate.Text = "Create";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 184);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(437, 23);
            this.progressBar1.TabIndex = 9;
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(15, 234);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonLoad.TabIndex = 10;
            this.buttonLoad.Text = "Load News";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // openFileDialogNewsXml
            // 
            this.openFileDialogNewsXml.FileName = "News.xml";
            this.openFileDialogNewsXml.Filter = "xml|*.xml";
            // 
            // buttonBrowserNewsXml
            // 
            this.buttonBrowserNewsXml.Location = new System.Drawing.Point(458, 91);
            this.buttonBrowserNewsXml.Name = "buttonBrowserNewsXml";
            this.buttonBrowserNewsXml.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowserNewsXml.TabIndex = 11;
            this.buttonBrowserNewsXml.Text = "...";
            this.buttonBrowserNewsXml.UseVisualStyleBackColor = true;
            this.buttonBrowserNewsXml.Click += new System.EventHandler(this.buttonBrowserNewsXml_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Insert";
            // 
            // numericUpDownCount
            // 
            this.numericUpDownCount.Location = new System.Drawing.Point(110, 150);
            this.numericUpDownCount.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDownCount.Name = "numericUpDownCount";
            this.numericUpDownCount.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownCount.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(248, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "rows";
            // 
            // checkBoxComplexIndex
            // 
            this.checkBoxComplexIndex.AutoSize = true;
            this.checkBoxComplexIndex.Location = new System.Drawing.Point(15, 122);
            this.checkBoxComplexIndex.Name = "checkBoxComplexIndex";
            this.checkBoxComplexIndex.Size = new System.Drawing.Size(66, 17);
            this.checkBoxComplexIndex.TabIndex = 15;
            this.checkBoxComplexIndex.Text = "Complex";
            this.checkBoxComplexIndex.UseVisualStyleBackColor = true;
            // 
            // FormCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 314);
            this.Controls.Add(this.checkBoxComplexIndex);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericUpDownCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.buttonBrowserNewsXml);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.textBoxNewsXml);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxDirectory);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxConnectionString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxTableName);
            this.Controls.Add(this.label1);
            this.Name = "FormCreate";
            this.Text = "Create Table";
            this.Load += new System.EventHandler(this.FormCreate_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDirectory;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxNewsXml;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.OpenFileDialog openFileDialogNewsXml;
        private System.Windows.Forms.Button buttonBrowserNewsXml;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownCount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxComplexIndex;
    }
}