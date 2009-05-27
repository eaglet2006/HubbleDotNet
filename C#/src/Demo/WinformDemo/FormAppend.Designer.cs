namespace WinformDemo
{
    partial class FormAppend
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
            this.buttonBrowserNewsXml = new System.Windows.Forms.Button();
            this.textBoxNewsXml = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownFrom = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.numericUpDownTo = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.openFileDialogNewsXml = new System.Windows.Forms.OpenFileDialog();
            this.buttonAppend = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTo)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonBrowserNewsXml
            // 
            this.buttonBrowserNewsXml.Location = new System.Drawing.Point(479, 12);
            this.buttonBrowserNewsXml.Name = "buttonBrowserNewsXml";
            this.buttonBrowserNewsXml.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowserNewsXml.TabIndex = 14;
            this.buttonBrowserNewsXml.Text = "...";
            this.buttonBrowserNewsXml.UseVisualStyleBackColor = true;
            this.buttonBrowserNewsXml.Click += new System.EventHandler(this.buttonBrowserNewsXml_Click);
            // 
            // textBoxNewsXml
            // 
            this.textBoxNewsXml.Location = new System.Drawing.Point(131, 12);
            this.textBoxNewsXml.Name = "textBoxNewsXml";
            this.textBoxNewsXml.Size = new System.Drawing.Size(342, 20);
            this.textBoxNewsXml.TabIndex = 13;
            this.textBoxNewsXml.Text = "News.xml";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "News.xml";
            // 
            // numericUpDownFrom
            // 
            this.numericUpDownFrom.Location = new System.Drawing.Point(131, 48);
            this.numericUpDownFrom.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDownFrom.Name = "numericUpDownFrom";
            this.numericUpDownFrom.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownFrom.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "From";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(36, 148);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(437, 23);
            this.progressBar1.TabIndex = 15;
            // 
            // numericUpDownTo
            // 
            this.numericUpDownTo.Location = new System.Drawing.Point(131, 85);
            this.numericUpDownTo.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDownTo.Name = "numericUpDownTo";
            this.numericUpDownTo.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownTo.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "To";
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(36, 205);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonLoad.TabIndex = 21;
            this.buttonLoad.Text = "Load News";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // openFileDialogNewsXml
            // 
            this.openFileDialogNewsXml.FileName = "News.xml";
            this.openFileDialogNewsXml.Filter = "xml|*.xml";
            // 
            // buttonAppend
            // 
            this.buttonAppend.Enabled = false;
            this.buttonAppend.Location = new System.Drawing.Point(131, 205);
            this.buttonAppend.Name = "buttonAppend";
            this.buttonAppend.Size = new System.Drawing.Size(75, 23);
            this.buttonAppend.TabIndex = 22;
            this.buttonAppend.Text = "Append";
            this.buttonAppend.UseVisualStyleBackColor = true;
            this.buttonAppend.Click += new System.EventHandler(this.buttonAppend_Click);
            // 
            // FormAppend
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 368);
            this.Controls.Add(this.buttonAppend);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.numericUpDownTo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownFrom);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonBrowserNewsXml);
            this.Controls.Add(this.textBoxNewsXml);
            this.Controls.Add(this.label4);
            this.Name = "FormAppend";
            this.Text = "Append";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowserNewsXml;
        private System.Windows.Forms.TextBox textBoxNewsXml;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownFrom;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.NumericUpDown numericUpDownTo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.OpenFileDialog openFileDialogNewsXml;
        private System.Windows.Forms.Button buttonAppend;
    }
}