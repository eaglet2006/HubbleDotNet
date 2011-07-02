namespace QueryAnalyzer.BigTable
{
    partial class FormBigTableBatchInsert
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
            this.comboBoxBalanceServers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonBalance = new System.Windows.Forms.RadioButton();
            this.radioButtonFailoverServers = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxTable = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxBalanceServers
            // 
            this.comboBoxBalanceServers.FormattingEnabled = true;
            this.comboBoxBalanceServers.Location = new System.Drawing.Point(145, 14);
            this.comboBoxBalanceServers.Name = "comboBoxBalanceServers";
            this.comboBoxBalanceServers.Size = new System.Drawing.Size(272, 21);
            this.comboBoxBalanceServers.TabIndex = 9;
            this.comboBoxBalanceServers.SelectedIndexChanged += new System.EventHandler(this.comboBoxBalanceServers_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Choose Server:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonFailoverServers);
            this.groupBox1.Controls.Add(this.radioButtonBalance);
            this.groupBox1.Location = new System.Drawing.Point(145, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 86);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Type";
            // 
            // radioButtonBalance
            // 
            this.radioButtonBalance.AutoSize = true;
            this.radioButtonBalance.Checked = true;
            this.radioButtonBalance.Location = new System.Drawing.Point(24, 20);
            this.radioButtonBalance.Name = "radioButtonBalance";
            this.radioButtonBalance.Size = new System.Drawing.Size(98, 17);
            this.radioButtonBalance.TabIndex = 0;
            this.radioButtonBalance.TabStop = true;
            this.radioButtonBalance.Text = "Balance Server";
            this.radioButtonBalance.UseVisualStyleBackColor = true;
            // 
            // radioButtonFailoverServers
            // 
            this.radioButtonFailoverServers.AutoSize = true;
            this.radioButtonFailoverServers.Location = new System.Drawing.Point(24, 53);
            this.radioButtonFailoverServers.Name = "radioButtonFailoverServers";
            this.radioButtonFailoverServers.Size = new System.Drawing.Size(101, 17);
            this.radioButtonFailoverServers.TabIndex = 1;
            this.radioButtonFailoverServers.Text = "Failover Servers";
            this.radioButtonFailoverServers.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Choose Server Type:";
            // 
            // listBoxTable
            // 
            this.listBoxTable.FormattingEnabled = true;
            this.listBoxTable.Location = new System.Drawing.Point(145, 146);
            this.listBoxTable.Name = "listBoxTable";
            this.listBoxTable.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxTable.Size = new System.Drawing.Size(274, 173);
            this.listBoxTable.Sorted = true;
            this.listBoxTable.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Choose Table:";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(15, 327);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 15;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(96, 327);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormBigTableBatchInsert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 353);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listBoxTable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxBalanceServers);
            this.Name = "FormBigTableBatchInsert";
            this.Text = "BigTable--BatchInsert";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxBalanceServers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonFailoverServers;
        private System.Windows.Forms.RadioButton radioButtonBalance;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxTable;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
    }
}