namespace QueryAnalyzer
{
    partial class FormDatabaseInfo
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
            this.textBoxDefIndexFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxDBAdapter = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxDefConnectionString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.buttonChange = new System.Windows.Forms.Button();
            this.textBoxDatabaseName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonTestConnectionString = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 15;
            // 
            // textBoxDefIndexFolder
            // 
            this.textBoxDefIndexFolder.Location = new System.Drawing.Point(187, 45);
            this.textBoxDefIndexFolder.Name = "textBoxDefIndexFolder";
            this.textBoxDefIndexFolder.Size = new System.Drawing.Size(241, 20);
            this.textBoxDefIndexFolder.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Folder path in server. E.g. d:\\Test";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(184, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Choose a DB Adapter";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Default DB Adapter";
            // 
            // comboBoxDBAdapter
            // 
            this.comboBoxDBAdapter.FormattingEnabled = true;
            this.comboBoxDBAdapter.Location = new System.Drawing.Point(187, 98);
            this.comboBoxDBAdapter.Name = "comboBoxDBAdapter";
            this.comboBoxDBAdapter.Size = new System.Drawing.Size(241, 21);
            this.comboBoxDBAdapter.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(184, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(244, 23);
            this.label5.TabIndex = 9;
            this.label5.Text = "Connection string to Database.";
            // 
            // textBoxDefConnectionString
            // 
            this.textBoxDefConnectionString.Location = new System.Drawing.Point(187, 150);
            this.textBoxDefConnectionString.Name = "textBoxDefConnectionString";
            this.textBoxDefConnectionString.Size = new System.Drawing.Size(329, 20);
            this.textBoxDefConnectionString.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(34, 154);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Default DB Connection String";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(187, 192);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(346, 13);
            this.textBox2.TabIndex = 11;
            this.textBox2.Text = "E.g. Data Source=(local);Initial Catalog=xxx;Integrated Security=True";
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(37, 232);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(75, 23);
            this.buttonChange.TabIndex = 12;
            this.buttonChange.Text = "Change";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // textBoxDatabaseName
            // 
            this.textBoxDatabaseName.Enabled = false;
            this.textBoxDatabaseName.Location = new System.Drawing.Point(187, 11);
            this.textBoxDatabaseName.Name = "textBoxDatabaseName";
            this.textBoxDatabaseName.Size = new System.Drawing.Size(241, 20);
            this.textBoxDatabaseName.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(34, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Database Name";
            // 
            // buttonTestConnectionString
            // 
            this.buttonTestConnectionString.Location = new System.Drawing.Point(133, 231);
            this.buttonTestConnectionString.Name = "buttonTestConnectionString";
            this.buttonTestConnectionString.Size = new System.Drawing.Size(169, 23);
            this.buttonTestConnectionString.TabIndex = 14;
            this.buttonTestConnectionString.Text = "Test DB Connection String";
            this.buttonTestConnectionString.UseVisualStyleBackColor = true;
            this.buttonTestConnectionString.Click += new System.EventHandler(this.buttonTestConnectionString_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(34, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(102, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Default Index Folder";
            // 
            // FormDatabaseInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 282);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.buttonTestConnectionString);
            this.Controls.Add(this.textBoxDatabaseName);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDefConnectionString);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxDBAdapter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDefIndexFolder);
            this.Controls.Add(this.label1);
            this.Name = "FormDatabaseInfo";
            this.Text = "Database Information";
            this.Load += new System.EventHandler(this.FormCreateDatabase_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDefIndexFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxDBAdapter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxDefConnectionString;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.TextBox textBoxDatabaseName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonTestConnectionString;
        private System.Windows.Forms.Label label8;
    }
}