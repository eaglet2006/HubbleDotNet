namespace QueryAnalyzer
{
    partial class FormRebuildTable
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
            this.labelTableName = new System.Windows.Forms.Label();
            this.labelDbAdapterName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelIndexOnly = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelCurrentCount = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonRebuild = new System.Windows.Forms.Button();
            this.groupBoxSetting = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownSleepRows = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDownSleepSeconds = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownImportCount = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDownStep = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxRebuildWholeTable = new System.Windows.Forms.CheckBox();
            this.numericUpDownDocIdFrom = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonStop = new System.Windows.Forms.Button();
            this.labelOptimizeProgress = new System.Windows.Forms.Label();
            this.groupBoxSetting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownImportCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDocIdFrom)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Table Name:";
            // 
            // labelTableName
            // 
            this.labelTableName.AutoSize = true;
            this.labelTableName.Location = new System.Drawing.Point(132, 27);
            this.labelTableName.Name = "labelTableName";
            this.labelTableName.Size = new System.Drawing.Size(33, 13);
            this.labelTableName.TabIndex = 1;
            this.labelTableName.Text = "name";
            // 
            // labelDbAdapterName
            // 
            this.labelDbAdapterName.AutoSize = true;
            this.labelDbAdapterName.Location = new System.Drawing.Point(132, 79);
            this.labelDbAdapterName.Name = "labelDbAdapterName";
            this.labelDbAdapterName.Size = new System.Drawing.Size(33, 13);
            this.labelDbAdapterName.TabIndex = 3;
            this.labelDbAdapterName.Text = "name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "DBAdapter Name:";
            // 
            // labelIndexOnly
            // 
            this.labelIndexOnly.AutoSize = true;
            this.labelIndexOnly.Location = new System.Drawing.Point(132, 54);
            this.labelIndexOnly.Name = "labelIndexOnly";
            this.labelIndexOnly.Size = new System.Drawing.Size(32, 13);
            this.labelIndexOnly.TabIndex = 7;
            this.labelIndexOnly.Text = "False";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "IndexOnly";
            // 
            // labelCurrentCount
            // 
            this.labelCurrentCount.AutoSize = true;
            this.labelCurrentCount.Location = new System.Drawing.Point(132, 106);
            this.labelCurrentCount.Name = "labelCurrentCount";
            this.labelCurrentCount.Size = new System.Drawing.Size(13, 13);
            this.labelCurrentCount.TabIndex = 10;
            this.labelCurrentCount.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 105);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Rebuilding records:";
            // 
            // buttonRebuild
            // 
            this.buttonRebuild.Location = new System.Drawing.Point(36, 360);
            this.buttonRebuild.Name = "buttonRebuild";
            this.buttonRebuild.Size = new System.Drawing.Size(75, 23);
            this.buttonRebuild.TabIndex = 11;
            this.buttonRebuild.Text = "Rebuild";
            this.buttonRebuild.UseVisualStyleBackColor = true;
            this.buttonRebuild.Click += new System.EventHandler(this.buttonRebuild_Click);
            // 
            // groupBoxSetting
            // 
            this.groupBoxSetting.Controls.Add(this.label10);
            this.groupBoxSetting.Controls.Add(this.numericUpDownSleepRows);
            this.groupBoxSetting.Controls.Add(this.label9);
            this.groupBoxSetting.Controls.Add(this.numericUpDownSleepSeconds);
            this.groupBoxSetting.Controls.Add(this.label8);
            this.groupBoxSetting.Controls.Add(this.numericUpDownImportCount);
            this.groupBoxSetting.Controls.Add(this.label7);
            this.groupBoxSetting.Controls.Add(this.numericUpDownStep);
            this.groupBoxSetting.Controls.Add(this.label4);
            this.groupBoxSetting.Controls.Add(this.checkBoxRebuildWholeTable);
            this.groupBoxSetting.Controls.Add(this.numericUpDownDocIdFrom);
            this.groupBoxSetting.Controls.Add(this.label2);
            this.groupBoxSetting.Location = new System.Drawing.Point(36, 147);
            this.groupBoxSetting.Name = "groupBoxSetting";
            this.groupBoxSetting.Size = new System.Drawing.Size(390, 207);
            this.groupBoxSetting.TabIndex = 16;
            this.groupBoxSetting.TabStop = false;
            this.groupBoxSetting.Text = "Rebuild Settings";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(312, 179);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Rows";
            // 
            // numericUpDownSleepRows
            // 
            this.numericUpDownSleepRows.Location = new System.Drawing.Point(220, 175);
            this.numericUpDownSleepRows.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.numericUpDownSleepRows.Name = "numericUpDownSleepRows";
            this.numericUpDownSleepRows.Size = new System.Drawing.Size(93, 20);
            this.numericUpDownSleepRows.TabIndex = 26;
            this.numericUpDownSleepRows.Value = new decimal(new int[] {
            200000,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(134, 179);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Seconds every";
            // 
            // numericUpDownSleepSeconds
            // 
            this.numericUpDownSleepSeconds.Location = new System.Drawing.Point(64, 176);
            this.numericUpDownSleepSeconds.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.numericUpDownSleepSeconds.Name = "numericUpDownSleepSeconds";
            this.numericUpDownSleepSeconds.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownSleepSeconds.TabIndex = 24;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 178);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Sleep";
            // 
            // numericUpDownImportCount
            // 
            this.numericUpDownImportCount.Location = new System.Drawing.Point(124, 135);
            this.numericUpDownImportCount.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.numericUpDownImportCount.Name = "numericUpDownImportCount";
            this.numericUpDownImportCount.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownImportCount.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 140);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Import count";
            // 
            // numericUpDownStep
            // 
            this.numericUpDownStep.Location = new System.Drawing.Point(124, 96);
            this.numericUpDownStep.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownStep.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownStep.Name = "numericUpDownStep";
            this.numericUpDownStep.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownStep.TabIndex = 20;
            this.numericUpDownStep.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Step";
            // 
            // checkBoxRebuildWholeTable
            // 
            this.checkBoxRebuildWholeTable.AutoSize = true;
            this.checkBoxRebuildWholeTable.Location = new System.Drawing.Point(25, 34);
            this.checkBoxRebuildWholeTable.Name = "checkBoxRebuildWholeTable";
            this.checkBoxRebuildWholeTable.Size = new System.Drawing.Size(120, 17);
            this.checkBoxRebuildWholeTable.TabIndex = 18;
            this.checkBoxRebuildWholeTable.Text = "RebuildWholeTable";
            this.checkBoxRebuildWholeTable.UseVisualStyleBackColor = true;
            this.checkBoxRebuildWholeTable.CheckedChanged += new System.EventHandler(this.checkBoxRebuildWholeTable_CheckedChanged);
            // 
            // numericUpDownDocIdFrom
            // 
            this.numericUpDownDocIdFrom.Location = new System.Drawing.Point(124, 60);
            this.numericUpDownDocIdFrom.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.numericUpDownDocIdFrom.Name = "numericUpDownDocIdFrom";
            this.numericUpDownDocIdFrom.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownDocIdFrom.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "DocId from:";
            // 
            // buttonStop
            // 
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(135, 360);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 17;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // labelOptimizeProgress
            // 
            this.labelOptimizeProgress.AutoSize = true;
            this.labelOptimizeProgress.ForeColor = System.Drawing.Color.Red;
            this.labelOptimizeProgress.Location = new System.Drawing.Point(32, 125);
            this.labelOptimizeProgress.Name = "labelOptimizeProgress";
            this.labelOptimizeProgress.Size = new System.Drawing.Size(91, 13);
            this.labelOptimizeProgress.TabIndex = 18;
            this.labelOptimizeProgress.Text = "Optimize Progress";
            // 
            // FormRebuildTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 395);
            this.Controls.Add(this.labelOptimizeProgress);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.groupBoxSetting);
            this.Controls.Add(this.buttonRebuild);
            this.Controls.Add(this.labelCurrentCount);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.labelIndexOnly);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelDbAdapterName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelTableName);
            this.Controls.Add(this.label1);
            this.Name = "FormRebuildTable";
            this.Text = "Rebuild Table";
            this.Load += new System.EventHandler(this.FormRebuildTable_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRebuildTable_FormClosing);
            this.groupBoxSetting.ResumeLayout(false);
            this.groupBoxSetting.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownImportCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDocIdFrom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelTableName;
        private System.Windows.Forms.Label labelDbAdapterName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelIndexOnly;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelCurrentCount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonRebuild;
        private System.Windows.Forms.GroupBox groupBoxSetting;
        private System.Windows.Forms.NumericUpDown numericUpDownImportCount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDownStep;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxRebuildWholeTable;
        private System.Windows.Forms.NumericUpDown numericUpDownDocIdFrom;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownSleepRows;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDownSleepSeconds;
        private System.Windows.Forms.Label labelOptimizeProgress;
    }
}