namespace QueryAnalyzer
{
    partial class FormPerformance
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
            this.textBoxSql = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTest = new System.Windows.Forms.Button();
            this.numericUpDownIteration = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelDuration = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxDataCache = new System.Windows.Forms.GroupBox();
            this.numericUpDownDataCache = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxDataCache = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIteration)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBoxDataCache.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDataCache)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxSql
            // 
            this.textBoxSql.Location = new System.Drawing.Point(25, 39);
            this.textBoxSql.MaxLength = 327670;
            this.textBoxSql.Multiline = true;
            this.textBoxSql.Name = "textBoxSql";
            this.textBoxSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSql.Size = new System.Drawing.Size(560, 200);
            this.textBoxSql.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "SQL";
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(27, 384);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 27;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // numericUpDownIteration
            // 
            this.numericUpDownIteration.Location = new System.Drawing.Point(80, 27);
            this.numericUpDownIteration.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownIteration.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownIteration.Name = "numericUpDownIteration";
            this.numericUpDownIteration.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownIteration.TabIndex = 28;
            this.numericUpDownIteration.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Iteration";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 30;
            this.label3.Text = "Duration";
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(77, 69);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(29, 13);
            this.labelDuration.TabIndex = 31;
            this.labelDuration.Text = "0 ms";
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(128, 384);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 32;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(224, 383);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 33;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDownIteration);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.labelDuration);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(27, 245);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Test";
            // 
            // groupBoxDataCache
            // 
            this.groupBoxDataCache.Controls.Add(this.checkBoxDataCache);
            this.groupBoxDataCache.Controls.Add(this.label5);
            this.groupBoxDataCache.Controls.Add(this.numericUpDownDataCache);
            this.groupBoxDataCache.Controls.Add(this.label4);
            this.groupBoxDataCache.Enabled = false;
            this.groupBoxDataCache.Location = new System.Drawing.Point(292, 245);
            this.groupBoxDataCache.Name = "groupBoxDataCache";
            this.groupBoxDataCache.Size = new System.Drawing.Size(200, 100);
            this.groupBoxDataCache.TabIndex = 35;
            this.groupBoxDataCache.TabStop = false;
            this.groupBoxDataCache.Text = "Data cache";
            // 
            // numericUpDownDataCache
            // 
            this.numericUpDownDataCache.Location = new System.Drawing.Point(84, 29);
            this.numericUpDownDataCache.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDataCache.Name = "numericUpDownDataCache";
            this.numericUpDownDataCache.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownDataCache.TabIndex = 30;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 31;
            this.label4.Text = "Time out";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(152, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(12, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "s";
            // 
            // checkBoxDataCache
            // 
            this.checkBoxDataCache.AutoSize = true;
            this.checkBoxDataCache.Location = new System.Drawing.Point(6, 0);
            this.checkBoxDataCache.Name = "checkBoxDataCache";
            this.checkBoxDataCache.Size = new System.Drawing.Size(82, 17);
            this.checkBoxDataCache.TabIndex = 33;
            this.checkBoxDataCache.Text = "Data cache";
            this.checkBoxDataCache.UseVisualStyleBackColor = true;
            this.checkBoxDataCache.CheckedChanged += new System.EventHandler(this.checkBoxDataCache_CheckedChanged);
            // 
            // FormPerformance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 418);
            this.Controls.Add(this.groupBoxDataCache);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSql);
            this.Name = "FormPerformance";
            this.Text = "Performance Test";
            this.Load += new System.EventHandler(this.FormPerformance_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIteration)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxDataCache.ResumeLayout(false);
            this.groupBoxDataCache.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDataCache)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSql;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.NumericUpDown numericUpDownIteration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxDataCache;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownDataCache;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxDataCache;
    }
}