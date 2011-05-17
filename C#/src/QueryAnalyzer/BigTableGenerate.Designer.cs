namespace QueryAnalyzer
{
    partial class BigTableGenerate
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageGlobal = new System.Windows.Forms.TabPage();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxIndexFolder = new System.Windows.Forms.TextBox();
            this.tabPageTables = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxBalanceServers = new System.Windows.Forms.GroupBox();
            this.buttonDeleteBS = new System.Windows.Forms.Button();
            this.listBoxBlanceServer = new System.Windows.Forms.ListBox();
            this.buttonAddBS = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.listBoxTablets = new System.Windows.Forms.ListBox();
            this.tabControl.SuspendLayout();
            this.tabPageGlobal.SuspendLayout();
            this.tabPageTables.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBoxBalanceServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageGlobal);
            this.tabControl.Controls.Add(this.tabPageTables);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(684, 413);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageGlobal
            // 
            this.tabPageGlobal.Controls.Add(this.buttonCancel);
            this.tabPageGlobal.Controls.Add(this.buttonSave);
            this.tabPageGlobal.Controls.Add(this.label1);
            this.tabPageGlobal.Controls.Add(this.textBoxTableName);
            this.tabPageGlobal.Controls.Add(this.label8);
            this.tabPageGlobal.Controls.Add(this.textBoxIndexFolder);
            this.tabPageGlobal.Location = new System.Drawing.Point(4, 22);
            this.tabPageGlobal.Name = "tabPageGlobal";
            this.tabPageGlobal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGlobal.Size = new System.Drawing.Size(676, 387);
            this.tabPageGlobal.TabIndex = 0;
            this.tabPageGlobal.Text = "Global Setting";
            this.tabPageGlobal.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(189, 354);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 34;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(19, 354);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(141, 23);
            this.buttonSave.TabIndex = 33;
            this.buttonSave.Text = "&Save Big Table Settings";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "BigTable Name";
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Location = new System.Drawing.Point(102, 18);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(241, 20);
            this.textBoxTableName.TabIndex = 29;
            this.textBoxTableName.TextChanged += new System.EventHandler(this.textBoxTableName_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Index Folder";
            // 
            // textBoxIndexFolder
            // 
            this.textBoxIndexFolder.Location = new System.Drawing.Point(102, 56);
            this.textBoxIndexFolder.Name = "textBoxIndexFolder";
            this.textBoxIndexFolder.Size = new System.Drawing.Size(241, 20);
            this.textBoxIndexFolder.TabIndex = 30;
            // 
            // tabPageTables
            // 
            this.tabPageTables.Controls.Add(this.panel1);
            this.tabPageTables.Controls.Add(this.splitter1);
            this.tabPageTables.Controls.Add(this.listBoxTablets);
            this.tabPageTables.Location = new System.Drawing.Point(4, 22);
            this.tabPageTables.Name = "tabPageTables";
            this.tabPageTables.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTables.Size = new System.Drawing.Size(676, 387);
            this.tabPageTables.TabIndex = 1;
            this.tabPageTables.Text = "Tablets";
            this.tabPageTables.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxBalanceServers);
            this.panel1.Controls.Add(this.buttonDelete);
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(202, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(471, 381);
            this.panel1.TabIndex = 2;
            // 
            // groupBoxBalanceServers
            // 
            this.groupBoxBalanceServers.Controls.Add(this.buttonDeleteBS);
            this.groupBoxBalanceServers.Controls.Add(this.listBoxBlanceServer);
            this.groupBoxBalanceServers.Controls.Add(this.buttonAddBS);
            this.groupBoxBalanceServers.Location = new System.Drawing.Point(27, 14);
            this.groupBoxBalanceServers.Name = "groupBoxBalanceServers";
            this.groupBoxBalanceServers.Size = new System.Drawing.Size(410, 209);
            this.groupBoxBalanceServers.TabIndex = 4;
            this.groupBoxBalanceServers.TabStop = false;
            this.groupBoxBalanceServers.Text = "Balance Servers";
            // 
            // buttonDeleteBS
            // 
            this.buttonDeleteBS.Location = new System.Drawing.Point(329, 58);
            this.buttonDeleteBS.Name = "buttonDeleteBS";
            this.buttonDeleteBS.Size = new System.Drawing.Size(75, 23);
            this.buttonDeleteBS.TabIndex = 7;
            this.buttonDeleteBS.Text = "Delete";
            this.buttonDeleteBS.UseVisualStyleBackColor = true;
            // 
            // listBoxBlanceServer
            // 
            this.listBoxBlanceServer.FormattingEnabled = true;
            this.listBoxBlanceServer.Location = new System.Drawing.Point(6, 17);
            this.listBoxBlanceServer.Name = "listBoxBlanceServer";
            this.listBoxBlanceServer.Size = new System.Drawing.Size(309, 186);
            this.listBoxBlanceServer.TabIndex = 0;
            // 
            // buttonAddBS
            // 
            this.buttonAddBS.Location = new System.Drawing.Point(329, 29);
            this.buttonAddBS.Name = "buttonAddBS";
            this.buttonAddBS.Size = new System.Drawing.Size(75, 23);
            this.buttonAddBS.TabIndex = 5;
            this.buttonAddBS.Text = "Add";
            this.buttonAddBS.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(108, 338);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(27, 338);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(199, 3);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 381);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // listBoxTablets
            // 
            this.listBoxTablets.Dock = System.Windows.Forms.DockStyle.Left;
            this.listBoxTablets.FormattingEnabled = true;
            this.listBoxTablets.Location = new System.Drawing.Point(3, 3);
            this.listBoxTablets.Name = "listBoxTablets";
            this.listBoxTablets.Size = new System.Drawing.Size(196, 381);
            this.listBoxTablets.TabIndex = 0;
            // 
            // BigTableGenerate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Name = "BigTableGenerate";
            this.Size = new System.Drawing.Size(684, 413);
            this.Load += new System.EventHandler(this.BigTableGenerate_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageGlobal.ResumeLayout(false);
            this.tabPageGlobal.PerformLayout();
            this.tabPageTables.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBoxBalanceServers.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageGlobal;
        private System.Windows.Forms.TabPage tabPageTables;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListBox listBoxTablets;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox textBoxIndexFolder;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.GroupBox groupBoxBalanceServers;
        private System.Windows.Forms.ListBox listBoxBlanceServer;
        private System.Windows.Forms.Button buttonDeleteBS;
        private System.Windows.Forms.Button buttonAddBS;
    }
}
