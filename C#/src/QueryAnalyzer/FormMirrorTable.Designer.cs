namespace QueryAnalyzer
{
    partial class FormMirrorTable
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
            this.buttonTestConnectionString = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDBAdapter = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxSQLForCreate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonTestConnectionString
            // 
            this.buttonTestConnectionString.Location = new System.Drawing.Point(252, 338);
            this.buttonTestConnectionString.Name = "buttonTestConnectionString";
            this.buttonTestConnectionString.Size = new System.Drawing.Size(169, 23);
            this.buttonTestConnectionString.TabIndex = 42;
            this.buttonTestConnectionString.Text = "Test DB Connection String";
            this.buttonTestConnectionString.UseVisualStyleBackColor = true;
            this.buttonTestConnectionString.Click += new System.EventHandler(this.buttonTestConnectionString_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 41;
            this.label1.Text = "Table Name";
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Location = new System.Drawing.Point(189, 21);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(241, 20);
            this.textBoxTableName.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(189, 152);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(346, 13);
            this.textBox2.TabIndex = 38;
            this.textBox2.Text = "E.g. Data Source=(local);Initial Catalog=xxx;Integrated Security=True";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(186, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(244, 23);
            this.label5.TabIndex = 37;
            this.label5.Text = "Connection string to Database.";
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Location = new System.Drawing.Point(189, 110);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(383, 20);
            this.textBoxConnectionString.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 13);
            this.label6.TabIndex = 35;
            this.label6.Text = "DB Connection String";
            // 
            // comboBoxDBAdapter
            // 
            this.comboBoxDBAdapter.FormattingEnabled = true;
            this.comboBoxDBAdapter.Location = new System.Drawing.Point(189, 58);
            this.comboBoxDBAdapter.Name = "comboBoxDBAdapter";
            this.comboBoxDBAdapter.Size = new System.Drawing.Size(241, 21);
            this.comboBoxDBAdapter.TabIndex = 1;
            this.comboBoxDBAdapter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxDBAdapter_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(186, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Choose a DB Adapter";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 32;
            this.label4.Text = "DB Adapter";
            // 
            // textBoxSQLForCreate
            // 
            this.textBoxSQLForCreate.Location = new System.Drawing.Point(189, 185);
            this.textBoxSQLForCreate.Multiline = true;
            this.textBoxSQLForCreate.Name = "textBoxSQLForCreate";
            this.textBoxSQLForCreate.Size = new System.Drawing.Size(383, 131);
            this.textBoxSQLForCreate.TabIndex = 43;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 189);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 44;
            this.label2.Text = "SQL For create";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(39, 338);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 45;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(140, 338);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 46;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormMirrorTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 373);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxSQLForCreate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonTestConnectionString);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxTableName);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxConnectionString);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxDBAdapter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Name = "FormMirrorTable";
            this.Text = "MirrorTable";
            this.Load += new System.EventHandler(this.FormMirrorTable_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTestConnectionString;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.ComboBox comboBoxDBAdapter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.TextBox textBoxSQLForCreate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}