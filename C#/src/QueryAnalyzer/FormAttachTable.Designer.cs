namespace QueryAnalyzer
{
    partial class FormAttachTable
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
            this.textBoxDirectory = new System.Windows.Forms.TextBox();
            this.checkBoxTableName = new System.Windows.Forms.CheckBox();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.textBoxConnectString = new System.Windows.Forms.TextBox();
            this.checkBoxConnectString = new System.Windows.Forms.CheckBox();
            this.textBoxDBTableName = new System.Windows.Forms.TextBox();
            this.checkBoxDBTableName = new System.Windows.Forms.CheckBox();
            this.buttonAttach = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Index folder in server";
            // 
            // textBoxDirectory
            // 
            this.textBoxDirectory.Location = new System.Drawing.Point(147, 27);
            this.textBoxDirectory.Name = "textBoxDirectory";
            this.textBoxDirectory.Size = new System.Drawing.Size(402, 20);
            this.textBoxDirectory.TabIndex = 1;
            // 
            // checkBoxTableName
            // 
            this.checkBoxTableName.AutoSize = true;
            this.checkBoxTableName.Location = new System.Drawing.Point(15, 59);
            this.checkBoxTableName.Name = "checkBoxTableName";
            this.checkBoxTableName.Size = new System.Drawing.Size(84, 17);
            this.checkBoxTableName.TabIndex = 4;
            this.checkBoxTableName.Text = "Table Name";
            this.checkBoxTableName.UseVisualStyleBackColor = true;
            this.checkBoxTableName.CheckedChanged += new System.EventHandler(this.checkBoxTableName_CheckedChanged);
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Enabled = false;
            this.textBoxTableName.Location = new System.Drawing.Point(147, 57);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(402, 20);
            this.textBoxTableName.TabIndex = 5;
            // 
            // textBoxConnectString
            // 
            this.textBoxConnectString.Enabled = false;
            this.textBoxConnectString.Location = new System.Drawing.Point(147, 83);
            this.textBoxConnectString.Name = "textBoxConnectString";
            this.textBoxConnectString.Size = new System.Drawing.Size(402, 20);
            this.textBoxConnectString.TabIndex = 7;
            // 
            // checkBoxConnectString
            // 
            this.checkBoxConnectString.AutoSize = true;
            this.checkBoxConnectString.Location = new System.Drawing.Point(15, 85);
            this.checkBoxConnectString.Name = "checkBoxConnectString";
            this.checkBoxConnectString.Size = new System.Drawing.Size(128, 17);
            this.checkBoxConnectString.TabIndex = 6;
            this.checkBoxConnectString.Text = "DB Connection String";
            this.checkBoxConnectString.UseVisualStyleBackColor = true;
            this.checkBoxConnectString.CheckedChanged += new System.EventHandler(this.checkBoxConnectString_CheckedChanged);
            // 
            // textBoxDBTableName
            // 
            this.textBoxDBTableName.Enabled = false;
            this.textBoxDBTableName.Location = new System.Drawing.Point(147, 109);
            this.textBoxDBTableName.Name = "textBoxDBTableName";
            this.textBoxDBTableName.Size = new System.Drawing.Size(402, 20);
            this.textBoxDBTableName.TabIndex = 9;
            // 
            // checkBoxDBTableName
            // 
            this.checkBoxDBTableName.AutoSize = true;
            this.checkBoxDBTableName.Location = new System.Drawing.Point(15, 111);
            this.checkBoxDBTableName.Name = "checkBoxDBTableName";
            this.checkBoxDBTableName.Size = new System.Drawing.Size(102, 17);
            this.checkBoxDBTableName.TabIndex = 8;
            this.checkBoxDBTableName.Text = "DB Table Name";
            this.checkBoxDBTableName.UseVisualStyleBackColor = true;
            this.checkBoxDBTableName.CheckedChanged += new System.EventHandler(this.checkBoxDBTableName_CheckedChanged);
            // 
            // buttonAttach
            // 
            this.buttonAttach.Location = new System.Drawing.Point(12, 151);
            this.buttonAttach.Name = "buttonAttach";
            this.buttonAttach.Size = new System.Drawing.Size(75, 23);
            this.buttonAttach.TabIndex = 10;
            this.buttonAttach.Text = "Attach";
            this.buttonAttach.UseVisualStyleBackColor = true;
            this.buttonAttach.Click += new System.EventHandler(this.buttonAttach_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(111, 151);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormAttachTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 214);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAttach);
            this.Controls.Add(this.textBoxDBTableName);
            this.Controls.Add(this.checkBoxDBTableName);
            this.Controls.Add(this.textBoxConnectString);
            this.Controls.Add(this.checkBoxConnectString);
            this.Controls.Add(this.textBoxTableName);
            this.Controls.Add(this.checkBoxTableName);
            this.Controls.Add(this.textBoxDirectory);
            this.Controls.Add(this.label1);
            this.Name = "FormAttachTable";
            this.Text = "Attach Table";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDirectory;
        private System.Windows.Forms.CheckBox checkBoxTableName;
        private System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.TextBox textBoxConnectString;
        private System.Windows.Forms.CheckBox checkBoxConnectString;
        private System.Windows.Forms.TextBox textBoxDBTableName;
        private System.Windows.Forms.CheckBox checkBoxDBTableName;
        private System.Windows.Forms.Button buttonAttach;
        private System.Windows.Forms.Button buttonCancel;
    }
}