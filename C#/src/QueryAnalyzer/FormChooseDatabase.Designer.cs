namespace QueryAnalyzer
{
    partial class FormChooseDatabase
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
            this.comboBoxDatabaseName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonChoose = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxDatabaseName
            // 
            this.comboBoxDatabaseName.FormattingEnabled = true;
            this.comboBoxDatabaseName.Location = new System.Drawing.Point(108, 21);
            this.comboBoxDatabaseName.Name = "comboBoxDatabaseName";
            this.comboBoxDatabaseName.Size = new System.Drawing.Size(215, 21);
            this.comboBoxDatabaseName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Database";
            // 
            // buttonChoose
            // 
            this.buttonChoose.Location = new System.Drawing.Point(39, 75);
            this.buttonChoose.Name = "buttonChoose";
            this.buttonChoose.Size = new System.Drawing.Size(75, 23);
            this.buttonChoose.TabIndex = 2;
            this.buttonChoose.Text = "Choose";
            this.buttonChoose.UseVisualStyleBackColor = true;
            this.buttonChoose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(135, 74);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormChooseDatabase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 123);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonChoose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxDatabaseName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormChooseDatabase";
            this.Text = "Choose Database";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDatabaseName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonChoose;
        private System.Windows.Forms.Button buttonCancel;
    }
}