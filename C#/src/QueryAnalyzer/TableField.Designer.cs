namespace QueryAnalyzer
{
    partial class TableField
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxDataType = new System.Windows.Forms.ComboBox();
            this.comboBoxIndexType = new System.Windows.Forms.ComboBox();
            this.numericUpDownDataLength = new System.Windows.Forms.NumericUpDown();
            this.comboBoxAnalyzer = new System.Windows.Forms.ComboBox();
            this.checkBoxNull = new System.Windows.Forms.CheckBox();
            this.checkBoxPK = new System.Windows.Forms.CheckBox();
            this.textBoxDefault = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxSelect = new System.Windows.Forms.CheckBox();
            this.textBoxFieldName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDataLength)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxDataType
            // 
            this.comboBoxDataType.FormattingEnabled = true;
            this.comboBoxDataType.Items.AddRange(new object[] {
            "TinyInt",
            "SmallInt",
            "Int",
            "BigInt",
            "Float",
            "Date",
            "SmallDateTime",
            "DateTime",
            "Varchar",
            "NVarchar",
            "Char",
            "NChar"});
            this.comboBoxDataType.Location = new System.Drawing.Point(155, 5);
            this.comboBoxDataType.Name = "comboBoxDataType";
            this.comboBoxDataType.Size = new System.Drawing.Size(102, 21);
            this.comboBoxDataType.TabIndex = 1;
            this.comboBoxDataType.SelectedIndexChanged += new System.EventHandler(this.comboBoxIndexType_SelectedIndexChanged);
            this.comboBoxDataType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxDataType_KeyPress);
            // 
            // comboBoxIndexType
            // 
            this.comboBoxIndexType.FormattingEnabled = true;
            this.comboBoxIndexType.Location = new System.Drawing.Point(329, 5);
            this.comboBoxIndexType.Name = "comboBoxIndexType";
            this.comboBoxIndexType.Size = new System.Drawing.Size(102, 21);
            this.comboBoxIndexType.TabIndex = 2;
            this.comboBoxIndexType.SelectedIndexChanged += new System.EventHandler(this.comboBoxIndexType_SelectedIndexChanged);
            this.comboBoxIndexType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxDataType_KeyPress);
            // 
            // numericUpDownDataLength
            // 
            this.numericUpDownDataLength.Location = new System.Drawing.Point(264, 5);
            this.numericUpDownDataLength.Maximum = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            this.numericUpDownDataLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDownDataLength.Name = "numericUpDownDataLength";
            this.numericUpDownDataLength.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownDataLength.TabIndex = 3;
            // 
            // comboBoxAnalyzer
            // 
            this.comboBoxAnalyzer.Enabled = false;
            this.comboBoxAnalyzer.FormattingEnabled = true;
            this.comboBoxAnalyzer.Location = new System.Drawing.Point(438, 5);
            this.comboBoxAnalyzer.Name = "comboBoxAnalyzer";
            this.comboBoxAnalyzer.Size = new System.Drawing.Size(102, 21);
            this.comboBoxAnalyzer.TabIndex = 4;
            this.comboBoxAnalyzer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxDataType_KeyPress);
            // 
            // checkBoxNull
            // 
            this.checkBoxNull.AutoSize = true;
            this.checkBoxNull.Location = new System.Drawing.Point(547, 8);
            this.checkBoxNull.Name = "checkBoxNull";
            this.checkBoxNull.Size = new System.Drawing.Size(54, 17);
            this.checkBoxNull.TabIndex = 5;
            this.checkBoxNull.Text = "NULL";
            this.checkBoxNull.UseVisualStyleBackColor = true;
            this.checkBoxNull.CheckedChanged += new System.EventHandler(this.checkBoxNull_CheckedChanged);
            // 
            // checkBoxPK
            // 
            this.checkBoxPK.AutoSize = true;
            this.checkBoxPK.Location = new System.Drawing.Point(607, 8);
            this.checkBoxPK.Name = "checkBoxPK";
            this.checkBoxPK.Size = new System.Drawing.Size(40, 17);
            this.checkBoxPK.TabIndex = 6;
            this.checkBoxPK.Text = "PK";
            this.checkBoxPK.UseVisualStyleBackColor = true;
            // 
            // textBoxDefault
            // 
            this.textBoxDefault.Location = new System.Drawing.Point(653, 6);
            this.textBoxDefault.Name = "textBoxDefault";
            this.textBoxDefault.Size = new System.Drawing.Size(96, 20);
            this.textBoxDefault.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.checkBoxSelect);
            this.panel1.Controls.Add(this.textBoxFieldName);
            this.panel1.Controls.Add(this.textBoxDefault);
            this.panel1.Controls.Add(this.checkBoxPK);
            this.panel1.Controls.Add(this.comboBoxDataType);
            this.panel1.Controls.Add(this.checkBoxNull);
            this.panel1.Controls.Add(this.comboBoxIndexType);
            this.panel1.Controls.Add(this.comboBoxAnalyzer);
            this.panel1.Controls.Add(this.numericUpDownDataLength);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 34);
            this.panel1.TabIndex = 8;
            // 
            // checkBoxSelect
            // 
            this.checkBoxSelect.AutoSize = true;
            this.checkBoxSelect.Location = new System.Drawing.Point(3, 9);
            this.checkBoxSelect.Name = "checkBoxSelect";
            this.checkBoxSelect.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSelect.TabIndex = 9;
            this.checkBoxSelect.UseVisualStyleBackColor = true;
            // 
            // textBoxFieldName
            // 
            this.textBoxFieldName.Location = new System.Drawing.Point(21, 6);
            this.textBoxFieldName.Name = "textBoxFieldName";
            this.textBoxFieldName.Size = new System.Drawing.Size(128, 20);
            this.textBoxFieldName.TabIndex = 8;
            // 
            // TableField
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "TableField";
            this.Size = new System.Drawing.Size(800, 34);
            this.Load += new System.EventHandler(this.TableField_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDataLength)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.ComboBox comboBoxDataType;
        internal System.Windows.Forms.ComboBox comboBoxIndexType;
        internal System.Windows.Forms.NumericUpDown numericUpDownDataLength;
        internal System.Windows.Forms.ComboBox comboBoxAnalyzer;
        internal System.Windows.Forms.CheckBox checkBoxNull;
        internal System.Windows.Forms.CheckBox checkBoxPK;
        internal System.Windows.Forms.TextBox textBoxDefault;
        internal System.Windows.Forms.CheckBox checkBoxSelect;
        internal System.Windows.Forms.TextBox textBoxFieldName;
    }
}
