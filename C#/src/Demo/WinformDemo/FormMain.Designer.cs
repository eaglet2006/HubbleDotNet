namespace WinformDemo
{
    partial class FormMain
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
            this.textBoxNewsTableDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labelDuration = new System.Windows.Forms.Label();
            this.buttonDrop = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonAppend = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonOptimize = new System.Windows.Forms.Button();
            this.buttonOptimizeMin = new System.Windows.Forms.Button();
            this.textBoxSql = new System.Windows.Forms.TextBox();
            this.buttonExcute = new System.Windows.Forms.Button();
            this.dataGridViewResult = new System.Windows.Forms.DataGridView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageResults = new System.Windows.Forms.TabPage();
            this.tabPageMessages = new System.Windows.Forms.TabPage();
            this.textBoxMessages = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageResults.SuspendLayout();
            this.tabPageMessages.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxNewsTableDir
            // 
            this.textBoxNewsTableDir.Location = new System.Drawing.Point(133, 12);
            this.textBoxNewsTableDir.Name = "textBoxNewsTableDir";
            this.textBoxNewsTableDir.Size = new System.Drawing.Size(260, 20);
            this.textBoxNewsTableDir.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "News Table Directory";
            // 
            // buttonCreate
            // 
            this.buttonCreate.Location = new System.Drawing.Point(21, 42);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(75, 23);
            this.buttonCreate.TabIndex = 2;
            this.buttonCreate.Text = "Create";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(111, 42);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(75, 23);
            this.buttonOpen.TabIndex = 3;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(135, 71);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(231, 20);
            this.textBoxSearch.TabIndex = 4;
            this.textBoxSearch.Text = "Content Match 大学";
            this.textBoxSearch.Visible = false;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(385, 69);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 5;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Visible = false;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(115, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Duration";
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(168, 99);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(29, 13);
            this.labelDuration.TabIndex = 7;
            this.labelDuration.Text = "0 ms";
            // 
            // buttonDrop
            // 
            this.buttonDrop.Location = new System.Drawing.Point(201, 42);
            this.buttonDrop.Name = "buttonDrop";
            this.buttonDrop.Size = new System.Drawing.Size(75, 23);
            this.buttonDrop.TabIndex = 8;
            this.buttonDrop.Text = "Drop";
            this.buttonDrop.UseVisualStyleBackColor = true;
            this.buttonDrop.Click += new System.EventHandler(this.buttonDrop_Click);
            // 
            // labelCount
            // 
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(71, 99);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(13, 13);
            this.labelCount.TabIndex = 10;
            this.labelCount.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Count";
            // 
            // buttonAppend
            // 
            this.buttonAppend.Location = new System.Drawing.Point(291, 42);
            this.buttonAppend.Name = "buttonAppend";
            this.buttonAppend.Size = new System.Drawing.Size(75, 23);
            this.buttonAppend.TabIndex = 11;
            this.buttonAppend.Text = "Append";
            this.buttonAppend.UseVisualStyleBackColor = true;
            this.buttonAppend.Click += new System.EventHandler(this.buttonAppend_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(385, 42);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 12;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonOptimize
            // 
            this.buttonOptimize.Location = new System.Drawing.Point(480, 42);
            this.buttonOptimize.Name = "buttonOptimize";
            this.buttonOptimize.Size = new System.Drawing.Size(75, 23);
            this.buttonOptimize.TabIndex = 13;
            this.buttonOptimize.Text = "Optimize";
            this.buttonOptimize.UseVisualStyleBackColor = true;
            this.buttonOptimize.Click += new System.EventHandler(this.buttonOptimize_Click);
            // 
            // buttonOptimizeMin
            // 
            this.buttonOptimizeMin.Location = new System.Drawing.Point(480, 71);
            this.buttonOptimizeMin.Name = "buttonOptimizeMin";
            this.buttonOptimizeMin.Size = new System.Drawing.Size(75, 23);
            this.buttonOptimizeMin.TabIndex = 14;
            this.buttonOptimizeMin.Text = "Optimize Min";
            this.buttonOptimizeMin.UseVisualStyleBackColor = true;
            this.buttonOptimizeMin.Click += new System.EventHandler(this.buttonOptimizeMin_Click);
            // 
            // textBoxSql
            // 
            this.textBoxSql.Location = new System.Drawing.Point(21, 118);
            this.textBoxSql.Multiline = true;
            this.textBoxSql.Name = "textBoxSql";
            this.textBoxSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSql.Size = new System.Drawing.Size(458, 100);
            this.textBoxSql.TabIndex = 15;
            // 
            // buttonExcute
            // 
            this.buttonExcute.Location = new System.Drawing.Point(485, 118);
            this.buttonExcute.Name = "buttonExcute";
            this.buttonExcute.Size = new System.Drawing.Size(75, 23);
            this.buttonExcute.TabIndex = 16;
            this.buttonExcute.Text = "Excute";
            this.buttonExcute.UseVisualStyleBackColor = true;
            this.buttonExcute.Click += new System.EventHandler(this.buttonExcute_Click);
            // 
            // dataGridViewResult
            // 
            this.dataGridViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewResult.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewResult.Name = "dataGridViewResult";
            this.dataGridViewResult.Size = new System.Drawing.Size(600, 257);
            this.dataGridViewResult.TabIndex = 17;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageResults);
            this.tabControl1.Controls.Add(this.tabPageMessages);
            this.tabControl1.Location = new System.Drawing.Point(21, 224);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(614, 289);
            this.tabControl1.TabIndex = 18;
            // 
            // tabPageResults
            // 
            this.tabPageResults.Controls.Add(this.dataGridViewResult);
            this.tabPageResults.Location = new System.Drawing.Point(4, 22);
            this.tabPageResults.Name = "tabPageResults";
            this.tabPageResults.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageResults.Size = new System.Drawing.Size(606, 263);
            this.tabPageResults.TabIndex = 0;
            this.tabPageResults.Text = "Results";
            this.tabPageResults.UseVisualStyleBackColor = true;
            // 
            // tabPageMessages
            // 
            this.tabPageMessages.Controls.Add(this.textBoxMessages);
            this.tabPageMessages.Location = new System.Drawing.Point(4, 22);
            this.tabPageMessages.Name = "tabPageMessages";
            this.tabPageMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMessages.Size = new System.Drawing.Size(606, 263);
            this.tabPageMessages.TabIndex = 1;
            this.tabPageMessages.Text = "Messages";
            this.tabPageMessages.UseVisualStyleBackColor = true;
            // 
            // textBoxMessages
            // 
            this.textBoxMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxMessages.Location = new System.Drawing.Point(3, 3);
            this.textBoxMessages.Multiline = true;
            this.textBoxMessages.Name = "textBoxMessages";
            this.textBoxMessages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMessages.Size = new System.Drawing.Size(600, 257);
            this.textBoxMessages.TabIndex = 0;
            this.textBoxMessages.WordWrap = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(485, 165);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 19;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 525);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonExcute);
            this.Controls.Add(this.textBoxSql);
            this.Controls.Add(this.buttonOptimizeMin);
            this.Controls.Add(this.buttonOptimize);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAppend);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonDrop);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.textBoxNewsTableDir);
            this.Controls.Add(this.label1);
            this.Name = "FormMain";
            this.Text = "Main";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageResults.ResumeLayout(false);
            this.tabPageMessages.ResumeLayout(false);
            this.tabPageMessages.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxNewsTableDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Button buttonDrop;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonAppend;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonOptimize;
        private System.Windows.Forms.Button buttonOptimizeMin;
        private System.Windows.Forms.TextBox textBoxSql;
        private System.Windows.Forms.Button buttonExcute;
        private System.Windows.Forms.DataGridView dataGridViewResult;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageResults;
        private System.Windows.Forms.TabPage tabPageMessages;
        private System.Windows.Forms.TextBox textBoxMessages;
        private System.Windows.Forms.Button button1;
    }
}