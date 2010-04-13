namespace QueryAnalyzer
{
    partial class FormTableInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTableInfo));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageFields = new System.Windows.Forms.TabPage();
            this.panelHead = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPageAttributes = new System.Windows.Forms.TabPage();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.numericUpDownQueryCacheTimeout = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.comboBoxQueryCacheEnabled = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.buttonInfoCancel = new System.Windows.Forms.Button();
            this.buttonSet = new System.Windows.Forms.Button();
            this.numericUpDownCleanupQueryCacheFileInDays = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxReturnCount = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownLastDocId = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.comboBoxIndexOnly = new System.Windows.Forms.ComboBox();
            this.comboBoxStoreQueryCacheInFile = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.textBoxDBAdapter = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxDBTableName = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxDocId = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxDirectory = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageFields.SuspendLayout();
            this.panelHead.SuspendLayout();
            this.tabPageAttributes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryCacheTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCleanupQueryCacheFileInDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxReturnCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLastDocId)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageFields);
            this.tabControl1.Controls.Add(this.tabPageAttributes);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(841, 566);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPageFields
            // 
            this.tabPageFields.AutoScroll = true;
            this.tabPageFields.Controls.Add(this.panelHead);
            this.tabPageFields.Location = new System.Drawing.Point(4, 22);
            this.tabPageFields.Name = "tabPageFields";
            this.tabPageFields.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFields.Size = new System.Drawing.Size(833, 540);
            this.tabPageFields.TabIndex = 0;
            this.tabPageFields.Text = "Fields";
            this.tabPageFields.UseVisualStyleBackColor = true;
            // 
            // panelHead
            // 
            this.panelHead.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHead.Controls.Add(this.label8);
            this.panelHead.Controls.Add(this.label1);
            this.panelHead.Controls.Add(this.label7);
            this.panelHead.Controls.Add(this.label2);
            this.panelHead.Controls.Add(this.label6);
            this.panelHead.Controls.Add(this.label3);
            this.panelHead.Controls.Add(this.label5);
            this.panelHead.Controls.Add(this.label4);
            this.panelHead.Location = new System.Drawing.Point(24, 13);
            this.panelHead.Name = "panelHead";
            this.panelHead.Size = new System.Drawing.Size(800, 34);
            this.panelHead.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(654, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Default";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(46, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Field Name";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(608, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "PK";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(156, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Data Type";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(548, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "NULL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(263, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Data Length";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(345, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Index Type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(439, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Analyzer";
            // 
            // tabPageAttributes
            // 
            this.tabPageAttributes.Controls.Add(this.label21);
            this.tabPageAttributes.Controls.Add(this.label20);
            this.tabPageAttributes.Controls.Add(this.numericUpDownQueryCacheTimeout);
            this.tabPageAttributes.Controls.Add(this.label18);
            this.tabPageAttributes.Controls.Add(this.comboBoxQueryCacheEnabled);
            this.tabPageAttributes.Controls.Add(this.label19);
            this.tabPageAttributes.Controls.Add(this.buttonInfoCancel);
            this.tabPageAttributes.Controls.Add(this.buttonSet);
            this.tabPageAttributes.Controls.Add(this.numericUpDownCleanupQueryCacheFileInDays);
            this.tabPageAttributes.Controls.Add(this.numericUpDownMaxReturnCount);
            this.tabPageAttributes.Controls.Add(this.numericUpDownLastDocId);
            this.tabPageAttributes.Controls.Add(this.label17);
            this.tabPageAttributes.Controls.Add(this.comboBoxIndexOnly);
            this.tabPageAttributes.Controls.Add(this.comboBoxStoreQueryCacheInFile);
            this.tabPageAttributes.Controls.Add(this.label16);
            this.tabPageAttributes.Controls.Add(this.label15);
            this.tabPageAttributes.Controls.Add(this.label14);
            this.tabPageAttributes.Controls.Add(this.textBoxDBAdapter);
            this.tabPageAttributes.Controls.Add(this.label13);
            this.tabPageAttributes.Controls.Add(this.textBoxDBTableName);
            this.tabPageAttributes.Controls.Add(this.label12);
            this.tabPageAttributes.Controls.Add(this.textBoxDocId);
            this.tabPageAttributes.Controls.Add(this.label11);
            this.tabPageAttributes.Controls.Add(this.label10);
            this.tabPageAttributes.Controls.Add(this.textBoxDirectory);
            this.tabPageAttributes.Controls.Add(this.label9);
            this.tabPageAttributes.Location = new System.Drawing.Point(4, 22);
            this.tabPageAttributes.Name = "tabPageAttributes";
            this.tabPageAttributes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAttributes.Size = new System.Drawing.Size(833, 540);
            this.tabPageAttributes.TabIndex = 1;
            this.tabPageAttributes.Text = "Attributes";
            this.tabPageAttributes.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(308, 390);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(29, 13);
            this.label21.TabIndex = 29;
            this.label21.Text = "days";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(308, 316);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(20, 13);
            this.label20.TabIndex = 28;
            this.label20.Text = "ms";
            // 
            // numericUpDownQueryCacheTimeout
            // 
            this.numericUpDownQueryCacheTimeout.Location = new System.Drawing.Point(168, 311);
            this.numericUpDownQueryCacheTimeout.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numericUpDownQueryCacheTimeout.Name = "numericUpDownQueryCacheTimeout";
            this.numericUpDownQueryCacheTimeout.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownQueryCacheTimeout.TabIndex = 27;
            this.numericUpDownQueryCacheTimeout.Tag = "QueryCacheTimeout";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(32, 316);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(104, 13);
            this.label18.TabIndex = 26;
            this.label18.Text = "QueryCacheTimeout";
            // 
            // comboBoxQueryCacheEnabled
            // 
            this.comboBoxQueryCacheEnabled.FormattingEnabled = true;
            this.comboBoxQueryCacheEnabled.Items.AddRange(new object[] {
            "True",
            "False"});
            this.comboBoxQueryCacheEnabled.Location = new System.Drawing.Point(168, 274);
            this.comboBoxQueryCacheEnabled.Name = "comboBoxQueryCacheEnabled";
            this.comboBoxQueryCacheEnabled.Size = new System.Drawing.Size(121, 21);
            this.comboBoxQueryCacheEnabled.TabIndex = 25;
            this.comboBoxQueryCacheEnabled.Tag = "QueryCacheEnabled";
            this.comboBoxQueryCacheEnabled.Text = "True";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(32, 279);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(105, 13);
            this.label19.TabIndex = 24;
            this.label19.Text = "QueryCacheEnabled";
            // 
            // buttonInfoCancel
            // 
            this.buttonInfoCancel.Location = new System.Drawing.Point(129, 466);
            this.buttonInfoCancel.Name = "buttonInfoCancel";
            this.buttonInfoCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonInfoCancel.TabIndex = 23;
            this.buttonInfoCancel.Text = "Cancel";
            this.buttonInfoCancel.UseVisualStyleBackColor = true;
            this.buttonInfoCancel.Click += new System.EventHandler(this.buttonInfoCancel_Click);
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(35, 466);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(75, 23);
            this.buttonSet.TabIndex = 22;
            this.buttonSet.Text = "Set";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
            // 
            // numericUpDownCleanupQueryCacheFileInDays
            // 
            this.numericUpDownCleanupQueryCacheFileInDays.Location = new System.Drawing.Point(193, 385);
            this.numericUpDownCleanupQueryCacheFileInDays.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numericUpDownCleanupQueryCacheFileInDays.Name = "numericUpDownCleanupQueryCacheFileInDays";
            this.numericUpDownCleanupQueryCacheFileInDays.Size = new System.Drawing.Size(96, 20);
            this.numericUpDownCleanupQueryCacheFileInDays.TabIndex = 21;
            this.numericUpDownCleanupQueryCacheFileInDays.Tag = "CleanupQueryCacheFileInDays";
            // 
            // numericUpDownMaxReturnCount
            // 
            this.numericUpDownMaxReturnCount.Location = new System.Drawing.Point(168, 238);
            this.numericUpDownMaxReturnCount.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numericUpDownMaxReturnCount.Minimum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownMaxReturnCount.Name = "numericUpDownMaxReturnCount";
            this.numericUpDownMaxReturnCount.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownMaxReturnCount.TabIndex = 20;
            this.numericUpDownMaxReturnCount.Tag = "MaxReturnCount";
            this.numericUpDownMaxReturnCount.Value = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            // 
            // numericUpDownLastDocId
            // 
            this.numericUpDownLastDocId.Location = new System.Drawing.Point(169, 199);
            this.numericUpDownLastDocId.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numericUpDownLastDocId.Name = "numericUpDownLastDocId";
            this.numericUpDownLastDocId.ReadOnly = true;
            this.numericUpDownLastDocId.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownLastDocId.TabIndex = 19;
            this.numericUpDownLastDocId.Tag = "LastDocId";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(32, 390);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(154, 13);
            this.label17.TabIndex = 17;
            this.label17.Text = "CleanupQueryCacheFileInDays";
            // 
            // comboBoxIndexOnly
            // 
            this.comboBoxIndexOnly.Enabled = false;
            this.comboBoxIndexOnly.FormattingEnabled = true;
            this.comboBoxIndexOnly.Items.AddRange(new object[] {
            "True",
            "False"});
            this.comboBoxIndexOnly.Location = new System.Drawing.Point(168, 54);
            this.comboBoxIndexOnly.Name = "comboBoxIndexOnly";
            this.comboBoxIndexOnly.Size = new System.Drawing.Size(121, 21);
            this.comboBoxIndexOnly.TabIndex = 16;
            this.comboBoxIndexOnly.Tag = "IndexOnly";
            this.comboBoxIndexOnly.Text = "True";
            // 
            // comboBoxStoreQueryCacheInFile
            // 
            this.comboBoxStoreQueryCacheInFile.FormattingEnabled = true;
            this.comboBoxStoreQueryCacheInFile.Items.AddRange(new object[] {
            "True",
            "False"});
            this.comboBoxStoreQueryCacheInFile.Location = new System.Drawing.Point(168, 348);
            this.comboBoxStoreQueryCacheInFile.Name = "comboBoxStoreQueryCacheInFile";
            this.comboBoxStoreQueryCacheInFile.Size = new System.Drawing.Size(121, 21);
            this.comboBoxStoreQueryCacheInFile.TabIndex = 15;
            this.comboBoxStoreQueryCacheInFile.Tag = "StoreQueryCacheInFile";
            this.comboBoxStoreQueryCacheInFile.Text = "True";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(32, 353);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(116, 13);
            this.label16.TabIndex = 14;
            this.label16.Text = "StoreQueryCacheInFile";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(32, 242);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(87, 13);
            this.label15.TabIndex = 12;
            this.label15.Text = "MaxReturnCount";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(32, 205);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(56, 13);
            this.label14.TabIndex = 10;
            this.label14.Text = "LastDocId";
            // 
            // textBoxDBAdapter
            // 
            this.textBoxDBAdapter.Location = new System.Drawing.Point(168, 163);
            this.textBoxDBAdapter.Name = "textBoxDBAdapter";
            this.textBoxDBAdapter.ReadOnly = true;
            this.textBoxDBAdapter.Size = new System.Drawing.Size(629, 20);
            this.textBoxDBAdapter.TabIndex = 9;
            this.textBoxDBAdapter.Tag = "DBAdapter";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(32, 168);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(59, 13);
            this.label13.TabIndex = 8;
            this.label13.Text = "DBAdapter";
            // 
            // textBoxDBTableName
            // 
            this.textBoxDBTableName.Location = new System.Drawing.Point(168, 127);
            this.textBoxDBTableName.Name = "textBoxDBTableName";
            this.textBoxDBTableName.ReadOnly = true;
            this.textBoxDBTableName.Size = new System.Drawing.Size(629, 20);
            this.textBoxDBTableName.TabIndex = 7;
            this.textBoxDBTableName.Tag = "DBTableName";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(32, 131);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(81, 13);
            this.label12.TabIndex = 6;
            this.label12.Text = "DB Table name";
            // 
            // textBoxDocId
            // 
            this.textBoxDocId.Location = new System.Drawing.Point(168, 91);
            this.textBoxDocId.Name = "textBoxDocId";
            this.textBoxDocId.ReadOnly = true;
            this.textBoxDocId.Size = new System.Drawing.Size(629, 20);
            this.textBoxDocId.TabIndex = 5;
            this.textBoxDocId.Tag = "DocId";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(32, 94);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(61, 13);
            this.label11.TabIndex = 4;
            this.label11.Text = "DocId Field";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(32, 57);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Index Only";
            // 
            // textBoxDirectory
            // 
            this.textBoxDirectory.Location = new System.Drawing.Point(168, 18);
            this.textBoxDirectory.Name = "textBoxDirectory";
            this.textBoxDirectory.ReadOnly = true;
            this.textBoxDirectory.Size = new System.Drawing.Size(629, 20);
            this.textBoxDirectory.TabIndex = 1;
            this.textBoxDirectory.Tag = "Directory";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(32, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Index Directory";
            // 
            // FormTableInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 566);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormTableInfo";
            this.Text = "Table Info";
            this.Load += new System.EventHandler(this.FormTableInfo_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageFields.ResumeLayout(false);
            this.panelHead.ResumeLayout(false);
            this.panelHead.PerformLayout();
            this.tabPageAttributes.ResumeLayout(false);
            this.tabPageAttributes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryCacheTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCleanupQueryCacheFileInDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxReturnCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLastDocId)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageFields;
        private System.Windows.Forms.TabPage tabPageAttributes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelHead;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxDirectory;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxDBAdapter;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxDBTableName;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBoxDocId;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numericUpDownLastDocId;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox comboBoxIndexOnly;
        private System.Windows.Forms.ComboBox comboBoxStoreQueryCacheInFile;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.NumericUpDown numericUpDownCleanupQueryCacheFileInDays;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxReturnCount;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.Button buttonInfoCancel;
        private System.Windows.Forms.NumericUpDown numericUpDownQueryCacheTimeout;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox comboBoxQueryCacheEnabled;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
    }
}